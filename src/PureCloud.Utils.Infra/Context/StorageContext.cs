using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;

namespace PureCloud.Utils.Infra.Context
{
    public static class StorageContext
    {
        private static CloudStorageAccount _cloudStorageAccount;
        private static readonly string _connectionStringLocal = Environment.GetEnvironmentVariable("StorageConnectionString", EnvironmentVariableTarget.Process);
        private static readonly string _connectionStringServer = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
        private static readonly string _conversationQueue = Environment.GetEnvironmentVariable("Queue:Convesation", EnvironmentVariableTarget.Process);
        private static readonly string _jobQueue = Environment.GetEnvironmentVariable("Queue:Job", EnvironmentVariableTarget.Process);

        public static CloudQueue Conversation
        {
            get
            {
                return GetCloudQueue(_conversationQueue);
            }
        }

        public static CloudQueue Job
        {
            get
            {
                return GetCloudQueue(_jobQueue);
            }
        }

        private static CloudQueue GetCloudQueue(string queue)
        {
                var connectionString = _connectionStringLocal ?? _connectionStringServer;
                _cloudStorageAccount = _cloudStorageAccount ?? CloudStorageAccount.Parse(connectionString);

                return _cloudStorageAccount.CreateCloudQueueClient().GetQueueReference(queue);
        }
    }
}