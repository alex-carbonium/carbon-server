using System;
using System.Linq;
using System.Linq.Expressions;
using Carbon.Framework.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Business.CloudSpecifications
{
    public abstract class TableEntitySpecification<T> : PredicateSpecification<T>, ITableEntitySpecification 
        where T : ITableEntity
    {
        protected sealed override Expression<Func<T, bool>> Expression
        {
            get { throw new NotSupportedException("Use EntityExpression instead"); }
        }

        protected abstract Expression<Func<ITableEntity, bool>> EntityExpression { get; }

        public override IQueryable<T> Apply(IQueryable<T> query)
        {
            return Apply(query.Cast<ITableEntity>()).Cast<T>();
        }

        public IQueryable<ITableEntity> Apply(IQueryable<ITableEntity> query)
        {            
            return query.Where(EntityExpression);
        }
    }
}
