using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace PureCloud.Utils.Function
{
    public static class RecordingDonwload
    {
        [FunctionName("RecordingDonwload.TimeTrigger")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo RecordingDonwload, ILogger log)
        {
            log.LogInformation($"Started 'RecordingDownload': {DateTime.Now}");

            // TODO 5. ler "table.conversations", sem uridownload
            // TODO 6. buscar "callrecording" no purecloud
            // TODO 7. atualizar "table.conversations", com uridownload
            // TODO 8. salvar recodingaudio em "blob.callrecordings"

            log.LogInformation($"Ended 'RecordingDownload': {DateTime.Now}");
        }
    }
}
