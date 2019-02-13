using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PureCloud.Utils.Function 
{
    public static class RecordingBulkDownload
    {
        [FunctionName("RecordingBulkDownload")]
        public async static Task Run([TimerTrigger("* * * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"RecordingBulkDownload started: {DateTime.Now}");

            

            log.LogInformation($"RecordingBulkDownload ended: {DateTime.Now}");
        }
    }
}