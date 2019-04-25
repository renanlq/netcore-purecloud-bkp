using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using PureCloud.Utils.Domain.Models;
using PureCloud.Utils.Infra.Service.Client;
using PureCloudPlatform.Client.V2.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PureCloud.Utils.Function.ServiceBusTrigger
{
    public static class ConversationQueue
    {
        [FunctionName("ConversationQueue")]
        public static async Task RunAsync(
            [ServiceBusTrigger("pagequeue", Connection = "ServiceBusConnectionString")]string pageJson,
            [EventHub("convesationhub", Connection = "EventhubConnectionString")]IAsyncCollector<string> convesationhub,
            [ServiceBus("pagequeue", Connection = "ServiceBusConnectionString", EntityType = EntityType.Queue)]IAsyncCollector<string> pageQueue,
            [Blob("conversation", Connection = "StorageConnectionString")] CloudBlobContainer container,
            ILogger log)
        {
            try
            {
                await container.CreateIfNotExistsAsync();

                // TODO: read from "pageQueue"
                DatePage datePage = new DatePage(pageJson);

                // TODO: get conversations from PureCloudClient
                PureCloudClient pureCloudClient = new PureCloudClient();
                await pureCloudClient.GetAccessToken();
                List<AnalyticsConversation> conversations = await pureCloudClient.GetConversationsByInterval(
                    datePage.Date, datePage.Date, datePage.Page);

                log.LogInformation($"Processing date: {datePage.Date}, at page: {datePage.Page}, with {conversations.Count} conversations");

                // TODO: add on "conversationQueue"
                //List<Task> taskList = new List<Task>(); // to mutch performatic kkkk :P
                foreach (var item in conversations)
                {
                    string conversationJson = JsonConvert.SerializeObject(item);
                    await convesationhub.AddAsync(conversationJson);

                    // add conversationJson to blob storage
                    CloudBlockBlob convesrationBlob = container.GetBlockBlobReference($"{item.ConversationId}-conversation.json");
                    await convesrationBlob.UploadTextAsync(conversationJson);
                }
                //await Task.WhenAll(taskList.ToArray()); // to mutch performatic kkkk :P

                // TODO: add new page on same date
                if (conversations.Count.Equals(100))
                { 
                    await pageQueue.AddAsync(new DatePage()
                    {
                        Date = datePage.Date,
                        Page = datePage.Page + 1
                    }.ToJson());
                }
            }
            catch (Exception ex)
            {
                TelemetryClient telemetry = new TelemetryClient();
                telemetry.InstrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");
                telemetry.TrackException(ex);

                do
                {
                    try
                    {
                        await Task.Delay(Convert.ToInt32(Environment.GetEnvironmentVariable("deplaytime")));
                        await pageQueue.AddAsync(pageJson);
                        break;
                    }
                    catch { }
                } while (true);
            }
        }
    }
}
