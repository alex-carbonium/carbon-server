using Carbon.Business.Services;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Carbon.Data.Azure
{
    public class QueueMailService : IMailService
    {
        private readonly CloudQueueClient _client;
        private bool _created;

        public QueueMailService(CloudQueueClient client)
        {
            _client = client;
        }

        public async Task Send(string to, string template, dynamic model)
        {
            var queue = await GetOrCreateQueue();
            var content = JsonConvert.SerializeObject(new { to, template, model });
            await queue.AddMessageAsync(new CloudQueueMessage(content));
        }

        private async Task<CloudQueue> GetOrCreateQueue()
        {
            var queue = _client.GetQueueReference("jobs");

            if (!_created)
            {
                await queue.CreateIfNotExistsAsync();
                _created = true;
            }

            return queue;
        }
    }
}
