using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace PureCloud.Utils.Infra.Context
{
    public static class StorageContext
    {
        private static CloudStorageAccount _cloudStorageAccount;
        private static readonly string _connectionStringLocal = Environment.GetEnvironmentVariable("StorageConnectionString", EnvironmentVariableTarget.Process);
        private static readonly string _connectionStringServer = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
        
        public static CloudBlobClient BlobClient
        {
            get
            {
                _cloudStorageAccount = _cloudStorageAccount ?? CloudStorageAccount.Parse(_connectionStringServer);
                return _cloudStorageAccount.CreateCloudBlobClient();
            }
        }

        public static CloudQueueClient QueueClient
        {
            get
            {
                _cloudStorageAccount = _cloudStorageAccount ?? CloudStorageAccount.Parse(_connectionStringServer);
                return _cloudStorageAccount.CreateCloudQueueClient();
            }
        }

        public static CloudTableClient TableClient
        {
            get
            {
                _cloudStorageAccount = _cloudStorageAccount ?? CloudStorageAccount.Parse(_connectionStringServer);
                return _cloudStorageAccount.CreateCloudTableClient();
            }
        }
    }
}