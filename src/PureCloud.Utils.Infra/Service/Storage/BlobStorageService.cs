using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using PureCloud.Utils.Infra.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PureCloud.Utils.Infra.Service.Storage
{
    public static class BlobStorageService
    {
        private static readonly string _callrecordingBlob = Environment.GetEnvironmentVariable("storage:blob:callrecordings", EnvironmentVariableTarget.Process);
        private static readonly string _conversationBlob = Environment.GetEnvironmentVariable("storage:blob:conversations", EnvironmentVariableTarget.Process);
        private static readonly string _errorLogBlob = Environment.GetEnvironmentVariable("storage:blob:errorlogs", EnvironmentVariableTarget.Process);

        public static async Task UploadCallRecordingBlobAsync(string path, string name, byte[] blob)
        {
            var blobContainer = await GetOrCreateContainerAsync(_callrecordingBlob);

            await blobContainer.SetPermissionsAsync(new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            });
            var cloudBlockBlob = blobContainer.GetBlockBlobReference($"{path}/{name}");

            await cloudBlockBlob.UploadFromByteArrayAsync(blob, 0, 0);
        }

        public static async Task CopyCallRecordingFromUrlToBlobStorage(string url, string folder, string filename)
        {
            var blobContainer = await GetOrCreateContainerAsync(_callrecordingBlob);
            var cloudBlockBlob = blobContainer.GetBlockBlobReference($"{folder}/{filename}");

            await cloudBlockBlob.StartCopyAsync(new Uri(url));
        }

        private static async Task<List<IListBlobItem>> ListBlobAsync(CloudBlobContainer blobContainer)
        {
            BlobContinuationToken continuationToken = null;
            List<IListBlobItem> results = new List<IListBlobItem>();

            do
            {
                var response = await blobContainer.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;
                results.AddRange(response.Results);

            } while (continuationToken != null);

            return results;
        }

        private static async Task<CloudBlobContainer> GetOrCreateContainerAsync(string blob)
        {
            var blobContainer = StorageContext.BlobClient.GetContainerReference(blob);
            await blobContainer.CreateIfNotExistsAsync();

            return blobContainer;
        }
    }
}