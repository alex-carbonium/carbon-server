using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Carbon.Framework.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Business.CloudSpecifications
{
    public class FindByExpression<T> : PredicateSpecification<T> where T : ITableEntity
    {
        private readonly Expression<Func<T, bool>> _expression;

        public FindByExpression(Expression<Func<T, bool>> expression)
        {
            _expression = expression;
        }

        protected override Expression<Func<T, bool>> Expression => _expression;
    }
}
