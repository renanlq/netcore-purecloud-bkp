using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PureCloud.Utils.Domain.Attribute;
using PureCloud.Utils.Service.Queue;
using System;
using System.Threading.Tasks;

namespace PureCloud.Utils.Function 
{
    public static class RecordingBulkDownload
    {
        private static string apiURI = Environment.GetEnvironmentVariable("PureCloudApi:URI", EnvironmentVariableTarget.Process);

        [FunctionName("RecordingBulkDownload")]
        [ExceptionFilter(Name = "RecordingBulkDownload")]
        public async static Task Run(
            [TimerTrigger("*/10 * * * * *", RunOnStartup = true)]TimerInfo myTimer, 
            ILogger log)
        {
            log.LogInformation($"Started 'RecordingBulkDownload': {DateTime.Now}");

            // get token

            // 1. api/v2/analytics/conversations/details/query

            // 2. add to queue conversation
            var item = $"convesation-{DateTime.Now}";
            
            await QueueStorageService.AddToConversationQueueAsync(
                JsonConvert.SerializeObject(item), TimeSpan.FromSeconds(new Random().Next(10)), log);

            // ler conversations da queue, equanto tiver item
            if (true)
            {
                // 3. 4. salvo o id do batch no fila job
                await QueueStorageService.AddToJobQueueAsync(
                    JsonConvert.SerializeObject($"job-{DateTime.Now}"), TimeSpan.FromSeconds(new Random().Next(10)));
                
                // removo o item i
                await QueueStorageService.DeleteToConversationQueueAsync(item, TimeSpan.FromSeconds(new Random().Next(10)));
            }

            log.LogInformation($"Ended 'RecordingBulkDownload': {DateTime.Now}");
        }
    }
}