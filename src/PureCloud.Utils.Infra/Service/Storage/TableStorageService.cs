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
        private static readonly string _conversationTable = Environment.GetEnvironmentVariable("storage:table:convesations", EnvironmentVariableTarget.Process);

        public static async Task AddToConversationTableAsync(Conversation content)
        {
            var table = await GetOrCreateQueueAsync(_conversationTable);

            TableOperation operation = TableOperation.Insert(content);
            await table.ExecuteAsync(operation);
        }

        public static async Task<Conversation> GetItemNoURLToConversationTableAsync()
        {
            var table = await GetOrCreateQueueAsync(_conversationTable);

            TableQuery<Conversation> query = new TableQuery<Conversation>()
                                 .Where(TableQuery.GenerateFilterCondition(
                                     "URL", QueryComparisons.Equal, null));

            var results = await table.ExecuteQuerySegmentedAsync(query, null);
            return (results.FirstOrDefault());
        }

        public static async Task UpdateToConversationTableAsync(Conversation content)
        {
            var table = await GetOrCreateQueueAsync(_conversationTable);

            TableOperation operation = TableOperation.InsertOrReplace(content);
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
