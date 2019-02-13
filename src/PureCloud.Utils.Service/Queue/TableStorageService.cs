using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Queue;
using PureCloud.Utils.Infra.Context;

namespace PureCloud.Utils.Service.Queue
{
    public static class QueueStorageService
    {
        public static async Task AddToLogQueueAsync(string messageContent, TimeSpan delay)
        {
            var queue = await GetOrCreateLogQueueAsync();
            var message = new CloudQueueMessage(messageContent);

            await queue.AddMessageAsync(message, null, delay, null, null);
        }

        private static async Task<CloudQueue> GetOrCreateLogQueueAsync()
        {
            var queue = StorageContext.Log;

            await queue.CreateIfNotExistsAsync();

            return queue;
        }
    }
}