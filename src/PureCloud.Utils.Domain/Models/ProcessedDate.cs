using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace PureCloud.Utils.Domain.Models
{
    public class ProcessedDate : TableEntity
    {
        public DateTime Date { get; set; }
    }
}
