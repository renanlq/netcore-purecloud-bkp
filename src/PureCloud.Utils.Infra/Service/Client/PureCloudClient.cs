using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PureCloud.Utils.Domain.Interfaces.Services;
using PureCloud.Utils.Infra.Service.Storage;
using PureCloudPlatform.Client.V2.Extensions;
using PureCloudPlatform.Client.V2.Model;

namespace PureCloud.Utils.Infra.Service.Client
{

    public class PureCloudClient : IPureCloudService
    {
        private const int _pageSize = 100;
        private const int _pageUserSize = 500;
        private readonly string _uribase = "https://api.mypurecloud.com";
        private string _token;
        private int _pageNumber = 1;

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
                else {
                    await BlobStorageService.AddToErrorAsync(
                        JsonConvert.SerializeObject(responseMessage), "getaccesstoken", $"{DateTime.Now}.json");
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
        public async Task<List<AnalyticsConversation>> GetConversationsByInterval(DateTime begin, DateTime end)
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
                        PageNumber = _pageNumber
                    }
                };

                do // passing trough pagination, due to 100 limited conversations in query response
                {
                    HttpResponseMessage responseMessage = await hc.PostAsync(_uribase + "/api/v2/analytics/conversations/details/query",
                      new StringContent(queryParam.ToJson(), Encoding.UTF8, "application/json"));

                    string jsonMessage = await responseMessage.Content.ReadAsStringAsync();
                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        response = JsonConvert.DeserializeObject<AnalyticsConversationQueryResponse>(jsonMessage);

                        if (response.Conversations != null)
                        {
                            result.AddRange(response.Conversations);
                        }

                        queryParam.Paging.PageNumber++;
                    }
                    else if ((int)responseMessage.StatusCode >= 400 && (int)responseMessage.StatusCode < 600)
                    {
                        await BlobStorageService.AddToErrorAsync(
                                JsonConvert.SerializeObject(responseMessage), "getconversationsnyinterval", $"{begin.Date}.json");
                        return new List<AnalyticsConversation>();
                    }

                } while (response.Conversations != null);
            }

            return result;
        }

        /// <summary>
        /// Batch download recordings.
        /// Link: https://developer.mypurecloud.com/api/rest/v2/recording/#post-api-v2-recording-batchrequests
        /// </summary>
        /// <param name="conversationId">String, conversation id</param>
        /// <returns>BatchDownloadJobSubmissionResult, Job result object</returns>
        public async Task<BatchDownloadJobSubmissionResult> BatchRecordingDownloadByConversation(string conversationId)
        {
            int count = 0;
            BatchDownloadJobSubmissionResult result = new BatchDownloadJobSubmissionResult();

            using (HttpClient hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_token}");
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                HttpResponseMessage responseMessage = new HttpResponseMessage();
                BatchDownloadJobSubmission queryParam = new BatchDownloadJobSubmission
                {
                    BatchDownloadRequestList = new List<BatchDownloadRequest>() {
                        new BatchDownloadRequest() { ConversationId = conversationId }
                    }
                };

                do
                {
                    responseMessage = await hc.PostAsync(_uribase + "/api/v2/recording/batchrequests",
                    new StringContent(queryParam.ToJson(), Encoding.UTF8, "application/json"));

                    string jsonMessage = await responseMessage.Content.ReadAsStringAsync();
                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        result = JsonConvert.DeserializeObject<BatchDownloadJobSubmissionResult>(jsonMessage);
                        if (count.Equals(3)) return result;
                    }
                    else if ((int)responseMessage.StatusCode >= 400 && (int)responseMessage.StatusCode < 600)
                    {
                        await BlobStorageService.AddToErrorAsync(
                                JsonConvert.SerializeObject(responseMessage), "batchrecordingdownloadbyconversation", $"{conversationId}.json");
                        return result;
                    }

                    count++;
                } while (responseMessage.StatusCode == HttpStatusCode.Accepted && responseMessage.StatusCode != HttpStatusCode.OK);
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
                do
                {
                    Task.Delay(1000).Wait();
                    responseMessage = await hc.GetAsync(_uribase + $"/api/v2/recording/batchrequests/{jobId}");

                    string jsonMessage = await responseMessage.Content.ReadAsStringAsync();
                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        result = JsonConvert.DeserializeObject<BatchDownloadJobStatusResult>(jsonMessage);
                    }
                    else if ((int)responseMessage.StatusCode >= 400 && (int)responseMessage.StatusCode < 600)
                    {
                        await BlobStorageService.AddToErrorAsync(
                            JsonConvert.SerializeObject(responseMessage), "getjobrecordingdownloadresultbyconversation", $"{jobId}.json");
                        return result;
                    }

                } while (!result.ExpectedResultCount.Equals(result.ResultCount));
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
                _pageNumber = 1;

                do // passing trough pagination, due to 500 limited users from response
                {
                    HttpResponseMessage responseMessage = await hc.GetAsync(_uribase +
                        $"/api/v2/users?pageSize={_pageUserSize}&pageNumber={_pageNumber}");

                    string jsonMessage = await responseMessage.Content.ReadAsStringAsync();
                    if (responseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        response = JsonConvert.DeserializeObject<UserEntityListing>(jsonMessage);

                        if (!response.Entities.Count.Equals(0))
                        {
                            result.AddRange(response.Entities);
                        }

                        _pageNumber++;
                    }

                    else if ((int)responseMessage.StatusCode >= 400 && (int)responseMessage.StatusCode < 600)
                    {
                        await BlobStorageService.AddToErrorAsync(
                            JsonConvert.SerializeObject(responseMessage), "getavailableusers", $"{DateTime.Now}.json");
                        return result;
                    }

                } while (!response.Entities.Count.Equals(0));
            }

            return result;
        }
    }
}