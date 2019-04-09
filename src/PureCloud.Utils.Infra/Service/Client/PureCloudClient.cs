using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PureCloud.Utils.Domain.Interfaces.Services;
using PureCloud.Utils.Domain.Models;
using PureCloudPlatform.Client.V2.Extensions;
using PureCloudPlatform.Client.V2.Model;

namespace PureCloud.Utils.Infra.Service.Client
{

    public class PureCloudClient : IPureCloudService
    {
        private string _token;
        private readonly string _uribase = "https://api.mypurecloud.com";

        public PureCloudClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback += (s, c, h, e) => true;
        }

        public async Task GetAccessToken()
        {
            AuthTokenInfo access_tokenInfo = null;
            using (HttpClient hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Authorization",
                    $"Basic YjIwOGZjYzUtZDUyOS00ZjY1LThmM2EtZmM4YmZjNDhjOGJmOndkWm1MMGUwV0JEM1NlMTNXNnczdEwyNVlUdmFVRE9ZLXEyQ0VyaU4xcEE=");
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                Dictionary<string, string> dc = new Dictionary<string, string>();
                dc.Add("grant_type", "client_credentials");

                FormUrlEncodedContent content = new FormUrlEncodedContent(dc);

                HttpResponseMessage responseMessage = await hc.PostAsync("https://login.mypurecloud.com/oauth/token", content);

                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    string jsonMessage = await responseMessage.Content.ReadAsStringAsync();
                    access_tokenInfo = JsonConvert.DeserializeObject<AuthTokenInfo>(jsonMessage);
                }
            }

            _token = access_tokenInfo?.AccessToken;
        }

        /// <summary>
        /// Get all conversations by interval.
        /// Link: https://developer.mypurecloud.com/api/rest/v2/conversations/#post-api-v2-analytics-conversations-details-query
        /// </summary>
        /// <param name="begin">Datetime, begin date of interval</param>
        /// <param name="end">Datetime, end date of interval</param>
        /// <returns>List of purecloud conversations</returns>
        public async Task<List<Domain.Models.Conversation>> GetConversationsByInterval(DateTime begin, DateTime end)
        {
            ConversationQuery queryParam = new ConversationQuery()
            {
                //Interval = "2019-01-01T00:00:00.000Z/2019-01-31T23:59:59.999Z",
                Interval = $"{begin.ToString("yyyy-MM-dd")}T00:00:00.000Z/{end.ToString("yyyy-MM-dd")}T23:59:59.999Z",
                Order = ConversationQuery.OrderEnum.Asc,
                OrderBy = ConversationQuery.OrderByEnum.Conversationstart
            };
            Domain.Models.ConversationResponse response = new Domain.Models.ConversationResponse();

            using (HttpClient hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_token}");
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                HttpResponseMessage responseMessage = await hc.PostAsync(_uribase + "/api/v2/analytics/conversations/details/query",
                      new StringContent(queryParam.ToJson(), Encoding.UTF8, "application/json"));

                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    string jsonMessage = await responseMessage.Content.ReadAsStringAsync();
                    response = JsonConvert.DeserializeObject<Domain.Models.ConversationResponse>(jsonMessage);
                }
            }

            return response.Conversations;
        }

        /// <summary>
        /// Batch download recordings.
        /// Link: https://developer.mypurecloud.com/api/rest/v2/recording/#post-api-v2-recording-batchrequests
        /// </summary>
        /// <param name="conversationId">String, conversation id</param>
        /// <returns>String, Job id</returns>
        public async Task<string> BatchRecordingDownloadByConversation(string conversationId)
        {
            int count = 0;
            BatchDownloadJobSubmission queryParam = new BatchDownloadJobSubmission {
                BatchDownloadRequestList = new List<BatchDownloadRequest>() {
                new BatchDownloadRequest() { ConversationId = conversationId }
            }};

            BatchDownloadJobSubmissionResult response = new BatchDownloadJobSubmissionResult();

            using (HttpClient hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_token}");
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                HttpResponseMessage responseMessage = new HttpResponseMessage();
                do
                {
                    responseMessage = await hc.PostAsync(_uribase + "/api/v2/recording/batchrequests",
                    new StringContent(queryParam.ToJson(), Encoding.UTF8, "application/json"));

                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        string jsonMessage = await responseMessage.Content.ReadAsStringAsync();
                        response = JsonConvert.DeserializeObject<BatchDownloadJobSubmissionResult>(jsonMessage);
                    }

                    count++;
                } while (responseMessage.StatusCode == HttpStatusCode.Accepted || count < 3 
                    && responseMessage.StatusCode != HttpStatusCode.OK);
            }

            return response.Id;
        }

        /// <summary>
        /// Check job batch recordings download results.
        /// Link: https://developer.mypurecloud.com/api/rest/v2/recording/#get-api-v2-recording-batchrequests--jobId-
        /// </summary>
        /// <param name="conversationId">String, conversation id</param>
        /// <returns>Batch response</returns>
        public async Task<Domain.Models.Batch> GetJobRecordingDownloadResultByConversation(string jobId)
        {
            Domain.Models.Batch response = new Domain.Models.Batch();

            using (HttpClient hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_token}");
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                HttpResponseMessage responseMessage = new HttpResponseMessage();
                do
                {
                    Task.Delay(1000).Wait();
                    responseMessage = await hc.GetAsync(_uribase + $"/api/v2/recording/batchrequests/{jobId}");

                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        string jsonMessage = await responseMessage.Content.ReadAsStringAsync();
                        response = JsonConvert.DeserializeObject<Domain.Models.Batch>(jsonMessage);
                    }

                } while (!response.ExpectedResultCount.Equals(response.ResultCount));
            }

             return response;
        }
    }
}