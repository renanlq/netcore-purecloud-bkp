using Newtonsoft.Json;
using System.Collections.Generic;

namespace PureCloud.Utils.Domain.Models
{
    public class Batch
    {
        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("JobId")]
        public string JobId { get; set; }

        [JsonProperty("expectedResultCount")]
        public string ExpectedResultCount { get; set; }

        [JsonProperty("resultCount")]
        public int? ResultCount { get; set; }

        [JsonProperty("errorCount")]
        public int? ErrorCount { get; set; }

        [JsonProperty("errorMsg")]
        public string ErrorMsg { get; set; }

        [JsonProperty("results")]
        public List<CallRecording> CallRecordings { get; set; }
    }
}