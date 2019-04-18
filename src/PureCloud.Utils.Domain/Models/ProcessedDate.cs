using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;

namespace PureCloud.Utils.Domain.Models
{
    public class ProcessedDate : TableEntity
    {
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        public static ProcessedDate ReturnDateToProcess(ProcessedDate processedDate)
        {
            if (processedDate == null) // begin default: 2016-06-08
            {
                return new ProcessedDate()
                {
                    Date = new DateTime(2016, 06, 08),
                    Page = 1,
                    Total = 0
                };
            }
            else
            {
                processedDate.Page++;
            }

            return processedDate;
        }
    }
}
