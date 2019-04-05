using Microsoft.WindowsAzure.Storage.Blob;
using PureCloud.Utils.Infra.Context;
using System;
using System.Threading.Tasks;

namespace PureCloud.Utils.Infra.Service.Storage
{
    public static class BlobStorageService
    {
        private static readonly string _callrecordingBlob = Environment.GetEnvironmentVariable("storage:blob:callrecordings", EnvironmentVariableTarget.Process);

        public static async Task UploadBlobAsync(string path, string name, string blob)
        {
            var container = await GetOrCreateContainerAsync();

            await container.SetPermissionsAsync(new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            });
            var cloudBlockBlob = container.GetBlockBlobReference($"{path}/{name}");

            await cloudBlockBlob.UploadTextAsync(blob);
        }

        private static async Task<CloudBlobContainer> GetOrCreateContainerAsync()
        {
            var container = StorageContext.BlobClient.GetContainerReference(_callrecordingBlob);
            await container.CreateIfNotExistsAsync();

            return container;
        }
    }
}