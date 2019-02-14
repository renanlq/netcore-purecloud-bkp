using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using PureCloud.Utils.Infra.Context;

namespace PureCloud.Utils.Service.Storage
{
    public static class QueueStorageService
    {
        private static readonly string _conversationQueue = Environment.GetEnvironmentVariable("storage:queue:convesation", EnvironmentVariableTarget.Process);
        private static readonly string _jobQueue = Environment.GetEnvironmentVariable("storage:queue:job", EnvironmentVariableTarget.Process);


        public static async Task AddToConversationQueueAsync(string content, TimeSpan delay)
        {
            var queue = await GetOrCreateQueueAsync(_conversationQueue);

            var message = new CloudQueueMessage(content);            
            await queue.AddMessageAsync(message, null, delay, null, null);
        }

        public static async Task AddToJobQueueAsync(string content, TimeSpan delay)
        {
            var queue = await GetOrCreateQueueAsync(_jobQueue);
            
            var message = new CloudQueueMessage(content);
            await queue.AddMessageAsync(message, null, delay, null, null);
        }

        private static async Task<CloudQueue> GetOrCreateQueueAsync(string queue)
        {
            var queueClient = StorageContext.QueueClient.GetQueueReference(queue);
            await queueClient.CreateIfNotExistsAsync();
            return queueClient;
        }
    }
}