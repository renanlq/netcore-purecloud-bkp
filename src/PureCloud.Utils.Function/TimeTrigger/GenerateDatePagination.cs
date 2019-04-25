using System;
using System.Linq;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using PureCloud.Utils.Domain.Models;

namespace PureCloud.Utils.Function.TimeTrigger
{
    public static class GenerateDatePagination
    {
        [FunctionName("GenerateDatePagination")]
        public static async System.Threading.Tasks.Task RunAsync(
            [TimerTrigger("*/1 * * * * *")]TimerInfo myTimer,
            [ServiceBus("pagequeue", Connection = "ServiceBusConnectionString", EntityType = EntityType.Queue)] IAsyncCollector<string> pageQueue,
            [Table("datepage", Connection = "StorageConnectionString")] CloudTable dateTable,
            ILogger log)
        {
            try
            {
                //await dateTable.CreateIfNotExistsAsync(); // create before execution, to avoid overhead proccess

                // TODO: read from date page table
                TableQuery<DatePage> query = new TableQuery<DatePage>();
                var results = await dateTable.ExecuteQuerySegmentedAsync(query, null);
                DatePage actualDate = results.OrderByDescending(r => r.Timestamp).FirstOrDefault();

                if (actualDate == null)
                {
                    actualDate = new DatePage()
                    {
                        Date = DateTime.Parse(Environment.GetEnvironmentVariable("initialdate")),
                        Page = 1
                    };
                }
                else
                {
                    actualDate.Date = actualDate.Date.AddDays(1);
                }

                // TODO: add day in "pageQueue" and go on, until datetime.now
                if (DateTime.Now > actualDate.Date)
                {
                    await pageQueue.AddAsync(new DatePage() {
                        Date = actualDate.Date.AddDays(1),
                        Page = 1
                    }.ToJson());

                    TableOperation operation = TableOperation.Insert(actualDate);
                    operation.Entity.PartitionKey = "purecloud";
                    operation.Entity.RowKey = Guid.NewGuid().ToString();                  
                    await dateTable.ExecuteAsync(operation);

                    log.LogInformation($"Added date: {actualDate.Date} to datepage table");
                }
            }
            catch (Exception ex)
            {
                TelemetryClient telemetry = new TelemetryClient();
                telemetry.InstrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");
                telemetry.TrackException(ex);

                log.LogInformation($"Exception during execution: {ex.Message}");
            }
        }
    }
}
