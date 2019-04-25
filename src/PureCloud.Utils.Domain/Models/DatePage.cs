using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;

namespace PureCloud.Utils.Domain.Models
{
    public class DatePage : TableEntity
    {
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        public DatePage() { }

        public DatePage(string json)
        {
            DatePage dp = JsonConvert.DeserializeObject<DatePage>(json);

            this.Date = dp.Date;
            this.Page = dp.Page;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(new DatePage() {
                Date = this.Date,
                Page = this.Page
            });
        }
    }
}
