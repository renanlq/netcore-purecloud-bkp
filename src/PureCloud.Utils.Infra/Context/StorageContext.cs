using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;

namespace PureCloud.Utils.Infra.Context
{
    public static class StorageContext
    {
        private static CloudStorageAccount _cloudStorageAccount;
        private static readonly string _connectionStringLocal = Environment.GetEnvironmentVariable("StorageConnectionString:key1", EnvironmentVariableTarget.Process);
        private static readonly string _connectionStringServer = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
        private static readonly string _jobQueue = Environment.GetEnvironmentVariable("job", EnvironmentVariableTarget.Process);

        public static CloudQueue Log
        {
            get
            {
                var connectionString = _connectionStringLocal ?? _connectionStringServer;
                _cloudStorageAccount = _cloudStorageAccount ?? CloudStorageAccount.Parse(connectionString);

                return _cloudStorageAccount.CreateCloudQueueClient().GetQueueReference(_jobQueue);
            }
        }
    }
}