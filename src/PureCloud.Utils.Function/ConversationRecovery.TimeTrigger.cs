using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
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

            // TODO 1. pegar da "table.processeddates" adicionar 1 a data p/ processar
            ProcessedDate processedDate = await TableStorageService.GetLastDateToProcessedDatesTableAsync();
            processedDate = processedDate ?? (_ = new ProcessedDate() { Date = new DateTime(2016, 06, 08) }); // inicio 2016-06-08

            // TODO 2. /api/v2/analytics/conversations/details/query
            // https://developer.mypurecloud.com/api/rest/v2/conversations/
            PureCloudClient purecloudClient = new PureCloudClient();
            await purecloudClient.GetAccessToken();

            if (processedDate.Date < DateTime.Now.Date)
            {
                List<Conversation> result = await purecloudClient.GetConversationsByInterval(DateTime.Now, DateTime.Now);

                // TODO 3. add to "table.conversations"
                foreach (var item in result)
                    await TableStorageService.AddToConversationTableAsync(item);
            }

            // TODO 4. add new date to "table.processeddates"
            await TableStorageService.AddToProcessedDatesTableAsync(new ProcessedDate() {
                Date = processedDate.Date.AddDays(1)
            });

            log.LogInformation($"Ended 'ConversationRecovery': {DateTime.Now}");
        }
    }
}