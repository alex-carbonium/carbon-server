using System;
using System.Linq;
using System.Linq.Expressions;

namespace Carbon.Framework.Specifications
{
    public abstract class PredicateSpecification<TEntity> : Specification<TEntity>        
    {
        protected abstract Expression<Func<TEntity, bool>> Expression { get; }

        public override IQueryable<TEntity> Apply(IQueryable<TEntity> query)
        {
            return query.Where(Expression);
        }

        public bool IsSatisfiedBy(TEntity entity)
        {
            return Expression.Compile()(entity);
        }
    }
}
