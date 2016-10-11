using System;
using Carbon.Framework.Models;

namespace Carbon.Framework.Specifications
{
    public class IdSpecification <TEntity> : PredicateSpecification<TEntity>
        where TEntity : DomainObject
    {
        private readonly long _key;
        
        public IdSpecification(long key)
        {
            _key = key;
        }        

        protected override System.Linq.Expressions.Expression<Func<TEntity, bool>> Expression
        {
            get 
            {
                return e => e.Id == _key;
            }
        }       
    }
}
