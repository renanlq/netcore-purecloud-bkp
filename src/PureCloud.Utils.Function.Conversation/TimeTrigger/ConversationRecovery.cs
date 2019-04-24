using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PureCloud.Utils.Domain.Models;
using PureCloud.Utils.Infra.Service.Client;
using PureCloud.Utils.Infra.Service.Storage;
using PureCloudPlatform.Client.V2.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PureCloud.Utils.Function.TimeTrigger
{
    public static class ConversationRecovery
    {
        private static readonly string _appversion = Environment.GetEnvironmentVariable("appversion", EnvironmentVariableTarget.Process);

        [FunctionName("ConversationRecovery")]
        public async static Task Run(
            [TimerTrigger("*/1 * * * * *", RunOnStartup = false)]TimerInfo myTimer,
            ILogger log)
        {
            try
            {
                // TODO 1. get last processed date on "table.processeddates"
                DateTime limitDate = DateTime.Now;
                ProcessedDate processedDate = await TableStorageService.GetLastProcessedDateAsync();
                processedDate = ProcessedDate.ReturnDateToProcess(processedDate);

                if (processedDate.Date < limitDate.Date)
                {
                    // TODO 4. update processdate page +1
                    await TableStorageService.SaveProcessedDateAsync(processedDate);

                    // TODO 2. /api/v2/analytics/conversations/details/query
                    PureCloudClient purecloudClient = new PureCloudClient();
                    await purecloudClient.GetAccessToken();

                    // TODO 3. add to "table.conversations"
                    List<AnalyticsConversation> conversations = await purecloudClient.GetConversationsByInterval(
                        processedDate.Date, processedDate.Date, processedDate.Page);

                    log.LogInformation($"Processing date: {processedDate.Date}, with {conversations.Count} conversations");

                    if (!conversations.Count.Equals(0))
                    {
                        foreach (var item in conversations)
                        {
                            await TableStorageService.AddConversationAsync(
                                new Domain.Models.Conversation()
                                {
                                    ConversationId = item.ConversationId,
                                    Processed = false,
                                    Tentatives = 0,
                                    Version = _appversion
                                });
                            await BlobStorageService.AddToConvesrationAsync(
                                JsonConvert.SerializeObject(item), item.ConversationId, $"conversation-{item.ConversationId}.json");
                        }
                        processedDate.Total += conversations.Count;

                        // TODO 4. add processed date to "table.processeddates"
                        await TableStorageService.SaveProcessedDateAsync(processedDate);
                    }
                    else
                    {
                        // check if "someone" (function) added future date before "me"
                        ProcessedDate processedDatebase = await TableStorageService.GetLastProcessedDateAsync();
                        if (processedDatebase.Date.Equals(processedDate.Date))
                        {
                            // go to next date
                            processedDate = new ProcessedDate()
                            {
                                Page = 0,
                                Total = 0,
                                Date = processedDate.Date.AddDays(1)
                            };

                            // TODO 4. add processed date to "table.processeddates"
                            await TableStorageService.SaveProcessedDateAsync(processedDate);
                        }
                    }
                }
                else
                {
                    log.LogInformation($"Pass limit date");
                }
            }
            catch (Exception ex)
            {
                log.LogInformation($"Exception: {ex.Message}");
                await BlobStorageService.AddToErrorAsync(
                    JsonConvert.SerializeObject(ex), "exception", $"{DateTime.Now}.json");
            }
        }
    }
}