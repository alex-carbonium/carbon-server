using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Carbon.Framework.JobScheduling;
using Hyak.Common;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Scheduler;
using Microsoft.WindowsAzure.Scheduler.Models;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json.Linq;

namespace Carbon.Data.Azure.Scheduler
{
    public class AzureJobScheduler : IJobScheduler
    {
        private readonly CloudQueueClient _queueClient;        
        private readonly SchedulerOptions _options;
        private readonly Action<Type, string> _runner;        
        private CloudQueue _queue;
        //private CertificateCloudCredentials _credentials;        

        public AzureJobScheduler(SchedulerOptions options, Action<Type, string> runner)
        {
            _queueClient = options.StorageAccount.CreateCloudQueueClient();            
            _options = options;
            _runner = runner;
        }

        public void ScheduleImmediately(Type t, string parameters = null)
        {
            var job = CreateJobDefinition(t, parameters);
            var xml = new XElement("ImmediateAction", new XElement("Message", job.ToString()));            
            GetQueue().AddMessage(new CloudQueueMessage(xml.ToString()));
        }

        public void ScheduleImmediately<T>(string parameters = null) where T : IJob
        {
            ScheduleImmediately(typeof(T), parameters);
        }

        public void SchedulePeriodic<T>(string cronExpression, string parameters = null) where T : IJob
        {
            SchedulePeriodic(typeof(T), cronExpression, parameters);
        }

        public void SchedulePeriodic(Type t, string cronExpression, string parameters = null)
        {                                    
            var recurrence = new JobRecurrence();
            recurrence.InitializeFromCron(cronExpression);
            var job = CreateJobDefinition(t, parameters);
            job["recurrent"] = true;
            ScheduleDelayed(job, DateTime.UtcNow, recurrence);
        }

        public void ScheduleOnceAt<T>(DateTime dateTime, string parameters = null, string groupName = null) where T : IJob
        {
            var job = CreateJobDefinition(typeof (T), parameters);
            ScheduleDelayed(job, dateTime);
        }

        public void CancelJob(string jobId)
        {            
            GetMessages(m =>
            {
                if (m.Id == jobId)
                {
                    GetQueue().DeleteMessage(m);
                    return false;
                }
                return true;
            });

            var schedulerClient = GetSchedulerClient();
            var futureJobs = schedulerClient.Jobs.List(new JobListParameters())
                .Jobs.Where(x => x.Id == jobId);
            foreach (var job in futureJobs)
            {
                schedulerClient.Jobs.Delete(job.Id);
            }

        }

        public void CancelJobs()
        {            
            GetMessages(m =>
            {
                GetQueue().DeleteMessage(m);
                return true;
            });

            var schedulerClient = GetSchedulerClient();
            var futureJobs = schedulerClient.Jobs.List(new JobListParameters());
            foreach (var job in futureJobs.Jobs)
            {
                try
                {
                    schedulerClient.Jobs.Delete(job.Id);
                }
                catch (CloudException ex)
                {
                    if (ex.Response.StatusCode != HttpStatusCode.NotFound)
                    {
                        throw;
                    }
                }
            }
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            Running = true;
            try
            {
                await Process(cancellationToken);
            }
            catch (TaskCanceledException)
            {                
            }
            Running = false;
        }        

        public IEnumerable<JobInfo> GetActiveJobs()
        {
            var jobs = new List<JobInfo>();
            
            PeekMessages(m =>
            {
                var task = GetTaskDefinition(m.AsString);
                jobs.Add(new JobInfo
                {
                    JobId = m.Id,
                    JobType = task.Value<string>("type"),
                    Parameters = task.Value<string>("parameters"),
                    FireTime = DateTime.UtcNow
                });
                return true;
            });

            var azureJobs = GetSchedulerClient().Jobs.List(new JobListParameters()).Jobs;
            
            jobs.AddRange(
                from azureJob in azureJobs
                let task = JObject.Parse(azureJob.Action.QueueMessage.Message)
                select new JobInfo
                {
                    JobId = azureJob.Id,
                    JobType = task.Value<string>("type"), 
                    Parameters = task.Value<string>("parameters"), 
                    FireTime = azureJob.Status.NextExecutionTime.Value
                });

            return jobs;
        }

        private void GetMessages(Func<CloudQueueMessage, bool> func)
        {
            var queue = GetQueue();
            IEnumerable<CloudQueueMessage> messages;
            do
            {
                messages = queue.GetMessages(_options.MessageBatchSize, TimeSpan.FromSeconds(1)).ToList();
                if (messages.Any())
                {
                    if (messages.Any(message => !func(message)))
                    {
                        return;
                    }
                }
            } while (messages.Any());
        }

        private void PeekMessages(Func<CloudQueueMessage, bool> func)
        {
            var queue = GetQueue();
            IEnumerable<CloudQueueMessage> messages;
            do
            {
                messages = queue.GetMessages(_options.MessageBatchSize).ToList();
                if (messages.Any())
                {
                    if (messages.Any(message => !func(message)))
                    {
                        return;
                    }
                }
            } while (messages.Any());
        }

        private async Task Process(CancellationToken token)
        {
            await RunWithBackoff(async () =>
            {
                var queue = GetQueue();
                var messages = await queue.GetMessagesAsync(_options.MessageBatchSize, _options.InvisibilityTimeout, null, null, token);
                if (messages == null || !messages.Any())
                {
                    return false;
                }
                
                foreach (var message in messages)
                {
                    var task = GetTaskDefinition(message.AsString);                
                    try
                    {                        
                        RunJob(task);
                    }
                    finally
                    {
                        queue.DeleteMessage(message);
                        var azureJobId = task.Value<string>("azureJobId");
                        if (!task.Value<bool>("recurrent") && azureJobId != null)
                        {
                            GetSchedulerClient().Jobs.Delete(azureJobId);
                        }   
                    }                    
                }
                return true;

            }, token);
        }

        private static JObject GetTaskDefinition(string message)
        {                        
            var xml = XDocument.Parse(message);            
            var task = JObject.Parse(xml.Descendants("Message").Single().Value);
            var jobId = xml.Descendants("SchedulerJobId").SingleOrDefault();
            if (jobId != null)
            {
                task["azureJobId"] = jobId.Value;
            }            
            return task;
        }        

        private async Task RunWithBackoff(Func<Task<bool>> action, CancellationToken token)
        {
            var minInterval = 1;
            var interval = minInterval;
            var exponent = 2;
            var maxInterval = 60;

            while (!token.IsCancellationRequested)
            {
                var exception = false;
                var performed = false;
                try
                {
                    performed = await action();
                }
                catch (Exception ex)
                {
                    _options.LogException(ex);
                    exception = true;
                }
                
                if (exception)
                {
                    interval = maxInterval;
                }
                else if (performed)
                {
                    interval = minInterval;
                }
                else
                {
                    interval = Math.Min(maxInterval, interval*exponent);
                }

                await Task.Delay(TimeSpan.FromSeconds(interval), token);
            }
        }

        private void RunJob(JObject definition)
        {
            var assembly = Assembly.Load(definition.Value<string>("assembly"));            
            var type = assembly.GetType(definition.Value<string>("type"));
            _runner(type, definition.Value<string>("parameters"));
        }

        private string GetQueueAccessToken()
        {
            const string policyName = "schedulerpolicy";

            var queue = GetQueue();            
            var permissions = queue.GetPermissions();
            if (!permissions.SharedAccessPolicies.ContainsKey(policyName))
            {
                var policy = new SharedAccessQueuePolicy { SharedAccessExpiryTime = DateTime.MaxValue, Permissions = SharedAccessQueuePermissions.Add };
                permissions.SharedAccessPolicies.Add(policyName, policy);
                queue.SetPermissions(permissions);
            }                        

            return queue.GetSharedAccessSignature(new SharedAccessQueuePolicy(), policyName); 
        }

        private void ScheduleDelayed(JObject job, DateTime startTime, JobRecurrence recurrence = null)
        {                                    
            var schedulerClient = GetSchedulerClient();

            var result = schedulerClient.Jobs.Create(new JobCreateParameters
            {               
                Action = new JobAction
                {                    
                    Type = JobActionType.StorageQueue,                    
                    QueueMessage = new JobQueueMessage
                    {
                        Message = job.ToString(),
                        QueueName = GetQueue().Name,
                        SasToken = GetQueueAccessToken(),
                        StorageAccountName = _options.StorageAccount.Credentials.AccountName
                    }
                },
                StartTime = startTime,
                Recurrence = recurrence
            });

            if (result.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException(string.Format("Could not schedule job {0}, http code {1}", job, result.StatusCode));
            }
        }

        public SchedulerClient GetSchedulerClient()
        {
//            var credentials = GetCredentials();
//            var schedulerClient = new SchedulerClient(_options.CloudServiceName, _options.JobCollectionName, credentials);
//            return schedulerClient;
            return null;
        }

        //private CertificateCloudCredentials GetCredentials()
        //{
        //    if (_credentials == null)
        //    {
        //        _credentials = CertificateCloudCredentialsFactory.FromPublishSettingsFile(_options.PublishSettingsFile);
        //    }            
        //    return _credentials;
        //}

        private JObject CreateJobDefinition(Type type, string parameters = null)
        {
            var job = new JObject();
            job["assembly"] = type.Assembly.GetName().Name;
            job["type"] = type.FullName;
            if (!string.IsNullOrEmpty(parameters))
            {
                job["parameters"] = parameters;
            }
            return job;
        }

        private CloudQueue GetQueue()
        {
            if (_queue == null)
            {
                _queue = _queueClient.GetQueueReference(_options.QueueName);                
                _queue.CreateIfNotExists();
            }            
            return _queue;
        }

        public bool Running { get; private set; }
    }
}
