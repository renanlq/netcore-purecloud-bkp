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
            [TimerTrigger("* */1 * * * *", RunOnStartup = true)]TimerInfo myTimer, 
            ILogger log)
        {
            log.LogInformation($"{DateTime.Now}: Started 'ConversationRecovery'");

            // TODO 1. get last processed date on "table.processeddates"
            ProcessedDate processedDate = await TableStorageService.GetLastProcessedDateTableAsync();
            processedDate = processedDate ?? new ProcessedDate() { Date = new DateTime(2016, 06, 08) }; // inicio 2016-06-08
            DateTime limitDate = new DateTime(2016, 06, 10); //DateTime.Now.Date)

            if (processedDate.Date < limitDate.Date)
            {
                // TODO 2. /api/v2/analytics/conversations/details/query
                PureCloudClient purecloudClient = new PureCloudClient();
                await purecloudClient.GetAccessToken();

                // TODO 3. add to "table.conversations"
                List<Conversation> conversations = await purecloudClient.GetConversationsByInterval(
                    processedDate.Date, processedDate.Date);
                
                if (conversations != null)
                {
                    foreach (var item in conversations)
                        await TableStorageService.AddToConversationTableAsync(item);
                }

                // TODO 4. add new date to "table.processeddates"
                await TableStorageService.AddToProcessedDatesTableAsync(new ProcessedDate()
                {
                    Date = processedDate.Date.AddDays(1)
                });
            }

            log.LogInformation($"{DateTime.Now}: Ended 'ConversationRecovery'");
        }
    }
}