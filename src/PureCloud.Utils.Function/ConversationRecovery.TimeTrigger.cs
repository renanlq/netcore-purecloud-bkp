using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PureCloud.Utils.Domain.Attribute;
using PureCloud.Utils.Infra.Service.Storage;
using System;
using System.Threading.Tasks;

namespace PureCloud.Utils.Function 
{
    public static class ConversationRecovery
    {
        [FunctionName("ConversationRecovery")]
        [ExceptionFilter(Name = "ConversationRecovery")]
        public async static Task Run(
            [TimerTrigger("*/10 * * * * *", RunOnStartup = true)]TimerInfo myTimer,
            ILogger log)
        {
            log.LogInformation($"Started 'ConversationRecovery': {DateTime.Now}");

            // TODO 1. api/v2/analytics/conversations/details/query
            // https://developer.mypurecloud.com/api/rest/v2/conversations/index.html

            // TODO 2. add to "queue.conversation"
            await QueueStorageService.AddToConversationQueueAsync(
                JsonConvert.SerializeObject($"conversation-{DateTime.Now}"), 
                                            TimeSpan.FromSeconds(new Random().Next(30)));

            log.LogInformation($"Ended 'ConversationRecovery': {DateTime.Now}");
        }
    }
}