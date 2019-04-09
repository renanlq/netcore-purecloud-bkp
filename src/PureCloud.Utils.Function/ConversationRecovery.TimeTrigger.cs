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
            [TimerTrigger("*/1 * * * * *", RunOnStartup = true)]TimerInfo myTimer, 
            ILogger log)
        {
            log.LogInformation($"Started 'ConversationRecovery': {DateTime.Now}");

            // TODO 1. get last processed date on "table.processeddates"
            ProcessedDate processedDate = await TableStorageService.GetLastProcessedDateTableAsync();
            processedDate = processedDate ?? (_ = new ProcessedDate() { Date = new DateTime(2016, 06, 08) }); // inicio 2016-06-08

            if (processedDate.Date < DateTime.Now.Date)
            {
                // TODO 2. /api/v2/analytics/conversations/details/query
                PureCloudClient purecloudClient = new PureCloudClient();
                await purecloudClient.GetAccessToken();

                // TODO 3. add to "table.conversations"
                List<Conversation> conversations = await purecloudClient.GetConversationsByInterval(processedDate.Date, processedDate.Date);
                if (conversations != null)
                {
                    foreach (var item in conversations)
                        await TableStorageService.AddToConversationTableAsync(item);
                }
            }

            // TODO 4. add new date to "table.processeddates"
            await TableStorageService.AddToProcessedDatesTableAsync(new ProcessedDate()
            {
                Date = processedDate.Date.AddDays(1)
            });

            log.LogInformation($"Ended 'ConversationRecovery': {DateTime.Now}");
        }
    }
}