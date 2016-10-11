using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Carbon.Framework.Specifications;

namespace Carbon.Framework.UnitOfWork
{
    public static class UnitOfWorkExtension
    {
        public static IQueryable<T> FindAll<T>(this IUnitOfWork unitOfWork)
        {
            return unitOfWork.Repository<T>().FindAll();
        }
        public static IQueryable<T> FindAll<T>(this IUnitOfWork unitOfWork, bool cache)
        {
            return unitOfWork.Repository<T>().FindAll(cache);
        }

        public static IQueryable<T> FindAllBy<T>(this IUnitOfWork unitOfWork, ISpecification<T> specification)
        {
            return unitOfWork.Repository<T>().FindAllBy(specification);
        }

        public static T FindSingleBy<T>(this IUnitOfWork unitOfWork, ISpecification<T> specification)
        {
            return unitOfWork.Repository<T>().FindSingleBy(specification);
        }

        public static T FindById<T>(this IUnitOfWork unitOfWork, long key, bool lockForUpdate = false)
        {
            return unitOfWork.Repository<T>().FindById(key, lockForUpdate: lockForUpdate);
        }
        
        public static T FindById<T>(this IUnitOfWork unitOfWork, string key, bool lockForUpdate = false)
        {
            return unitOfWork.Repository<T>().FindById(key, lockForUpdate: lockForUpdate);
        }
        public static async Task<T> FindByIdAsync<T>(this IUnitOfWork unitOfWork, string key, bool lockForUpdate = false)
        {
            return await unitOfWork.Repository<T>().FindByIdAsync(key, lockForUpdate: lockForUpdate);
        }

        public static T FindFirstOnly<T>(this IUnitOfWork unitOfWork)
        {
            return unitOfWork.Repository<T>().FindFirstOnly();
        }

        public static bool Exists<T>(this IUnitOfWork unitOfWork, dynamic key)
        {
            return unitOfWork.Repository<T>().Exists(key);
        }
        public static bool ExistsBy<T>(this IUnitOfWork unitOfWork, ISpecification<T> specification)
        {
            return unitOfWork.Repository<T>().ExistsBy(specification);
        }

        public static void Insert<T>(this IUnitOfWork unitOfWork, T entity)
        {
            unitOfWork.Repository<T>().Insert(entity);
        }

        public static Task InsertAsync<T>(this IUnitOfWork unitOfWork, T entity)
        {
            return unitOfWork.Repository<T>().InsertAsync(entity);
        }

        public static void Update<T>(this IUnitOfWork unitOfWork, T entity)
        {
            unitOfWork.Repository<T>().Update(entity);
        }

        public static void Delete<T>(this IUnitOfWork unitOfWork, T entity)
        {
            unitOfWork.Repository<T>().Delete(entity);
        }
        public static void Delete<T>(this IUnitOfWork unitOfWork, dynamic id)
        {
            unitOfWork.Repository<T>().Delete(id);
        }
        public static void DeleteBy<T>(this IUnitOfWork unitOfWork, ISpecification<T> spec)
        {
            unitOfWork.Repository<T>().DeleteBy(spec);
        }

        public static void InsertOrUpdate<T>(this IUnitOfWork unitOfWork, T entity)
        {
            unitOfWork.Repository<T>().InsertOrUpdate(entity);
        }

        public static void InsertAll<T>(this IUnitOfWork unitOfWork, IEnumerable<T> entities)
        {
            unitOfWork.Repository<T>().InsertAll(entities);
        }

        public static void UpdateAll<T>(this IUnitOfWork unitOfWork, IEnumerable<T> entities)
        {
            unitOfWork.Repository<T>().UpdateAll(entities);
        }

        public static void DeleteAll<T>(this IUnitOfWork unitOfWork, IEnumerable<T> entities)
        {
            unitOfWork.Repository<T>().DeleteAll(entities);
        }

        public static void Lock<T>(this IUnitOfWork unitOfWork, long id)
        {
            unitOfWork.Repository<T>().Lock(id);
        }

        public static void DeleteAll<T>(this IUnitOfWork unitOfWork)
        {
            unitOfWork.Repository<T>().DeleteAll();
        }
    }
}
