using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Carbon.Framework.JobScheduling
{
    public interface IJobScheduler
    {
        void ScheduleImmediately(Type t, string parameters = null);
        void ScheduleImmediately<T>(string parameters = null) where T : IJob;
        void SchedulePeriodic<T>(string cronExpression, string parameters = null) where T : IJob;
        void SchedulePeriodic(Type t, string cronExpression, string parameters = null);
        void ScheduleOnceAt<T>(DateTime dateTime, string parameters = null, string groupName = null) where T : IJob;        

        void CancelJob(string jobId);
        void CancelJobs();

        Task RunAsync(CancellationToken cancellationToken);        
        
        IEnumerable<JobInfo> GetActiveJobs();        
    }
}