using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace PureCloud.Utils.Function
{
    public static class RecordingBatchRequest
    {
        [FunctionName("RecordingBatchRequest")]
        public async static Task Run(
            [QueueTrigger("job", Connection = "AzureWebJobsStorage")]string queueLog, 
            ILogger log)
        {
            log.LogInformation($"Started 'RecordingBatchRequest': {DateTime.Now}");

            log.LogInformation($"Ended 'RecordingBatchRequest': {DateTime.Now}");
        }
    }
}