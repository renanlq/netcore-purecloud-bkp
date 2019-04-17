﻿using Microsoft.WindowsAzure.Storage.Table;
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
        private static readonly string _usersTable = Environment.GetEnvironmentVariable("storage:table:users", EnvironmentVariableTarget.Process);

        public static async Task AddToCallRecorgindAsync(CallRecording content)
        {
            var table = await GetOrCreateTableAsync(_callrecordingsTable);
            await AddToTableAsync<CallRecording>(content, table);
        }

        public static async Task AddToConversationAsync(Conversation content)
        {
            var table = await GetOrCreateTableAsync(_conversationsTable);
            await AddToTableAsync<Conversation>(content, table);
        }
        
        public static async Task AddToProcessedDatesAsync(ProcessedDate content)
        {
            var table = await GetOrCreateTableAsync(_processedDatesTable);
            await AddToTableAsync<ProcessedDate>(content, table);
        }

        public static async Task AddToUserAsync(User content)
        {
            var table = await GetOrCreateTableAsync(_usersTable);
            await AddToTableAsync<User>(content, table);
        }

        public static async Task<Conversation> GetNotProcessedConversationAsync()
        {
            var table = await GetOrCreateTableAsync(_conversationsTable);

            TableQuery<Conversation> query = new TableQuery<Conversation>()
                                 .Where(TableQuery.GenerateFilterConditionForBool(
                                     "Processed", QueryComparisons.Equal, false));

            var results = await table.ExecuteQuerySegmentedAsync(query, null);
            return results.FirstOrDefault();
        }

        public static async Task<ProcessedDate> GetLastProcessedDateAsync()
        {
            var table = await GetOrCreateTableAsync(_processedDatesTable);

            TableQuery<ProcessedDate> query = new TableQuery<ProcessedDate>();

            var results = await table.ExecuteQuerySegmentedAsync(query, null);
            return (results.OrderByDescending(r => r.Timestamp).FirstOrDefault());
        }

        public static async Task<User> GetUserAsync(string id)
        {
            var table = await GetOrCreateTableAsync(_usersTable);

            TableQuery<User> query = new TableQuery<User>()
                                 .Where(TableQuery.GenerateFilterCondition(
                                     "Id", QueryComparisons.Equal, id));

            var results = await table.ExecuteQuerySegmentedAsync(query, null);
            return results.FirstOrDefault();
        }

        public static async Task UpdateConversationTableAsync(Conversation content)
        {
            var table = await GetOrCreateTableAsync(_conversationsTable);

            TableOperation operation = TableOperation.Merge(content);

            await table.ExecuteAsync(operation);
        }

        private static async Task AddToTableAsync<T>(T content, CloudTable table)
        {
            TableOperation operation = TableOperation.Insert((ITableEntity)content);
            operation.Entity.PartitionKey = "purecloud";
            operation.Entity.RowKey = Guid.NewGuid().ToString();

            await table.ExecuteAsync(operation);
        }

        private static async Task<CloudTable> GetOrCreateTableAsync(string table)
        {
            var tableClient = StorageContext.TableClient.GetTableReference(table);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }
    }
}
