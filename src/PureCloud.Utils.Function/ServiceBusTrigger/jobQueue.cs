using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using PureCloud.Utils.Infra.Service.Client;
using PureCloudPlatform.Client.V2.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PureCloud.Utils.Function.ServiceBusTrigger
{
    public static class JobQueue
    {
        [FunctionName("JobQueue")]
        public static async Task RunAsync(
            [ServiceBusTrigger("jobqueue", Connection = "ServiceBusConnectionString")]string jobJson,
            [ServiceBus("jobqueue", Connection = "ServiceBusConnectionString", EntityType = EntityType.Queue)]IAsyncCollector<string> jobQueue,
            [Blob("conversation", Connection = "StorageConnectionString")] CloudBlobContainer container,
            ILogger log)
        {
            try
            {
                await container.CreateIfNotExistsAsync();

                BatchDownloadJobSubmissionResult job = JsonConvert.DeserializeObject<BatchDownloadJobSubmissionResult>(jobJson);

                // TODO: read from "jobQueue"
                PureCloudClient purecloudClient = new PureCloudClient();
                await purecloudClient.GetAccessToken();
                BatchDownloadJobStatusResult batch = await purecloudClient.GetJobRecordingDownloadResultByConversation(job.Id);

                // TODO: end when resultcount == resultaudios
                if (batch.ExpectedResultCount.Equals(batch.ResultCount))
                {
                    List<Task> taskList = new List<Task>();
                    foreach (var item in batch.Results)
                    {
                        if (!string.IsNullOrEmpty(item.ResultUrl))
                        {
                            if (string.IsNullOrEmpty(item.ErrorMsg))
                            {
                                var extension = string.Empty;
                                switch (item.ContentType)
                                {
                                    case "audio/ogg":
                                        extension = "ogg";
                                        break;
                                    case "application/zip":
                                        extension = "zip";
                                        break;
                                    default:
                                        extension = "none";
                                        break;
                                }

                                CloudBlockBlob convesrationBlob = container.GetBlockBlobReference($"{item.ConversationId}-{item.RecordingId}.{extension}");
                                taskList.Add(convesrationBlob.StartCopyAsync(new Uri(item.ResultUrl)));
                            }
                            else
                            {
                                CloudBlockBlob convesrationBlob = container.GetBlockBlobReference($"{item.ConversationId}.error");
                                taskList.Add(convesrationBlob.UploadTextAsync(JsonConvert.SerializeObject(item)));
                            }
                        }
                    }
                    await Task.WhenAll(taskList.ToArray());
                }
                else
                {
                    // TODO: else, return item to jobQueue for next tentative
                    await jobQueue.AddAsync(jobJson);
                }
            }
            catch (Exception ex)
            {
                await jobQueue.AddAsync(jobJson);

                log.LogInformation($"Exception: {ex.Message}");

                TelemetryClient telemetry = new TelemetryClient();
                telemetry.InstrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");
                telemetry.TrackException(ex);
            }
        }
    }
}
