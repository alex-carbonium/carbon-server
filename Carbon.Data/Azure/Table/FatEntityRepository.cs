using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Carbon.Business;
using Carbon.Business.CloudSpecifications;
using Carbon.Framework.Repositories;
using Carbon.Framework.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Data.Azure.Table
{
    public class FatEntityRepository<T> : Repository<T>
        where T : ITableEntity, IPipe<string>, new()
    {
        public const int MaxBatchPayload = 1024 * 1024 * 3;
        public const int MaxBatchOperations = 100;

        private readonly ConcurrentDictionary<Type, CloudTable> _tables = new ConcurrentDictionary<Type, CloudTable>();

        private readonly CloudTableClient _client;

        public string CustomTableName { get; set; }

        public FatEntityRepository(CloudTableClient client)
        {
            _client = client;
        }

        public override IQueryable<T> FindAll(bool cache = false)
        {
            return TransformResults(CreateQuery());
        }

        public override IQueryable<T> FindAllBy(ISpecification<T> specification)
        {
            var tableSpec = specification as ITableEntitySpecification;
            if (tableSpec == null)
            {
                throw new NotSupportedException(string.Format("Fat repository supports only table entityt specification. Requested type was {0}", specification.GetType().FullName));
            }
            var q = CreateQuery();
            q = (TableQuery<DynamicTableEntity>)tableSpec.Apply(q).Cast<DynamicTableEntity>();
            return TransformResults(GetOrCreateTable().ExecuteQuery(q));
        }

        public override async Task<IQueryable<T>> FindAllByAsync(ISpecification<T> specification)
        {
            var tableSpec = specification as ITableEntitySpecification;
            if (tableSpec == null)
            {
                throw new NotSupportedException(string.Format("Fat repository supports only table entityt specification. Requested type was {0}", specification.GetType().FullName));
            }
            var q = CreateQuery();
            q = (TableQuery<DynamicTableEntity>) tableSpec.Apply(q).Cast<DynamicTableEntity>();
            return TransformResults(await GetOrCreateTable().ExecuteQueryAsync(q));
        }

        public override T FindSingleBy(ISpecification<T> specification)
        {
            return FindAllBy(specification).SingleOrDefault();
        }

        public override T FindById(dynamic key, bool lockForUpdate = false)
        {
            throw new NotSupportedException("Use FindAllBy with a key range");
        }

        public override T FindFirstOnly()
        {
            throw new NotImplementedException();
        }

        public override bool ExistsBy(ISpecification<T> specification)
        {
            throw new NotImplementedException();
        }

        public override void Insert(T entity)
        {
            var fatEntities = SliceObject(entity);
#pragma warning disable 4014
            ExecuteBatch(fatEntities, (operation, fatEntity) => operation.Insert(fatEntity.WrappedEntity), async: false);
#pragma warning restore 4014
        }

        public override async Task InsertAsync(T entity)
        {
            var fatEntities = SliceObject(entity);
            await ExecuteBatch(fatEntities, (operation, fatEntity) => operation.Insert(fatEntity.WrappedEntity), async: true);
        }

        public override void Update(T entity)
        {
            throw new NotImplementedException();
        }

        public override void Delete(T entity)
        {
            throw new NotImplementedException();
        }

        public override void InsertOrUpdate(T entity)
        {
            throw new NotImplementedException();
        }

        public override void Delete(dynamic id)
        {
            throw new NotImplementedException();
        }

        public override void Lock(dynamic id)
        {
            throw new NotImplementedException();
        }

        public string GetTableName()
        {
            return CustomTableName ?? typeof (T).Name;
        }

        public async Task ExecuteBatch(IEnumerable<FatEntity> fatEntities, Action<TableBatchOperation, FatEntity> action, bool async)
        {
            var currentOperation = new TableBatchOperation();
            var currentPayload = 0;
            foreach (var fatEntity in fatEntities)
            {
                var entityPayload = fatEntity.GetPayloadSize();
                var potentialPayload = currentPayload + entityPayload;
                if (currentOperation.Count == MaxBatchOperations || potentialPayload >= MaxBatchPayload)
                {
                    if (async)
                    {
                        await GetOrCreateTable().ExecuteBatchAsync(currentOperation);
                    }
                    else
                    {
                        GetOrCreateTable().ExecuteBatch(currentOperation);
                    }

                    currentOperation = new TableBatchOperation();
                    currentPayload = 0;
                }
                action(currentOperation, fatEntity);
                currentPayload += entityPayload;
            }

            if (currentOperation.Count > 0)
            {
                if (async)
                {
                    await GetOrCreateTable().ExecuteBatchAsync(currentOperation);
                }
                else
                {
                    GetOrCreateTable().ExecuteBatch(currentOperation);
                }
            }
        }

        public static IEnumerable<FatEntity> SliceObject(T obj)
        {
            int count;
            int max;
            int total;
            obj.GetStatistics(out count, out max, out total);

            if (FatEntity.CanUseSimpleFormat(count, max, total))
            {
                var fatEntity = new FatEntity(obj.PartitionKey, $"{obj.RowKey}_000");
                fatEntity.FillSimple(obj.Write());
                yield return fatEntity;
            }
            else
            {
                foreach (var fatEntity in SliceComplex(obj))
                {
                    yield return fatEntity;
                }
            }
        }

        private static IEnumerable<FatEntity> SliceComplex(T obj)
        {
            var buffers = obj.Write();
            var entityIndex = 0;
            FatEntity fatEntity = null;
            foreach (var s in buffers)
            {
                var buffer = Framework.Defs.Encoding.GetBytes(s);
                var index = 0;
                do
                {
                    if (fatEntity == null)
                    {
                        fatEntity = new FatEntity(obj.PartitionKey, $"{obj.RowKey}_{entityIndex:D3}");
                        ++entityIndex;
                    }
                    var full = !fatEntity.Fill(buffer, ref index);
                    if (full)
                    {
                        fatEntity.Flush(true);
                        yield return fatEntity;
                        fatEntity = null;
                    }
                } while (index < buffer.Length);
            }

            if (fatEntity != null)
            {
                fatEntity.Flush(true);
                yield return fatEntity;
            }
        }

        public static T CombineFatEntities(IList<DynamicTableEntity> entities, string partitionKey, string rowKey)
        {
            var payload = FatEntity.GetPayload(entities);

            var entity = new T();
            entity.PartitionKey = partitionKey;
            entity.RowKey = rowKey;
            entity.Read(payload);
            return entity;
        }

        private IQueryable<T> TransformResults(IEnumerable<DynamicTableEntity> entities)
        {
            var result = new List<T>();
            string lastRowKey = null, lastPartitionKey = null;
            var fatEntities = new List<DynamicTableEntity>();
            foreach (var entity in entities)
            {
                var lastUnderscore = entity.RowKey.LastIndexOf("_");
                var realKey = entity.RowKey.Substring(0, lastUnderscore);

                if (lastPartitionKey != null && lastPartitionKey != entity.PartitionKey
                    || lastRowKey != null && lastRowKey != realKey)
                {
                    var realEntity = CombineFatEntities(fatEntities, entity.PartitionKey, realKey);
                    result.Add(realEntity);
                    fatEntities.Clear();
                }

                fatEntities.Add(entity);
                lastPartitionKey = entity.PartitionKey;
                lastRowKey = realKey;
            }

            if (fatEntities.Count > 0)
            {
                result.Add(CombineFatEntities(fatEntities, lastPartitionKey, lastRowKey));
            }

            return result.AsQueryable();
        }

        private CloudTable GetOrCreateTable()
        {
            return _tables.GetOrAdd(typeof (T), t =>
            {
                var table = _client.GetTableReference(GetTableName());
                table.CreateIfNotExists();
                return table;
            });
        }

        private TableQuery<DynamicTableEntity> CreateQuery()
        {
            return GetOrCreateTable().CreateQuery<DynamicTableEntity>();
        }
    }
}
