using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Carbon.Framework.Specifications;

namespace Carbon.Framework.Repositories
{
    public interface IRepository : IDisposable
    {
        void Commit();
        void Rollback();
        void Flush();
    }
    public interface IRepository<TEntity> : IRepository
    {
        IQueryable<TEntity> FindAll(bool cache = false);
        IQueryable<TEntity> FindAllBy(ISpecification<TEntity> specification);

        TEntity FindSingleBy(ISpecification<TEntity> specification);
        TEntity FindById(dynamic key, bool lockForUpdate = false);
        TEntity FindFirstOnly();

        bool Exists(dynamic key);
        bool ExistsBy(ISpecification<TEntity> specification);

        void Insert(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        void InsertOrUpdate(TEntity entity);
        void Delete(dynamic id);
        void Lock(dynamic id);

        Task<TEntity> FindByIdAsync(dynamic key, bool lockForUpdate = false);
        Task<IQueryable<TEntity>> FindAllByAsync(ISpecification<TEntity> spec);
        Task<TEntity> FindSingleByAsync(ISpecification<TEntity> spec);
        Task InsertAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TEntity entity);
        Task DeleteAsync(dynamic id);

        void InsertAll(IEnumerable<TEntity> entities);
        void InsertOrUpdateAll(IEnumerable<TEntity> entities);
        void UpdateAll(IEnumerable<TEntity> entities);
        void DeleteAll(IEnumerable<TEntity> entities);
        void DeleteAll();
        void DeleteBy(ISpecification<TEntity> spec);
        Task<IQueryable<TEntity>> FindAllAsync();
    }
}
