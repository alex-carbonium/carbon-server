using System;

namespace Carbon.Framework.JobScheduling
{
    public class JobInfo
    {
        public string JobId { get; set; }        
        public string JobType { get; set; }
        public string Parameters { get; set; }
        public DateTime FireTime { get; set; }
    }
}