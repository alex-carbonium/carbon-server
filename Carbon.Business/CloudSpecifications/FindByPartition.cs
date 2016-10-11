using System;
using System.Linq.Expressions;
using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Business.CloudSpecifications
{
    public class FindByPartition<T> : TableEntitySpecification<T> where T : ITableEntity
    {
        private readonly string _partitionKey;

        public FindByPartition(string partitionKey)
        {
            _partitionKey = partitionKey;
        }

        protected override Expression<Func<ITableEntity, bool>> EntityExpression
        {
            get { return x => x.PartitionKey == _partitionKey; }
        }
    }
}
