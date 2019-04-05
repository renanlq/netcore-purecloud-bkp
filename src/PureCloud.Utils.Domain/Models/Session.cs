using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace PureCloud.Utils.Domain.Models
{
    public class Session : TableEntity
    {
        [JsonProperty("mediaType")]
        public string MediaType { get; set; }

        [JsonProperty("sessionId")]
        public string SessionId { get; set; }

        [JsonProperty("ani")]
        public string Ani { get; set; }

        [JsonProperty("direction")]
        public string Direction { get; set; }

        [JsonProperty("dnis")]
        public string Dnis { get; set; }

        [JsonProperty("sessionDnis")]
        public string SessionDnis { get; set; }

        [JsonProperty("edgeId")]
        public string EdgeId { get; set; }

        [JsonProperty("remoteNameDisplayable")]
        public string RemoteNameDisplayable { get; set; }

        [JsonProperty("recording")]
        public string Recording { get; set; }
    }
}
