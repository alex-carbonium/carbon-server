using System;
using Microsoft.WindowsAzure.Storage;

namespace Carbon.Data.Azure.Scheduler
{
    public class SchedulerOptions
    {
        public SchedulerOptions()
        {
            QueueName = "tasks";
            MessageBatchSize = 32;
            CloudServiceName = "SchedulerService";
            JobCollectionName = "DefaultJobCollection";
            InvisibilityTimeout = TimeSpan.FromMinutes(30);
        }

        public CloudStorageAccount StorageAccount { get; set; }
        public string QueueName { get; set; }
        public int MessageBatchSize { get; set; }
        public TimeSpan InvisibilityTimeout { get; set; }
        public string CloudServiceName { get; set; }        
        public string JobCollectionName { get; set; }        
        public string PublishSettingsFile { get; set; }

        public Action<Exception> LogException { get; set; }
    }
}
