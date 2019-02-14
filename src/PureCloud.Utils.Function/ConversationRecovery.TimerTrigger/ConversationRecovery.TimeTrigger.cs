using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PureCloud.Utils.Domain.Attribute;
using PureCloud.Utils.Service.Queue;
using System;
using System.Threading.Tasks;

namespace PureCloud.Utils.Function 
{
    public static class ConversationRecovery
    {
        private static string apiURI = Environment.GetEnvironmentVariable("PureCloudApi:URI", EnvironmentVariableTarget.Process);

        [FunctionName("ConversationRecovery")]
        [ExceptionFilter(Name = "ConversationRecovery")]
        public async static Task Run(
            [TimerTrigger("*/10 * * * * *", RunOnStartup = true)]TimerInfo myTimer,
            ILogger log)
        {
            log.LogInformation($"Started 'ConversationRecovery': {DateTime.Now}");

            // get token

            // 1. api/v2/analytics/conversations/details/query

            // 2. add to queue conversation
            var item = $"{DateTime.Now}";

            await QueueStorageService.AddToConversationQueueAsync(
                JsonConvert.SerializeObject($"conversation-{item}"), TimeSpan.FromSeconds(new Random().Next(30)), log);

            log.LogInformation($"Ended 'ConversationRecovery': {DateTime.Now}");
        }
    }
}