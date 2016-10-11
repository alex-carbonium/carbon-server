using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Carbon.Framework.Specifications;

namespace Carbon.Framework.Repositories
{
    public abstract class Repository<T> : IRepository<T>        
    {
        public abstract IQueryable<T> FindAll(bool cache = false);
        public abstract IQueryable<T> FindAllBy(ISpecification<T> specification);
        public abstract T FindSingleBy(ISpecification<T> specification);
        public abstract T FindById(dynamic key, bool lockForUpdate = false);
        
        public abstract T FindFirstOnly();

        public virtual bool Exists(dynamic key)
        {
            return FindById(key);
        }

        public abstract bool ExistsBy(ISpecification<T> specification);
        public abstract void Insert(T entity);        
        public abstract void Update(T entity);
        public abstract void Delete(T entity);
        public abstract void InsertOrUpdate(T entity);
        public abstract void Delete(dynamic id);
        public abstract void Lock(dynamic id);

        public virtual Task<IQueryable<T>> FindAllAsync()
        {
            return Task.FromResult(FindAll());
        }

        public virtual Task<T> FindByIdAsync(dynamic key, bool lockForUpdate = false)
        {
            return Task.FromResult<T>(FindById(key, lockForUpdate));
        }
        public virtual Task<IQueryable<T>> FindAllByAsync(ISpecification<T> spec)
        {
            return Task.FromResult(FindAllBy(spec));
        }

        public virtual Task<T> FindSingleByAsync(ISpecification<T> spec)
        {
            return Task.FromResult(FindSingleBy(spec)); 
        }

        public virtual Task InsertAsync(T entity)
        {
            Insert(entity);
            return Task.FromResult(0);
        }
        public virtual Task UpdateAsync(T entity)
        {
            Update(entity);
            return Task.FromResult(0);
        }
        public virtual Task DeleteAsync(T entity)
        {
            Delete(entity);
            return Task.FromResult(0);
        }

        public virtual Task DeleteAsync(dynamic id)
        {
            Delete(id);
            return Task.FromResult(0);
        }

        public virtual void Flush()
        {            
        }

        public virtual void InsertAll(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                Insert(entity);
            }
        }

        public virtual void InsertOrUpdateAll(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                InsertOrUpdate(entity);
            }    
        }

        public virtual void UpdateAll(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                Update(entity);
            }
        }

        public virtual void DeleteAll(IEnumerable<T> entities)
        {
            foreach (var instance in entities)
            {
                Delete(instance);
            }
        }

        public virtual void DeleteAll()
        {            
        }

        public virtual void DeleteBy(ISpecification<T> spec)
        {
            DeleteAll(FindAllBy(spec));
        }

        public virtual void Commit()
        {
        }
        public virtual void Rollback()
        {
        }
        public virtual void Dispose()
        {
        }        
    }
}