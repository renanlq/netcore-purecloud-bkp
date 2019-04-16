using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace PureCloud.Utils.Domain.Models
{
    public class User : TableEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("departament")]
        public string Departament { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("manager.id")]
        public string ManagerId { get; set; }

        [JsonProperty("manager.name")]
        public string ManagerName { get; set; }

        [JsonProperty("chat.jabberId")]
        public string Chat { get; set; }

        [JsonProperty("division.id")]
        public string DivisionId { get; set; }

        [JsonProperty("division.name")]
        public string DivisionName { get; set; }
    }
}
