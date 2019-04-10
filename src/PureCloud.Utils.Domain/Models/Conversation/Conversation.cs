using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PureCloud.Utils.Domain.Models
{
    public class Conversation : TableEntity
    {
        [JsonProperty("conversationId")]
        public string ConversationId { get; set; }

        [JsonProperty("conversationStart")]
        public DateTime? ConversationStart { get; set; }

        [JsonProperty("conversationEnd")]
        public DateTime? ConversationEnd { get; set; }

        [JsonProperty("participants")]
        public List<Participant> Participants { get; set; }

        public string ParticipantsJson { get; set; }

        [JsonProperty("divisionIds")]
        public List<string> DivisionIds { get; set; }

        [JsonProperty("processed")]
        public bool Processed { get; set; }
    }

    public class ConversationResponse
    {
        [JsonProperty("conversations")]
        public List<Conversation> Conversations { get; set; }
    }
}