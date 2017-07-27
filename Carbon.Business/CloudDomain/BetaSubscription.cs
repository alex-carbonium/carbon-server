using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carbon.Business.CloudDomain
{
    public class BetaSubscription : TableEntity
    {
        public BetaSubscription()
        {

        }

        public BetaSubscription(string email)
        {
            this.PartitionKey = "beta";
            this.RowKey = email;
            this.Created = DateTime.Now;
        }

        public DateTime Created { get; set; }
    }
}
