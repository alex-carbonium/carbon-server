using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Carbon.Framework.Specifications;

namespace Carbon.Framework.Repositories
{
    public class InMemoryRepository<T> : Repository<T>
    {
        public static long IdCounter = 0;

        private readonly HashSet<dynamic> _inserted = new HashSet<dynamic>();
        private readonly Dictionary<dynamic, T> _originals = new Dictionary<dynamic, T>();
        private readonly HashSet<dynamic> _deleted = new HashSet<dynamic>();

        public InMemoryRepository()
        {
            Store = new List<T>();
        }

        public List<T> Store { get; private set; }

        public override IQueryable<T> FindAll(bool cache = false)
        {
            return Store.ToList().AsQueryable();
        }

        public override IQueryable<T> FindAllBy(ISpecification<T> specification)
        {
            return specification.Apply(Store.AsQueryable()).ToList().AsQueryable();
        }

        public override T FindById(dynamic id, bool lockForUpdate = false)
        {
            return Store.Cast<dynamic>().SingleOrDefault(x => x.Id == id);
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
            dynamic domainObject = entity;
            if (domainObject.GetType().GetProperty("Id", typeof (long)) != null && domainObject.Id == 0)
            {
                domainObject.Id = Interlocked.Increment(ref IdCounter);
            }
            Store.Add(entity);
            if (Inserted != null)
            {
                Inserted(entity);
            }
        }

        public override void Update(T entity)
        {
            if (!Store.Contains(entity))
            {
                Store.Add(entity);
            }
            if (Updated != null)
            {
                Updated(entity);
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
            if (Deleted != null)
            {
                Deleted(entity);
            }

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

        public override void DeleteAll()
        {
            Store.Clear();            
        }

        public override void Delete(dynamic id)
        {
            Delete(FindById(id));
        }

        public override void Lock(dynamic id)
        {
        }

        public IQueryable<T> AsQueryable()
        {
            return Store.AsQueryable();
        }

        public event Action<object> Updated;
        public event Action<object> Deleted;
        public event Action<object> Inserted;

        public void EnableChangeTracking()
        {
            Inserted += o => _inserted.Add(o);
            Deleted += o => _deleted.Add(o);
            foreach (var entity in Store)
            {
                var original = Clone(entity);
                _originals[entity] = original;    
            }
        }

        public override void Commit()
        {
            base.Commit();

            _inserted.Clear();
            _deleted.Clear();
            _originals.Clear();
        }

        public override void Rollback()
        {
            base.Rollback();

            foreach (dynamic entity in _inserted)
            {
                Store.Remove(entity);
            }
            foreach (dynamic entity in _deleted)
            {
                Store.Add(entity);
            }
            foreach (var kv in _originals)
            {
                Populate(kv.Value, kv.Key);
            }
        }

        private static dynamic Clone(dynamic obj)
        {
            return typeof(object).GetMethod("MemberwiseClone",
                BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic)
                .Invoke(obj, new object[] { });
        }

        private static void Populate(dynamic source, dynamic destination)
        {
            var destProperties = destination.GetType().GetProperties();
            foreach (var sourceProperty in source.GetType().GetProperties())
            {
                foreach (var destProperty in destProperties)
                {
                    if (destProperty.Name == sourceProperty.Name)
                    {
                        destProperty.SetValue(destination, sourceProperty.GetValue(
                            source, new object[] { }), new object[] { });
                        break;
                    }
                }
            }
        }
    }
}