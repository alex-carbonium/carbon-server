using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Carbon.Framework.Cloud
{
    public class PartitionKeySpecification<TEntity> : ITableRepositorySpecification<TEntity>
        where TEntity : ITableEntity, new()
    {
        string _partitionKey;
        public PartitionKeySpecification(string partitionKey)
        {
            _partitionKey = partitionKey;
        }

        public void Apply(TableQuery<TEntity> query)
        {
            query.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _partitionKey));
        }
    }
}
