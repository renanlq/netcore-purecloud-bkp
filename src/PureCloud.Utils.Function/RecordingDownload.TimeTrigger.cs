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

            // TODO 5. ler "table.conversations", não processadas
            Conversation conversation = await TableStorageService.GetNoProcessedItemToConversationTableAsync();

            // TODO 6.  /api/v2/conversations/{conversationId}/recordings
            PureCloudClient purecloudClient = new PureCloudClient();
            await purecloudClient.GetAccessToken();

            string jobId = await purecloudClient.BatchRecordingDownloadByConversation(conversation.ConversationId);
            Batch batch = await purecloudClient.GetJobRecordingDownloadResultByConversation(jobId);

            if (!batch.CallRecordings.Count.Equals(0))
            {
                foreach (var item in batch.CallRecordings)
                {
                    await TableStorageService.AddToCallRecorginsTableAsync(item);

                    // TODO 7. baixar arquivo e salvar em "blob.callrecordings"
                    byte[] blob = await WebClient.DownloadFileFromUrl(item.ResultUrl);
                    await BlobStorageService.UploadBlobAsync(item.ConversationId, item.ConversationId, blob);
                }

                // TODO 8. atualizar "table.conversations", com uridownload
                conversation.Processed = true;
                await TableStorageService.UpdateConversationTableAsync(conversation);
            }
            else
            {
                await TableStorageService.AddToCallRecorginsTableAsync(
                    new CallRecording() {
                        ConversationId = conversation.ConversationId,
                        JobId = jobId,
                        ErrorMsg = batch.CallRecordings[0].ErrorMsg
                    });
            }

            log.LogInformation($"Ended 'RecordingDownload': {DateTime.Now}");
        }
    }
}
