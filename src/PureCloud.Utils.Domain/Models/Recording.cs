using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace PureCloud.Utils.Domain.Models
{    
    public class Recording : TableEntity
    {
        [JsonProperty("id")]
        public int Id {get; set;}

        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("selfUri")]
        public string SelfUri {get; set;}
    }
}