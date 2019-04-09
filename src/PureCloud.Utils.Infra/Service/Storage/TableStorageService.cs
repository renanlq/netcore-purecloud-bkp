using Microsoft.WindowsAzure.Storage.Table;
using PureCloud.Utils.Domain.Models;
using PureCloud.Utils.Infra.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PureCloud.Utils.Infra.Service.Storage
{
    public static class TableStorageService
    {
        private static readonly string _callrecordingsTable = Environment.GetEnvironmentVariable("storage:table:callrecordings", EnvironmentVariableTarget.Process);
        private static readonly string _conversationsTable = Environment.GetEnvironmentVariable("storage:table:conversations", EnvironmentVariableTarget.Process);
        private static readonly string _processedDatesTable = Environment.GetEnvironmentVariable("storage:table:processeddates", EnvironmentVariableTarget.Process);

        public static async Task AddToCallRecorginsTableAsync(CallRecording content)
        {
            var table = await GetOrCreateQueueAsync(_callrecordingsTable);

            TableOperation operation = TableOperation.Insert(content);
            operation.Entity.PartitionKey = "purecloud";
            operation.Entity.RowKey = Guid.NewGuid().ToString();

            await table.ExecuteAsync(operation);
        }

        public static async Task AddToConversationTableAsync(Conversation content)
        {
            var table = await GetOrCreateQueueAsync(_conversationsTable);

            TableOperation operation = TableOperation.Insert(content);
            operation.Entity.PartitionKey = "purecloud";
            operation.Entity.RowKey = Guid.NewGuid().ToString();

            await table.ExecuteAsync(operation);
        }
        
        public static async Task AddToProcessedDatesTableAsync(ProcessedDate content)
        {
            var table = await GetOrCreateQueueAsync(_processedDatesTable);

            TableOperation operation = TableOperation.Insert(content);
            operation.Entity.PartitionKey = "purecloud";
            operation.Entity.RowKey = Guid.NewGuid().ToString();

            await table.ExecuteAsync(operation);
        }

        public static async Task<Conversation> GetNoProcessedItemToConversationTableAsync()
        {
            var table = await GetOrCreateQueueAsync(_conversationsTable);

            TableQuery<Conversation> query = new TableQuery<Conversation>()
                                 .Where(TableQuery.GenerateFilterConditionForBool(
                                     "Processed", QueryComparisons.Equal, false));

            var results = await table.ExecuteQuerySegmentedAsync(query, null);
            return results.FirstOrDefault();
        }

        public static async Task<ProcessedDate> GetLastProcessedDateTableAsync()
        {
            var table = await GetOrCreateQueueAsync(_processedDatesTable);

            TableQuery<ProcessedDate> query = new TableQuery<ProcessedDate>();

            var results = await table.ExecuteQuerySegmentedAsync(query, null);
            return (results.OrderByDescending(r => r.Timestamp).FirstOrDefault());
        }

        public static async Task UpdateConversationTableAsync(Conversation content)
        {
            var table = await GetOrCreateQueueAsync(_conversationsTable);

            TableOperation operation = TableOperation.Merge(content);

            await table.ExecuteAsync(operation);
        }

        private static async Task<CloudTable> GetOrCreateQueueAsync(string table)
        {
            var tableClient = StorageContext.TableClient.GetTableReference(table);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }
    }
}
