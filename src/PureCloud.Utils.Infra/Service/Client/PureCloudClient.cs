using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using PureCloud.Utils.Domain.Interfaces.Services;
using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Extensions;
using PureCloudPlatform.Client.V2.Model;

namespace PureCloud.Utils.Infra.Service.Client
{
    
    public class PureCloudClient : IPureCloudService
    {
        private string _token;

        public PureCloudClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback += (s, c, h, e) => true;

            //if (string.IsNullOrWhiteSpace(PureCloudPlatform.Client.V2.Client.Configuration.Default.Access_token))
            //{
            //    //AuthTokenInfo access_tokenInfo = Configuration.Default.ApiClient.Post_token("b208fcc5-d529-4f65-8f3a-fc8bfc48c8bf",
            //    //"wdZmL0e0WBD3Se13W6w3tL25YTvaUDOY-q2CEriN1pA");
            //    //PureCloudPlatform.Client.V2.Client.Configuration.Default.Access_token = access_tokenInfo?.Access_token;
            //    PureCloudPlatform.Client.V2.Client.Configuration.Default.Access_token = _token;
            //}
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

                HttpResponseMessage responseMessage = await hc.PostAsync("https://login.mypurecloud.com/oauth/_token", content);

                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    string jsonMessage = await responseMessage.Content.ReadAsStringAsync();
                    access_tokenInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthTokenInfo>(jsonMessage);
                }
            }

            _token = access_tokenInfo?.AccessToken;
        }

        public async Task<List<string>> GetAllConversationQuery(int quantidade, int page, string value, DateTime data)
        {
            List<string> conversations = new List<string>();

            AnalyticsApi api = new AnalyticsApi();

            List<AnalyticsQueryPredicate> queryPredicate = new List<AnalyticsQueryPredicate>();
            queryPredicate.Add(new AnalyticsQueryPredicate()
            {
                Type = AnalyticsQueryPredicate.TypeEnum.Dimension,
                Dimension = AnalyticsQueryPredicate.DimensionEnum.Mediatype,
                _Operator = AnalyticsQueryPredicate.OperatorEnum.Matches,
                Value = value
            });

            List<AnalyticsQueryFilter> queryFilter = new List<AnalyticsQueryFilter>();
            queryFilter.Add(new AnalyticsQueryFilter()
            {
                Type = AnalyticsQueryFilter.TypeEnum.Or,
                Predicates = queryPredicate
            });

            ConversationQuery queryParam = new ConversationQuery()
            {
                //Interval = "2019-01-01T00:00:00.000Z/2019-01-31T23:59:59.999Z",
                Interval = $"{data.ToString("yyyy-MM-dd")}T00:00:00.000Z/{data.ToString("yyyy-MM-dd")}T23:59:59.999Z",
                Order = ConversationQuery.OrderEnum.Asc,
                OrderBy = ConversationQuery.OrderByEnum.Conversationstart,
                Paging = new PagingSpec(quantidade, page),
                SegmentFilters = queryFilter
            };

            AnalyticsConversationQueryResponse response = null;
            using (HttpClient hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_token}");
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");


                HttpResponseMessage responseMessage = await hc.PostAsync("https://api.mypurecloud.com/api/v2/analytics/conversations/details/query",
                      new StringContent(queryParam.ToJson(), Encoding.UTF8, "application/json"));

                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    string jsonMessage = await responseMessage.Content.ReadAsStringAsync();
                    response = Newtonsoft.Json.JsonConvert.DeserializeObject<AnalyticsConversationQueryResponse>(jsonMessage);
                }
            }

            if (response != null)
            {
                conversations.AddRange(response.Conversations?.Select(c => c.ConversationId));

                if (response.Conversations.Count == quantidade)
                {
                    conversations.AddRange(await this.GetAllConversationQuery(quantidade, page + 1, value, data));
                }
            }

            return conversations;
        }

        public async Task<(string, List<string>)> GetConversationEmailsMessages(string conversationId, bool orderByAsc)
        {
            List<string> emailMessages = new List<string>();
            ConversationsApi api = new ConversationsApi();
            EmailMessageListing response = null;

            using (HttpClient hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_token}");
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                HttpResponseMessage responseMessage = await hc.GetAsync($"https://api.mypurecloud.com/api/v2/conversations/emails/{conversationId}/messages");

                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    string jsonMessage = await responseMessage.Content.ReadAsStringAsync();
                    response = Newtonsoft.Json.JsonConvert.DeserializeObject<EmailMessageListing>(jsonMessage);
                }
            }

            if (orderByAsc)
            {
                emailMessages.AddRange(response.Entities?.OrderBy(e => e.Time).Select(e => e.Id));
            }
            else
            {
                emailMessages.AddRange(response.Entities?.OrderByDescending(e => e.Time).Select(e => e.Id));
            }

            return (conversationId, emailMessages);
        }

        public async Task<(string, string)> GetConversationEmailMessageDetail(string conversationId, string messageId)
        {
            ConversationsApi api = new ConversationsApi();

            EmailMessage emailMessage = null;
            using (HttpClient hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_token}");
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                HttpResponseMessage responseMessage = await hc.GetAsync($"https://api.mypurecloud.com/api/v2/conversations/emails/{conversationId}/messages/{messageId}");

                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    string jsonMessage = await responseMessage.Content.ReadAsStringAsync();
                    emailMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<EmailMessage>(jsonMessage);
                }
            }

            return (emailMessage?.Id, emailMessage?.TextBody);
        }
    }
}