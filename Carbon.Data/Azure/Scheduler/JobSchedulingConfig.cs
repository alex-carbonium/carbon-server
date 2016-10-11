using System;
using System.Diagnostics;
using Carbon.Business;
using Carbon.Framework.JobScheduling;
using Carbon.Framework.Logging;
using Carbon.Framework.UnitOfWork;
using Carbon.Framework.Util;
using Microsoft.WindowsAzure.Storage;
using Carbon.Business.Logging;

namespace Carbon.Data.Azure.Scheduler
{
    public class JobSchedulingConfig
    {
        //TODO: fix scheduler if needed
        public static IJobScheduler Register(IDependencyContainer container, SchedulerOptions options = null)
        {
            var logService = container.Resolve<ILogService>();
            var appSettings = container.Resolve<AppSettings>();

            options = options ?? new SchedulerOptions
            {
                StorageAccount = container.Resolve<CloudStorageAccount>(),
                //CloudServiceName = appSettings.Azure.SchedulerServiceName,
                //InvisibilityTimeout = TimeSpan.FromSeconds(appSettings.Azure.SchedulerInvisibilityTimeout)
            };
            //if (!string.IsNullOrEmpty(appSettings.Azure.PublishSettingsFile))
            //{
            //    options.PublishSettingsFile = appSettings.GetPhysicalPath(appSettings.Azure.PublishSettingsFile);
            //}
            options.LogException = ex => logService.GetLogger(typeof(AzureJobScheduler).FullName).ErrorWithContext("Unhandled job exception", ex);
            
            var scheduler = new AzureJobScheduler(options, (type, parameters) => RunJob(container, logService, type, parameters));

            container.RegisterInstance<IJobScheduler>(scheduler);

            return scheduler;
        }

        public static void RunJob(IDependencyContainer container, ILogService logService, Type type, string parameters)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var logger = logService.GetLogger(type.FullName);

            using (var scope = container.BeginScope())
            using (var uow = scope.Resolve<IUnitOfWork>())
            {
                var context = new JobContext(scope, uow, logger);

                var job = (IJob) scope.Resolve(type);
                job.Initialize(parameters);
                job.Execute(context);

                uow.Commit();
            }

            stopwatch.Stop();
            logger.Info("Job completed in {0} ms", stopwatch.ElapsedMilliseconds);
        }
    }
}
