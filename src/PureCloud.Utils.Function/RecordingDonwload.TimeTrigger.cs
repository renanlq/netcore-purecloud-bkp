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

            // TODO 4. ler "table.conversations", sem uridownload
            // TODO 5. buscar "callrecording" no purecloud
            // TODO 6. atualizar "table.conversations", com uridownload
            // TODO 7. salvar recodingaudio em "blob.callrecordings"

            log.LogInformation($"Ended 'RecordingDownload': {DateTime.Now}");
        }
    }
}
