using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using PureCloud.Utils.Infra.Service.Client;
using PureCloudPlatform.Client.V2.Model;
using System;
using System.Threading.Tasks;

namespace PureCloud.Utils.Function.ServiceBusTrigger
{
    public static class JobQueue
    {
        [FunctionName("JobQueue")]
        public static async Task RunAsync(
            [ServiceBusTrigger("jobqueue", Connection = "ServiceBusConnectionString")]string jobId,
            [ServiceBus("jobqueue", Connection = "ServiceBusConnectionString", EntityType = EntityType.Queue)]IAsyncCollector<string> jobQueue,
            [Blob("conversation", Connection = "StorageConnectionString")] CloudBlobContainer container,
            ILogger log)
        {
            try
            {
                await container.CreateIfNotExistsAsync();

                // TODO: read from "jobQueue"
                PureCloudClient purecloudClient = new PureCloudClient();
                await purecloudClient.GetAccessToken();
                BatchDownloadJobStatusResult batch = await purecloudClient.GetJobRecordingDownloadResultByConversation(jobId);

                // TODO: end when resultcount == resultaudios
                if (!batch.ExpectedResultCount.Equals(0) && batch.ExpectedResultCount.Equals(batch.ResultCount))
                {
                    if (batch.Results != null)
                    {
                        //List<Task> taskList = new List<Task>(); // to mutch performatic kkkk :P
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
                                    await convesrationBlob.StartCopyAsync(new Uri(item.ResultUrl));
                                }
                                else
                                {
                                    CloudBlockBlob convesrationBlob = container.GetBlockBlobReference($"{item.ConversationId}.error");
                                    await convesrationBlob.UploadTextAsync(JsonConvert.SerializeObject(item));
                                }
                            }
                        }
                        //await Task.WhenAll(taskList.ToArray()); // to mutch performatic kkkk :P
                    }

                }
                else
                {
                    // TODO: else, return item to jobQueue for next tentative
                    await jobQueue.AddAsync(jobId);
                }
            }
            catch (Exception ex)
            {
                TelemetryClient telemetry = new TelemetryClient();
                telemetry.InstrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");
                telemetry.TrackException(ex);

                log.LogInformation($"Exception during execution: {ex.Message}");

                do
                {
                    try
                    {
                        await Task.Delay(Convert.ToInt32(Environment.GetEnvironmentVariable("deplaytime")));

                        await jobQueue.AddAsync(jobId);
                        break;
                    }
                    catch { }
                } while (true);
            }
        }
    }
}
