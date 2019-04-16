using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PureCloud.Utils.Domain.Attribute;
using PureCloud.Utils.Domain.Models;
using PureCloud.Utils.Infra.Service.Client;
using PureCloud.Utils.Infra.Service.Storage;
using PureCloudPlatform.Client.V2.Model;

namespace PureCloud.Utils.Function.TimeTrigger
{
    public static class RecordingDownload
    {
        [FunctionName("RecordingDownload")]
        [ExceptionFilter(Name = "ConversationRecovery")]
        public async static Task Run(
            [TimerTrigger("* */1 * * * *", RunOnStartup = true)]TimerInfo myTimer, 
            ILogger log)
        {
            log.LogInformation($"Started 'RecordingDownload' function");

            // TODO 5. get not processed "table.conversations"
            Domain.Models.Conversation conversation = await TableStorageService.GetNoProcessedItemToConversationTableAsync();

            if (conversation != null)
            {
                // TODO 6.  /api/v2/conversations/{conversationId}/recordings
                PureCloudClient purecloudClient = new PureCloudClient();
                await purecloudClient.GetAccessToken();

                // Post batch download conversations
                string jobId = await purecloudClient.BatchRecordingDownloadByConversation(conversation.ConversationId);
                // Get url for download by JobId
                BatchDownloadJobStatusResult batch = await purecloudClient.GetJobRecordingDownloadResultByConversation(jobId);

                if (batch.Results != null)
                {
                    if (batch.ErrorCount.Equals(0))
                    {
                        log.LogInformation($"Total callrecordings to download: {batch.Results.Count}, " +
                            $"for conversation: {conversation.ConversationId}, in JobId: {jobId}");

                        foreach (var item in batch.Results)
                        {
                            await TableStorageService.AddToCallRecorginsTableAsync(
                                new CallRecording() {
                                    JobId = jobId,
                                    RecordingId = item.RecordingId,
                                    ConversationId = item.ConversationId,
                                    CallRecordingJson = JsonConvert.SerializeObject(item)
                                });

                            // TODO 7. download file and upload to "blob.callrecordings"
                            if (!string.IsNullOrEmpty(item.ResultUrl))
                            {
                                await BlobStorageService.CopyCallRecordingFromUrlToBlobStorage(
                                    item.ResultUrl, item.ConversationId, item.RecordingId + ".ogg");
                            }
                        }

                        // TODO 8. update "table.conversations" with uridownload
                        conversation.Processed = true;
                        await TableStorageService.UpdateConversationTableAsync(conversation);
                    }
                    else if (batch.Results.Count.Equals(1))
                    {
                        log.LogInformation($"No callrecordings to download for conversation: " +
                            $"{conversation.ConversationId}, in JobId: {jobId}");

                        await TableStorageService.AddToCallRecorginsTableAsync(
                            new CallRecording() {
                                JobId = batch.JobId,
                                ConversationId = batch.Results[0].ConversationId,
                                CallRecordingJson = JsonConvert.SerializeObject(batch),
                                ErrorMsg = batch.Results[0].ErrorMsg
                            });
                    }
                }
                else
                {
                    log.LogInformation($"Error for conversation: {conversation.ConversationId}, in JobId: {jobId}");

                    await TableStorageService.AddToCallRecorginsTableAsync(
                        new CallRecording()
                        {
                            ConversationId = conversation.ConversationId,
                            JobId = jobId,
                            ErrorMsg = batch.Results[0].ErrorMsg
                        });
                }
            }
            else
            {
                log.LogInformation($"No conversation to process");
            }

            log.LogInformation($"Ended 'RecordingDownload' function");
        }
    }
}
