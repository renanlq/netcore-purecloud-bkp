using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using PureCloudPlatform.Client.V2.Model;
using System;
using System.Collections.Generic;

namespace PureCloud.Utils.Domain.Models
{
    public class Conversation : TableEntity
    {
        [JsonProperty("conversationId")]
        public string ConversationId { get; set; }

        [JsonProperty("processed")]
        public bool Processed { get; set; }

        [JsonProperty("tentatives")]
        public int Tentatives { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    public class ConversationResponse
    {
        [JsonProperty("conversations")]
        public List<Conversation> Conversations { get; set; }
    }
}