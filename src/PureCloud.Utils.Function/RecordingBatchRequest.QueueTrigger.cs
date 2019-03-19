using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PureCloud.Utils.Domain.Attribute;
using PureCloud.Utils.Infra.Service.Storage;
using System;
using System.Threading.Tasks;

namespace PureCloud.Utils.Function
{
    public static class RecordingBatchRequest
    {
        [FunctionName("RecordingBatchRequest")]
        [ExceptionFilter(Name = "RecordingBatchRequest")]
        public async static Task Run(
            [QueueTrigger("conversation", Connection = "AzureWebJobsStorage")]string queuedConversation, 
            ILogger log)
        {
            log.LogInformation($"Started 'RecordingBatchRequest': {DateTime.Now}");

            // TODO 2. Requisitar o batch de grava��es(100 em 100 do dia do passo 1): /api/v2/recording/batchrequests
            // https://developer.mypurecloud.com/api/rest/v2/recording/#postRecordingBatchrequests

            // TODO 3. ler "queue.conversations", equanto tiver item

            // TODO 4. salvo o id do batch no fila "queue.job"
            await QueueStorageService.AddToJobQueueAsync(	
                JsonConvert.SerializeObject($"job-{queuedConversation}"), TimeSpan.FromSeconds(new Random().Next(30)));	

            log.LogInformation($"Ended 'RecordingBatchRequest': {DateTime.Now}");
        }
    }
}