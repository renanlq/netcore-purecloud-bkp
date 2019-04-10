using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace PureCloud.Utils.Domain.Models
{
    public class Result : TableEntity
    {
        [JsonProperty("jobId")]
        public string JobId { get; set; }

        [JsonProperty("conversationId")]
        public string ConversationId { get; set; }

        [JsonProperty("recordingId")]
        public string RecordingId { get; set; }

        [JsonProperty("resultUrl")]
        public string ResultUrl { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("errorMsg")]
        public string ErrorMsg { get; set; }
    }
}