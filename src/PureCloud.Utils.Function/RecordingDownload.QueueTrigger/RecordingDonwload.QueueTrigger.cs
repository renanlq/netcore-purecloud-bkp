using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace PureCloud.Utils.Function
{
    public static class RecordingDownload
    {
        [FunctionName("LogQueueTriggerFunction")]
        public async static Task Run([QueueTrigger("jobs", Connection = "AzureWebJobsStorage")]string queueLog, ILogger log)
        {
            log.LogInformation($"RecordingBulkDownload started: {DateTime.Now}");

            log.LogInformation($"RecordingBulkDownload ended: {DateTime.Now}");
        }
    }
}