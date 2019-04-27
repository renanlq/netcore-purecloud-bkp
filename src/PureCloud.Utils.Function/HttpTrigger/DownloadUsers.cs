using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using PureCloud.Utils.Infra.Service.Client;
using PureCloudPlatform.Client.V2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PureCloud.Utils.Function.HttpTrigger
{
    public static class DownloadUsers
    {
        [FunctionName("DownloadUsers")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/users")] HttpRequest req,
            [Blob("user", Connection = "StorageConnectionString")] CloudBlobContainer container,
            [Table("user", Connection = "StorageConnectionString")] CloudTable userTable,
            ILogger log)
        {
            try
            {
                //await userTable.CreateIfNotExistsAsync(); // create before execution, to avoid overhead proccess

                PureCloudClient purecloudClient = new PureCloudClient();
                await purecloudClient.GetAccessToken();

                List<User> users = await purecloudClient.GetAvailableUsers();

                if (!users.Count.Equals(0))
                {
                    log.LogInformation($"Total users to save: {users.Count}");

                    foreach (var user in users)
                    {
                        TableQuery<Domain.Models.User> query = new TableQuery<Domain.Models.User>()
                                 .Where(TableQuery.GenerateFilterCondition(
                                     "Id", QueryComparisons.Equal, user.Id));

                        var results = await userTable.ExecuteQuerySegmentedAsync(query, null);
                        Domain.Models.User userDb = results.Results.FirstOrDefault();
                        if (userDb == null)
                        {
                            TableOperation operation = TableOperation.Insert(new Domain.Models.User()
                            {
                                Id = userDb.Id,
                                Email = userDb.Email
                            });
                            operation.Entity.PartitionKey = "";
                            operation.Entity.RowKey = Guid.NewGuid().ToString();

                            await userTable.ExecuteAsync(operation);

                            var cloudBlockBlob = container.GetBlockBlobReference($"{user.Id}.json");
                            await cloudBlockBlob.UploadTextAsync(JsonConvert.SerializeObject(user));
                        }
                    }
                }

                return (ActionResult)new OkResult();
            }
            catch (Exception ex)
            {
                TelemetryClient telemetry = new TelemetryClient();
                telemetry.InstrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");
                telemetry.TrackException(ex);

                return (ActionResult) new StatusCodeResult(500);
            }
        }
    }
}