using System.Collections.Generic;
using System.Linq;
using Sketch.Framework.Models;
using Sketch.Framework.Repositories;
using Sketch.Framework.Specifications;

namespace Sketch.Test.Unit
{
    public class InMemoryRepository<T> : Repository<T> where T : class, IDomainObject
    {
        public InMemoryRepository()
        {
            Store = new List<T>();            
        }

        public List<T> Store { get; private set; }

        public override IQueryable<T> FindAll()
        {
            return Store.AsQueryable();
        }

        public override IQueryable<T> FindAllBy(ISpecification<T> specification)
        {
            return specification.Apply(Store.AsQueryable());
        }

        public override T FindById(long id, bool lockForUpdate = false)
        {
            return Store.SingleOrDefault(x => x.Id == id);
        }

        public override T FindSingleBy(ISpecification<T> specification)
        {
            return specification.Apply(Store.AsQueryable()).SingleOrDefault();
        }

        public override T FindFirstOnly()
        {
            return Store.FirstOrDefault();
        }

        public override bool ExistsBy(ISpecification<T> specification)
        {
            return specification.Apply(Store.AsQueryable()).FirstOrDefault() != null;
        }

        public override void Insert(T entity)
        {
            Store.Add(entity);
        }

        public override void Update(T entity)
        {
            if (!Store.Contains(entity))
            {
                Store.Add(entity);
            }
        }

        public override void InsertOrUpdate(T entity)
        {
            if (!Store.Contains(entity))
                Insert(entity);
        }

        public override void Delete(T entity)
        {
            if (Store.Contains(entity))
                Store.Remove(entity);
        }

        public void Delete(int id)
        {
            var entity = FindById(id);
            if (entity != null)
                Delete(entity);
        }

        public void DeleteBy(PredicateSpecification<T> specification)
        {
            foreach (var entity in FindAllBy(specification))
            {
                Delete(entity);
            }
        }

        public void DeleteAll()
        {
            Store.Clear();
        }

        public override void Delete(long id)
        {
            Delete(FindById(id));
        }
    }
}