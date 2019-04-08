using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace PureCloud.Utils.Domain.Models
{
    public class Participant : TableEntity
    {
        [JsonProperty("participantId")]
        public string ParticipantId { get; set; }

        [JsonProperty("participantName")]
        public string ParticipantName { get; set; }

        [JsonProperty("purpose")]
        public string Purpose { get; set; }

        [JsonProperty("sessions")]
        public List<Session> Sessions { get; set; }
    }
}
