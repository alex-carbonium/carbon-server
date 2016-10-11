using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Framework.Cloud;

namespace Carbon.Business.CloudDomain.Specifications
{
    public class ProjectTypeSpecification : ITableRepositorySpecification<PublicResource>
    {
        private string _projectType;
        private string _partitionKey;
        private string _theme;
        public ProjectTypeSpecification(string partitionKey, string projectType, string theme)
        {
            _projectType = projectType;
            _partitionKey = partitionKey;
            _theme = theme;
        }
        public void Apply(Microsoft.WindowsAzure.Storage.Table.TableQuery<PublicResource> query)
        {
            query.Where(
                TableQuery.CombineFilters(
                    TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _partitionKey),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("ProjectType", QueryComparisons.Equal, _projectType)),
                    TableOperators.And,
                     TableQuery.GenerateFilterCondition("Theme", QueryComparisons.Equal, _theme)));

        }
    }
}
