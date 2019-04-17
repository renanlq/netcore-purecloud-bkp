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
        private static readonly string _callrecordingsBlob = Environment.GetEnvironmentVariable("storage:blob:callrecordings", EnvironmentVariableTarget.Process);
        private static readonly string _conversationsBlob = Environment.GetEnvironmentVariable("storage:blob:conversations", EnvironmentVariableTarget.Process);
        private static readonly string _errorLogsBlob = Environment.GetEnvironmentVariable("storage:blob:errorlogs", EnvironmentVariableTarget.Process);
        private static readonly string _usersBlob = Environment.GetEnvironmentVariable("storage:blob:users", EnvironmentVariableTarget.Process);

        public static async Task AddCallRecordingAsync(string callrecording, string folder, string name)
        {
            var blobContainer = await GetOrCreateContainerAsync(_callrecordingsBlob);
            var cloudBlockBlob = blobContainer.GetBlockBlobReference($"{folder}/{name}");

            await cloudBlockBlob.UploadTextAsync(callrecording);
        }

        public static async Task AddCallRecordingAudioFromUrlAsync(string url, string folder, string name)
        {
            var blobContainer = await GetOrCreateContainerAsync(_callrecordingsBlob);
            var cloudBlockBlob = blobContainer.GetBlockBlobReference($"{folder}/{name}");

            await cloudBlockBlob.StartCopyAsync(new Uri(url));
        }

        public static async Task AddConversationFromTextAsync(string conversation, string name)
        {
            var blobContainer = await GetOrCreateContainerAsync(_conversationsBlob);
            var cloudBlockBlob = blobContainer.GetBlockBlobReference($"{name}");

            await cloudBlockBlob.UploadTextAsync(conversation);
        }

        public static async Task AddErrorFromTextAsync(string error, string folder, string name)
        {
            var blobContainer = await GetOrCreateContainerAsync(_errorLogsBlob);
            var cloudBlockBlob = blobContainer.GetBlockBlobReference($"{folder}/{name}");

            await cloudBlockBlob.UploadTextAsync(error);
        }

        public static async Task AddUserFromTextAsync(string user, string name)
        {
            var blobContainer = await GetOrCreateContainerAsync(_usersBlob);
            var cloudBlockBlob = blobContainer.GetBlockBlobReference($"{name}");

            await cloudBlockBlob.UploadTextAsync(user);
        }

        private static async Task<CloudBlobContainer> GetOrCreateContainerAsync(string blob)
        {
            var blobContainer = StorageContext.BlobClient.GetContainerReference(blob);
            await blobContainer.CreateIfNotExistsAsync();

            return blobContainer;
        }
    }
}