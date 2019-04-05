using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PureCloud.Utils.Domain.Interfaces.Services
{
    public interface IPureCloudService
    {
        Task GetAccessToken();
        Task<List<string>> GetAllConversationQuery(int quantidade, int page, string value, DateTime data);
        Task<(string, List<string>)> GetConversationEmailsMessages(string conversationId, bool orderByAsc);
        Task<(string, string)> GetConversationEmailMessageDetail(string conversationId, string messageId);
    }
}
