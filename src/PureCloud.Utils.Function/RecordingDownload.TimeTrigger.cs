using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PureCloud.Utils.Domain.Attribute;
using PureCloud.Utils.Domain.Models;
using PureCloud.Utils.Infra.Service.Client;
using PureCloud.Utils.Infra.Service.Storage;

namespace PureCloud.Utils.Function
{
    public static class RecordingDownload
    {
        [FunctionName("RecordingDownload")]
        [ExceptionFilter(Name = "ConversationRecovery")]
        public async static Task Run(
            [TimerTrigger("*/1 * * * * *", RunOnStartup = true)]TimerInfo myTimer, 
            ILogger log)
        {
            log.LogInformation($"Started 'RecordingDownload': {DateTime.Now}");

            // TODO 5. get not processed "table.conversations"
            Conversation conversation = await TableStorageService.GetNoProcessedItemToConversationTableAsync();

            // TODO 6.  /api/v2/conversations/{conversationId}/recordings
            PureCloudClient purecloudClient = new PureCloudClient();
            await purecloudClient.GetAccessToken();

            // Post batch download conversations
            string jobId = await purecloudClient.BatchRecordingDownloadByConversation(conversation.ConversationId);
            // Get url for download by JobId
            Batch batch = await purecloudClient.GetJobRecordingDownloadResultByConversation(jobId);

            if (batch.CallRecordings != null)
            {
                if (batch.ErrorCount.Equals(0))
                {
                    foreach (var item in batch.CallRecordings)
                    {
                        await TableStorageService.AddToCallRecorginsTableAsync(item);

                        // TODO 7. download file and upload to "blob.callrecordings"
                        if (!string.IsNullOrEmpty(item.ResultUrl))
                        {
                            //byte[] blob = await WebClient.DownloadFileFromUrl(item.ResultUrl);
                            //await BlobStorageService.UploadBlobAsync(item.ConversationId, filenameextension, blob);
                            string filenameextension = item.RecordingId + ".ogg";
                            await BlobStorageService.CopyFromUrlToBlobStorage(item.ResultUrl, item.ConversationId, filenameextension);
                        }
                    }

                    // TODO 8. update "table.conversations" with uridownload
                    conversation.Processed = true;
                    await TableStorageService.UpdateConversationTableAsync(conversation);
                }
                else if (batch.CallRecordings.Count.Equals(0))
                {
                    await TableStorageService.AddToCallRecorginsTableAsync(
                        new CallRecording()
                        {
                            ConversationId = conversation.ConversationId,
                            JobId = jobId,
                            ErrorMsg = batch.CallRecordings[0].ErrorMsg
                        });
                }
            }
            else
            {
                await TableStorageService.AddToCallRecorginsTableAsync(
                    new CallRecording()
                    {
                        ConversationId = conversation.ConversationId,
                        JobId = jobId,
                        ErrorMsg = batch.ErrorMsg
                    });
            }

            log.LogInformation($"Ended 'RecordingDownload': {DateTime.Now}");
        }
    }
}
