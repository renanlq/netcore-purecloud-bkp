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
            [TimerTrigger("* */1 * * * *", RunOnStartup = true)]TimerInfo myTimer, 
            ILogger log)
        {
            log.LogInformation($"Started 'ConversationRecovery' function");

            // TODO 1. get last processed date on "table.processeddates"
            DateTime limitDate = DateTime.Now;
            ProcessedDate processedDate = await TableStorageService.GetLastProcessedDateTableAsync();
            processedDate = ProcessedDate.ReturnDateToProcess(processedDate);

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
                    log.LogInformation($"Processing date: {processedDate.Date}, with {conversations.Count} conversations");

                    foreach (var item in conversations)
                    {
                        // table storage with 1 level information
                        item.ParticipantsJson = JsonConvert.SerializeObject(item.Participants);
                        await TableStorageService.AddToConversationTableAsync(item);
                    }
                }

                // TODO 4. add processed date to "table.processeddates"
                await TableStorageService.AddToProcessedDatesTableAsync(processedDate);
            }
            else
            {
                log.LogInformation($"Pass limit date");
            }

            log.LogInformation($"Ended 'ConversationRecovery' function");
        }
    }
}