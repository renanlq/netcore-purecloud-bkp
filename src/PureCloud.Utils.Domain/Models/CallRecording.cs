using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace PureCloud.Utils.Domain.Models
{
    public class CallRecording : TableEntity
    {
        [JsonProperty("jobId")]
        public string JobId { get; set; }

        [JsonProperty("recordingId")]
        public string RecordingId { get; set; }

        [JsonProperty("conversationId")]
        public string ConversationId { get; set; }
        
        [JsonProperty("callRecordingJson")]
        public string CallRecordingJson { get; set; }

        [JsonProperty("errorMsg")]
        public string ErrorMsg { get; set; }
    }
}