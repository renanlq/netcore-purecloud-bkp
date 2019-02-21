using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PureCloud.Utils.Domain.Attribute;
using PureCloud.Utils.Service;
using System;
using System.Threading.Tasks;

namespace PureCloud.Utils.Function
{
    public static class RecordingDownload
    {
        [FunctionName("RecordingDownload")]
        [ExceptionFilter(Name = "RecordingDownload")]
        public async static Task Run(
            [QueueTrigger("job", Connection = "AzureWebJobsStorage")]string queuedJob,
            ILogger log)
        {
            log.LogInformation($"Started 'RecordingDownload': {DateTime.Now}");

            // TODO 5. ler "queue.job", equanto tiver item

            // TODO 6. salvar uri disponibilizada em storage "blob.callrecording"
                // TODO 6.1. se response não completo TENTAR NOVAMENTE em 10 min



            log.LogInformation($"Ended 'RecordingDownload': {DateTime.Now}");
        }
    }
}