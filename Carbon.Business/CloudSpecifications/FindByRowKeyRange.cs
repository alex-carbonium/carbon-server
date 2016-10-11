using System;
using System.Linq.Expressions;
using Microsoft.WindowsAzure.Storage.Table;
namespace Carbon.Business.CloudSpecifications
{
    public class FindByRowKeyRange<T> : TableEntitySpecification<T> where T : ITableEntity
    {
        readonly string _partitionKey;
        private readonly string _fromKey;
        private readonly string _toKey;

        public FindByRowKeyRange(string partitionKey, string fromKey = null, string toKey = null)
        {
            _partitionKey = partitionKey;
            _fromKey = fromKey;
            _toKey = toKey;
        }

        protected override Expression<Func<ITableEntity, bool>> EntityExpression
        {
            get
            {
                if (string.IsNullOrEmpty(_fromKey) && string.IsNullOrEmpty(_toKey))
                {
                    return x => x.PartitionKey == _partitionKey;
                }
                if (!string.IsNullOrEmpty(_fromKey) && !string.IsNullOrEmpty(_toKey))
                {
                    return x => x.PartitionKey == _partitionKey && x.RowKey.CompareTo(_fromKey) >= 0 && x.RowKey.CompareTo(_toKey) <= 0;
                }
                if (string.IsNullOrEmpty(_fromKey))
                {
                    return x => x.PartitionKey == _partitionKey && x.RowKey.CompareTo(_toKey) <= 0;
                }
                return x => x.PartitionKey == _partitionKey && x.RowKey.CompareTo(_fromKey) >= 0;
            }
        }        
    }
}