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

                HttpResponseMessage responseMessage = await hc.PostAsync("https://login.mypurecloud.com/oauth/token", content);

                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    string jsonMessage = await responseMessage.Content.ReadAsStringAsync();
                    access_tokenInfo = JsonConvert.DeserializeObject<AuthTokenInfo>(jsonMessage);
                }
            }

            _token = access_tokenInfo?.AccessToken;
        }

        public async Task<List<Domain.Models.Conversation>> GetConversationsByInterval(DateTime begin, DateTime end)
        {
            ConversationQuery queryParam = new ConversationQuery()
            {
                //Interval = "2019-01-01T00:00:00.000Z/2019-01-31T23:59:59.999Z",
                Interval = $"{begin.ToString("yyyy-MM-dd")}T00:00:00.000Z/{end.ToString("yyyy-MM-dd")}T23:59:59.999Z",
                Order = ConversationQuery.OrderEnum.Asc,
                OrderBy = ConversationQuery.OrderByEnum.Conversationstart
            };

            ConversationResponse response = new ConversationResponse();
            using (HttpClient hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_token}");
                hc.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");


                HttpResponseMessage responseMessage = await hc.PostAsync("https://api.mypurecloud.com/api/v2/analytics/conversations/details/query",
                      new StringContent(queryParam.ToJson(), Encoding.UTF8, "application/json"));

                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    string jsonMessage = await responseMessage.Content.ReadAsStringAsync();
                    response = JsonConvert.DeserializeObject<ConversationResponse>(jsonMessage);
                }
            }

            return response.Conversations;
        }

        public Task<List<string>> GetRecordingsByConversation(string conversationId)
        {
            throw new NotImplementedException();
        }
    }
}