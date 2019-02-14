using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using PureCloud.Utils.Infra.Context;

namespace PureCloud.Utils.Service.Queue
{
    public static class QueueStorageService
    {
        public static async Task AddToConversationQueueAsync(string content, TimeSpan delay, ILogger log)
        {
            var queue = await GetOrCreateQueueAsync(StorageContext.Conversation);
            var message = new CloudQueueMessage(content);
            await queue.AddMessageAsync(message, null, delay, null, null);
        }

        public static async Task DeleteToConversationQueueAsync(string content, TimeSpan delay)
        {
            var queue = await GetOrCreateQueueAsync(StorageContext.Conversation);
            var message = new CloudQueueMessage(content);
            await queue.DeleteMessageAsync(message);
        }

        public static async Task AddToJobQueueAsync(string content, TimeSpan delay)
        {
            var queue = await GetOrCreateQueueAsync(StorageContext.Job);
            var message = new CloudQueueMessage(content);
            await queue.AddMessageAsync(message, null, delay, null, null);
        }

        private static async Task<CloudQueue> GetOrCreateQueueAsync(CloudQueue queue)
        {
            await queue.CreateIfNotExistsAsync();
            return queue;
        }
    }
}