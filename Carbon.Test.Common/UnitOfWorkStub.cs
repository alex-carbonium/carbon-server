using System.Collections.Generic;
using System.Linq;
using Carbon.Framework.Repositories;
using Carbon.Framework.UnitOfWork;

namespace Carbon.Test.Common
{
    public class UnitOfWorkStub : IUnitOfWork
    {
        private readonly Dictionary<string, IRepository> _repositories = new Dictionary<string, IRepository>();

        public Dictionary<string, object> AllRepositories { get; set; }

        public void Dispose()
        {           

        }

        public IRepository<TEntity> Repository<TEntity>()
        {
            return InMemRepositoryRepository<TEntity>();
        }

        public InMemoryRepository<TEntity> InMemRepositoryRepository<TEntity>()            
        {
            object repository;
            var key = typeof (TEntity).FullName;
            if (!AllRepositories.TryGetValue(key, out repository))
            {
                var repo = new InMemoryRepository<TEntity>();
                RegisterRepository(repo);

                repository = repo;

            }
            return (InMemoryRepository<TEntity>) repository;
        }

        public void RegisterRepository<T>(InMemoryRepository<T> repo)
        {
            var key = typeof(T).FullName;
            repo.EnableChangeTracking();
            AllRepositories.Add(key, repo);
            _repositories.Add(key, repo);
        }

        public void Flush()
        {
        }
        public void Commit()
        {
            foreach (var repository in _repositories.Values)
            {
                repository.Commit();
            }
        }

        public void Rollback()
        {
            foreach (var repository in _repositories.Values)
            {
                repository.Rollback();
            }
        }

        public bool IsDisposed { get; private set; }
        
        public long ExecuteDirect(string code, IDictionary<string, object> parameters)
        {
            return 0;
        }

        public IQueryable<TEntity> Queryable<TEntity>()
        {
            return InMemRepositoryRepository<TEntity>().AsQueryable();
        }
    }
}
