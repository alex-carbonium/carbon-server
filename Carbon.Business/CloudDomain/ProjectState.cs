using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Business.CloudDomain
{
    public class ProjectState : TableEntity
    {
        public ProjectState()
        {
        }
        public ProjectState(string userId, string projectId)
        {
            PartitionKey = userId;
            RowKey = projectId;
        }

        public string InitialVersion { get; set; }
        public string EditVersion { get; set; }
        public long TimesSaved { get; set; }
    }
}