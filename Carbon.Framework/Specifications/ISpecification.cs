using System.Linq;

namespace Carbon.Framework.Specifications
{
    public interface ISpecification<TEntity>        
    {
        IQueryable<TEntity> Apply(IQueryable<TEntity> query);

        bool Cacheable { get; set; }
    }
}
