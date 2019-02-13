using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace PureCloud.Utils.Function 
{
    public static class RecordingBulkDownload
    {
        private static string apiURI = Environment.GetEnvironmentVariable("PureCloudApi:URI", EnvironmentVariableTarget.Process);

        [FunctionName("RecordingBulkDownload")]
        public async static Task Run([TimerTrigger("* * * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"RecordingBulkDownload started: {DateTime.Now}");

            // get token

            // 1. api/v2/analytics/conversations/details/query

            // 2. add to queue conversation

            // equanto tiver conversation na fila

                // pego 100
                
                // 3. 4. salvo o id do batch no fila job
                
                // removo os 100

            //

            log.LogInformation($"RecordingBulkDownload ended: {DateTime.Now}");
        }
    }
}