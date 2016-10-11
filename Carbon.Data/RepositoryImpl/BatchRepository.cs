using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Carbon.Framework.Repositories;
using Carbon.Framework.Specifications;

namespace Carbon.Data.RepositoryImpl
{
    public class BatchRepository<T> : Repository<T>
    {
        private readonly IRepository<T> _repository;
        private readonly int _flushLimit;
        private readonly ConcurrentDictionary<T, bool> _insertList = new ConcurrentDictionary<T, bool>();
        private readonly ConcurrentDictionary<T, bool> _insertOrUpdateList = new ConcurrentDictionary<T, bool>();
        private readonly ConcurrentDictionary<T, bool> _updateList = new ConcurrentDictionary<T, bool>();
        private readonly ConcurrentDictionary<T, bool> _deleteList = new ConcurrentDictionary<T, bool>();        

        public BatchRepository(IRepository<T> repository, int flushLimit)
        {
            _repository = repository;
            _flushLimit = flushLimit;
            BatchRepositories.All.Add(this);
        }

        public override IQueryable<T> FindAll(bool cache)
        {
            return _repository.FindAll(cache);
        }

        public override IQueryable<T> FindAllBy(ISpecification<T> specification)
        {
            return _repository.FindAllBy(specification);
        }

        public override T FindSingleBy(ISpecification<T> specification)
        {
            return _repository.FindSingleBy(specification);
        }

        public override T FindById(dynamic key, bool lockForUpdate = false)
        {
            return _repository.FindById(key, lockForUpdate);
        }

        public override T FindFirstOnly()
        {
            return _repository.FindFirstOnly();
        }

        public override bool ExistsBy(ISpecification<T> specification)
        {
            return _repository.ExistsBy(specification);
        }

        public override void Insert(T entity)
        {
            _insertList.GetOrAdd(entity, e => true);
            if (_insertList.Count >= _flushLimit)
            {
                _repository.InsertAll(RetrieveBatch(_insertList));
            }
        }

        public override void Update(T entity)
        {
            _updateList.GetOrAdd(entity, e => true);
            if (_updateList.Count >= _flushLimit)
            {
                _repository.UpdateAll(RetrieveBatch(_updateList));
            }
        }

        public override void Delete(T entity)
        {
            _deleteList.GetOrAdd(entity, e => true);
            if (_deleteList.Count >= _flushLimit)
            {
                _repository.DeleteAll(RetrieveBatch(_deleteList));
            }
        }

        public override void InsertOrUpdate(T entity)
        {
            _insertOrUpdateList.GetOrAdd(entity, e => true);
            if (_insertOrUpdateList.Count >= _flushLimit)
            {
                _repository.InsertOrUpdateAll(RetrieveBatch(_insertOrUpdateList));
            }
        }

        public override void Flush()
        {
            IList<T> batch;
            do
            {
                batch = RetrieveBatch(_insertList);
                _repository.InsertAll(batch);
            } while (batch.Count > 0);

            do
            {
                batch = RetrieveBatch(_updateList);
                _repository.UpdateAll(batch);
            } while (batch.Count > 0);

            do
            {
                batch = RetrieveBatch(_deleteList);
                _repository.DeleteAll(batch);
            } while (batch.Count > 0);

            do
            {
                batch = RetrieveBatch(_insertOrUpdateList);
                _repository.InsertOrUpdateAll(batch);
            } while (batch.Count > 0);                        
        }

        public override void Delete(dynamic id)
        {
            Delete(FindById(id));
        }

        public override void Lock(dynamic id)
        {
            _repository.Lock(id);
        }

        private IList<T> RetrieveBatch(ConcurrentDictionary<T, bool> dict)
        {
            var keys = dict.Keys.Take(_flushLimit).ToArray();
            foreach (var key in keys)
            {
                bool value;
                dict.TryRemove(key, out value);
            }
            return keys;
        }

        public override void Dispose()
        {
            Flush();
            base.Dispose();
        }
    }
}
