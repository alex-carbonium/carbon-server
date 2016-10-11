using System.Linq;

namespace Carbon.Framework.Specifications
{
    public abstract class Specification<TEntity> : ISpecification<TEntity>        
    {
        public abstract IQueryable<TEntity> Apply(IQueryable<TEntity> query);
        public bool Cacheable { get; set; }
    }
}
