using PureCloud.Utils.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PureCloud.Utils.Domain.Interfaces.Services
{
    public interface IPureCloudService
    {
        Task GetAccessToken();
        Task<List<Conversation>> GetConversationsByInterval(DateTime begin, DateTime end);
        Task<List<string>> GetRecordingsByConversation(string conversationId);
    }
}
