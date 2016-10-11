using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Carbon.Framework.Repositories;
using Carbon.Framework.Specifications;
using Microsoft.WindowsAzure.Storage.Table;
using Carbon.Business.Exceptions;

namespace Carbon.Data.Azure.Table
{
    public class TableRepository<T> : Repository<T>
        where T : ITableEntity, new()
    {
        private readonly CloudTableClient _client;
        private readonly ConcurrentDictionary<Type, CloudTable> _tables = new ConcurrentDictionary<Type, CloudTable>();

        public TableRepository(CloudTableClient client)
        {
            _client = client;
        }

        public override IQueryable<T> FindAll(bool cache = false)
        {            
            return CreateQuery();
        }        

        public override IQueryable<T> FindAllBy(ISpecification<T> specification)
        {
            var q = CreateQuery();
            q = (TableQuery<T>)specification.Apply(q);
            return GetOrCreateTable().ExecuteQuery(q).AsQueryable();
        }

        public override async Task<IQueryable<T>> FindAllByAsync(ISpecification<T> spec)
        {
            var q = CreateQuery();
            q = (TableQuery<T>) spec.Apply(q);
            return (await GetOrCreateTable().ExecuteQueryAsync(q)).AsQueryable();
        }

        public override T FindSingleBy(ISpecification<T> specification)
        {
            return FindAllBy(specification).SingleOrDefault();
        }

        public override async Task<T> FindSingleByAsync(ISpecification<T> spec)
        {
            return (await FindAllByAsync(spec)).SingleOrDefault();
        }

        public override T FindById(dynamic key, bool lockForUpdate = false)
        {
            var tableResult = GetOrCreateTable().Execute(TableOperation.Retrieve<T>(key.PartitionKey, key.RowKey));
            return (T)tableResult.Result;
        }

        public override async Task<T> FindByIdAsync(dynamic key, bool lockForUpdate = false)
        {
            var tableResult = await GetOrCreateTable().ExecuteAsync(TableOperation.Retrieve<T>(key.PartitionKey, key.RowKey));
            return (T)tableResult.Result;
        }

        public override T FindFirstOnly()
        {
            return CreateQuery().FirstOrDefault();
        }        

        public override bool ExistsBy(ISpecification<T> specification)
        {
            throw new NotImplementedException();
        }

        public override void Insert(T entity)
        {
            GetOrCreateTable().WithConflictHandler(t => t.Execute(TableOperation.Insert(entity)));
        }

        public override async Task InsertAsync(T entity)
        {
            await GetOrCreateTable().WithConflictHandlerAsync(t => t.ExecuteAsync(TableOperation.Insert(entity)));
        }

        public override void InsertOrUpdate(T entity)
        {
            GetOrCreateTable().Execute(TableOperation.InsertOrReplace(entity));
        }

        public override void InsertAll(IEnumerable<T> entities)
        {
            var operation = new TableBatchOperation();
            foreach (var entity in entities)
            {
                operation.Insert(entity);
            }
            if (operation.Count > 0)
            {
                GetOrCreateTable().ExecuteBatch(operation);    
            }            
        }

        public override void Update(T entity)
        {
            GetOrCreateTable().WithOptimisticLock(t => t.Execute(TableOperation.Replace(entity)));
        }

        public override async Task UpdateAsync(T entity)
        {
            await GetOrCreateTable().WithOptimisticLockAsync(t => t.ExecuteAsync(TableOperation.Replace(entity)));
        }

        public override void UpdateAll(IEnumerable<T> entities)
        {
            var operation = new TableBatchOperation();
            foreach (var entity in entities)
            {
                operation.Replace(entity);
            }
            if (operation.Count > 0)
            {
                GetOrCreateTable().WithOptimisticLock(t => t.ExecuteBatch(operation));
            }
        }

        public override void Delete(T entity)
        {
            GetOrCreateTable().Execute(TableOperation.Delete(entity));
        }

        public override void DeleteAll(IEnumerable<T> entities)
        {
            var operation = new TableBatchOperation();
            foreach (var entity in entities)
            {
                operation.Delete(entity);
            }
            if (operation.Count > 0)
            {
                GetOrCreateTable().ExecuteBatch(operation);    
            }
        }

        public override void Delete(dynamic id)
        {
            var entity = FindById(id);
            if (entity != null)
            {
                GetOrCreateTable().Execute(TableOperation.Delete(entity));
            }
        }

        public override void Lock(dynamic id)
        {
            throw new NotImplementedException();
        }

        private TableQuery<T> CreateQuery()
        {
            return GetOrCreateTable().CreateQuery<T>();
        }

        private CloudTable GetOrCreateTable()
        {
            return _tables.GetOrAdd(typeof(T), t =>
            {
                var table = _client.GetTableReference(t.Name);
                table.CreateIfNotExists();
                return table;
            });
        }
    }
}
