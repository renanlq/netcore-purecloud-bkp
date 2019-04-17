using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace PureCloud.Utils.Domain.Models
{
    public class User : TableEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
