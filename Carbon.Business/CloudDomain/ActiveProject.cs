using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Business.CloudDomain
{
    public class ActiveProject : TableEntity
    {
        public ActiveProject()
        {
        }

        public ActiveProject(string companyId, string projectId, string machineName)
        {
            PartitionKey = companyId;
            RowKey = projectId;
            MachineName = machineName;
            Activated = LastAccessed = DateTime.UtcNow;
        }

        public string MachineName { get; set; }
        public DateTime Activated { get; set; }
        public DateTime LastAccessed { get; set; }
    }
}