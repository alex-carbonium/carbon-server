using System;
using System.Linq.Expressions;
using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Business.CloudSpecifications
{
    public class FindByRowKey<T> : TableEntitySpecification<T> where T : ITableEntity
    {
        readonly string _partitionKey;
        private readonly string _rowKey;

        public FindByRowKey(string partitionKey, string rowKey)
        {
            _partitionKey = partitionKey;
            _rowKey = rowKey;
        }

        protected override Expression<Func<ITableEntity, bool>> EntityExpression
        {
            get { return x => x.PartitionKey == _partitionKey && x.RowKey == _rowKey; }
        }
    }
}