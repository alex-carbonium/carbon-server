using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Business.CloudDomain
{
    public class FeatureSubscription : TableEntity
    {
        public FeatureSubscription()
        {
            
        }

        public FeatureSubscription(string companyId, string projectId, string feature)
        {
            this.CompanyId = companyId;
            this.ProjectId = projectId;
            this.Feature = feature;
            this.PartitionKey = feature;
            this.RowKey = companyId + ":" + projectId;
        }
        public string CompanyId { get; set; }
        public string ProjectId { get; set; }
        public string Feature { get; set; }
    }
}
