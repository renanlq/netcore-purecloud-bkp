using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PureCloud.Utils.Domain.Interfaces.Services;
using PureCloudPlatform.Client.V2.Extensions;
using PureCloudPlatform.Client.V2.Model;

namespace PureCloud.Utils.Infra.Service.Client
{

    public class PureCloudClient : IPureCloudService
    {
        private const int _pageSize = 100;
        private const int _pageUserSize = 500;

        private static readonly string _authorizarionToken = Environment.GetEnvironmentVariable("purecloud:authorization", EnvironmentVariableTarget.Process);
        private static readonly string _uribase = Environment.GetEnvironmentVariable("purecloud:uribase", EnvironmentVariableTarget.Process);
        private static readonly string _uritoken = Environment.GetEnvironmentVariable("purecloud:uritoken", EnvironmentVariableTarget.Process); 

        private string _token;

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
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {_authorizarionToken}");
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                Dictionary<string, string> dc = new Dictionary<string, string>();
                dc.Add("grant_type", "client_credentials");

                FormUrlEncodedContent content = new FormUrlEncodedContent(dc);

                HttpResponseMessage responseMessage = await hc.PostAsync(_uritoken, content);
                string jsonMessage = await responseMessage.Content.ReadAsStringAsync();
                if (responseMessage.IsSuccessStatusCode)
                {                  
                    access_tokenInfo = JsonConvert.DeserializeObject<AuthTokenInfo>(jsonMessage);
                }
                else {
                    throw new Exception(jsonMessage);
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
        /// <returns>List<AnalyticsConversation>, list of purecloud conversations</returns>
        public async Task<List<AnalyticsConversation>> GetConversationsByInterval(DateTime begin, DateTime end, int pageNumber)
        {
            List<AnalyticsConversation> result = new List<AnalyticsConversation>();

            using (HttpClient hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_token}");
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                AnalyticsConversationQueryResponse response = new AnalyticsConversationQueryResponse();
                ConversationQuery queryParam = new ConversationQuery()
                {
                    //Interval = "2019-01-01T00:00:00.000Z/2019-01-31T23:59:59.999Z",
                    Interval = $"{begin.ToString("yyyy-MM-dd")}T00:00:00.000Z/{end.ToString("yyyy-MM-dd")}T23:59:59.999Z",
                    Order = ConversationQuery.OrderEnum.Asc,
                    OrderBy = ConversationQuery.OrderByEnum.Conversationstart,
                    Paging = new PagingSpec()
                    {
                        PageSize = _pageSize,
                        PageNumber = pageNumber
                    }
                };

                HttpResponseMessage responseMessage = await hc.PostAsync(_uribase + "/api/v2/analytics/conversations/details/query",
                    new StringContent(queryParam.ToJson(), Encoding.UTF8, "application/json"));

                int tentatives = 0;
                do 
                {
                    string jsonMessage = await responseMessage.Content.ReadAsStringAsync();
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        response = JsonConvert.DeserializeObject<AnalyticsConversationQueryResponse>(jsonMessage);
                        if (response.Conversations != null) result = response.Conversations;
                    }
                    else if ((int)responseMessage.StatusCode >= 300 && (int)responseMessage.StatusCode < 600)
                    {
                        throw new Exception(jsonMessage);
                    }

                    tentatives++;
                } while (!responseMessage.IsSuccessStatusCode && tentatives < 3);
            }

            return result;
        }

        /// <summary>
        /// Batch download recordings.
        /// Link: https://developer.mypurecloud.com/api/rest/v2/recording/#post-api-v2-recording-batchrequests
        /// </summary>
        /// <param name="conversationId">String, conversation id</param>
        /// <returns>BatchDownloadJobSubmissionResult, Job result object</returns>
        public async Task<BatchDownloadJobSubmissionResult> BatchRecordingDownloadByConversation(List<string> conversations)
        {
            BatchDownloadJobSubmissionResult result = new BatchDownloadJobSubmissionResult();

            using (HttpClient hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_token}");
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                HttpResponseMessage responseMessage = new HttpResponseMessage();
                BatchDownloadJobSubmission queryParam = new BatchDownloadJobSubmission
                {
                    BatchDownloadRequestList = conversations
                        .Select(c => new BatchDownloadRequest() { ConversationId = c }).ToList()
                };

                int tentatives = 0;
                do
                {
                    responseMessage = await hc.PostAsync(_uribase + "/api/v2/recording/batchrequests",
                    new StringContent(queryParam.ToJson(), Encoding.UTF8, "application/json"));

                    string jsonMessage = await responseMessage.Content.ReadAsStringAsync();
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        result = JsonConvert.DeserializeObject<BatchDownloadJobSubmissionResult>(jsonMessage);
                    }
                    else if ((int)responseMessage.StatusCode >= 300 && (int)responseMessage.StatusCode < 600)
                    {
                        throw new Exception(jsonMessage);
                    }

                    tentatives++;
                } while (!responseMessage.IsSuccessStatusCode && tentatives < 3);
            }

            return result;
        }

        /// <summary>
        /// Check job batch recordings download results.
        /// Link: https://developer.mypurecloud.com/api/rest/v2/recording/#get-api-v2-recording-batchrequests--jobId-
        /// </summary>
        /// <param name="conversationId">String, conversation id</param>
        /// <returns>BatchDownloadJobStatusResult, object of bach request</returns>
        public async Task<BatchDownloadJobStatusResult> GetJobRecordingDownloadResultByConversation(string jobId)
        {
            BatchDownloadJobStatusResult result = new BatchDownloadJobStatusResult();

            using (HttpClient hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_token}");
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                HttpResponseMessage responseMessage = new HttpResponseMessage();
                
                Task.Delay(3000).Wait();
                responseMessage = await hc.GetAsync(_uribase + $"/api/v2/recording/batchrequests/{jobId}");

                string jsonMessage = await responseMessage.Content.ReadAsStringAsync();
                if (responseMessage.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<BatchDownloadJobStatusResult>(jsonMessage);
                }
                else if ((int)responseMessage.StatusCode >= 300 && (int)responseMessage.StatusCode < 600)
                {
                    throw new Exception(jsonMessage);
                }
            }

            return result;
        }

        /// <summary>
        /// Get availabe users from PureCloud
        /// </summary>
        /// <returns>List<Users>, list of object user</returns>
        public async Task<List<User>> GetAvailableUsers()
        {
            List<User> result = new List<User>();
           
            using (HttpClient hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_token}");
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                UserEntityListing response = new UserEntityListing();
                int pageNumber = 1;

                do // passing trough pagination, due to 500 limited users from response
                {
                    HttpResponseMessage responseMessage = await hc.GetAsync(_uribase +
                        $"/api/v2/users?pageSize={_pageUserSize}&pageNumber={pageNumber}");

                    string jsonMessage = await responseMessage.Content.ReadAsStringAsync();
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        response = JsonConvert.DeserializeObject<UserEntityListing>(jsonMessage);

                        if (!response.Entities.Count.Equals(0))
                        {
                            result.AddRange(response.Entities);
                        }

                        pageNumber++;
                    }
                    else if ((int)responseMessage.StatusCode >= 300 && (int)responseMessage.StatusCode < 600)
                    {
                        throw new Exception(jsonMessage);
                    }

                } while (!response.Entities.Count.Equals(0));
            }

            return result;
        }
    }
}