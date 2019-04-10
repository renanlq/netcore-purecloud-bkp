using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace PureCloud.Utils.Domain.Models
{
    public class ProcessedDate : TableEntity
    {
        public DateTime Date { get; set; }

        public static ProcessedDate ReturnDateToProcess(ProcessedDate processedDate)
        {
            if (processedDate == null) // begin default: 2016-06-08
                processedDate = new ProcessedDate() { Date = new DateTime(2016, 06, 08) };
            else
                processedDate.Date = processedDate.Date.AddDays(1);

            return processedDate;
        }
    }
}
