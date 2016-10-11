using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Carbon.Framework.Cloud
{
    public class KeysSpecification<TEntity> : ITableRepositorySpecification<TEntity>
        where TEntity : ITableEntity, new()
    {
        string _partitionKey;
        string _rowKey;
        public KeysSpecification(string partitionKey, string rowKey)
        {
            _partitionKey = partitionKey;
            _rowKey = rowKey;
        }

        public void Apply(TableQuery<TEntity> query)
        {
            query.Where(TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _partitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, _rowKey)));
        }
    }
}
