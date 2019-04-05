using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PureCloud.Utils.Domain.Attribute;
using PureCloud.Utils.Domain.Models;
using PureCloud.Utils.Infra.Service.Client;
using PureCloud.Utils.Infra.Service.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PureCloud.Utils.Function 
{
    public static class ConversationRecovery
    {
        [FunctionName("ConversationRecovery")]
        [ExceptionFilter(Name = "ConversationRecovery")]
        public async static Task Run(
            [TimerTrigger("*/5 * * * * *", RunOnStartup = true)]TimerInfo myTimer,
            ILogger log)
        {
            log.LogInformation($"Started 'ConversationRecovery': {DateTime.Now}");

            // TODO 1. pegar da "table.conversations" não "processed" e adicionar 1 a data p/ processar
            DateTime data = new DateTime();

            // TODO 2. /api/v2/analytics/conversations/details/query
            // https://developer.mypurecloud.com/api/rest/v2/conversations/
            PureCloudClient purecloudClient = new PureCloudClient();
            await purecloudClient.GetAccessToken();

            List<Conversation> result = new List<Conversation>();
            if (data.Date < DateTime.Now.Date)
            {
                result = await purecloudClient.GetConversationsByInterval(DateTime.Now, DateTime.Now);

                // TODO 3. add to "table.conversations"
                foreach (var item in result)
                    await TableStorageService.AddToConversationTableAsync(item);
            }
            
            log.LogInformation($"Ended 'ConversationRecovery': {DateTime.Now}");
        }
    }
}