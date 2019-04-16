using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PureCloud.Utils.Infra.Service.Client;
using System.Collections.Generic;
using PureCloudPlatform.Client.V2.Model;

namespace HttpTrigger
{
    public static class DownloadUsers
    {
        [FunctionName("DownloadUsers")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "v1/users")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"Started 'DownloadUsers' function");

            PureCloudClient purecloudClient = new PureCloudClient();
            await purecloudClient.GetAccessToken();

            List<User> users = await purecloudClient.GetAvailableUsers();

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");

            //log.LogInformation($"Ended 'DownloadUsers' function");
        }
    }
}
