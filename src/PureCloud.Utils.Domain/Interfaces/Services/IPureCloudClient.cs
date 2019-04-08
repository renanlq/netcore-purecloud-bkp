using PureCloud.Utils.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PureCloud.Utils.Domain.Interfaces.Services
{
    public interface IPureCloudService
    {
        Task GetAccessToken();

        /// <summary>
        /// Get all conversations by interval.
        /// Link: https://developer.mypurecloud.com/api/rest/v2/conversations/#post-api-v2-analytics-conversations-details-query
        /// </summary>
        /// <param name="begin">Datetime, begin date of interval</param>
        /// <param name="end">Datetime, end date of interval</param>
        /// <returns>List of purecloud conversations</returns>
        Task<List<Conversation>> GetConversationsByInterval(DateTime begin, DateTime end);

        /// <summary>
        /// Batch download recordings.
        /// Link: https://developer.mypurecloud.com/api/rest/v2/recording/#post-api-v2-recording-batchrequests
        /// </summary>
        /// <param name="conversationId">String, conversation id</param>
        /// <returns>String, Job id</returns>
        Task<string> BatchRecordingDownloadByConversation(string conversationId);

        /// <summary>
        /// Check job batch recordings download results.
        /// Link: https://developer.mypurecloud.com/api/rest/v2/recording/#get-api-v2-recording-batchrequests--jobId-
        /// </summary>
        /// <param name="conversationId">String, conversation id</param>
        /// <returns>Batch response</returns>
        Task<Domain.Models.Batch> GetJobRecordingDownloadResultByConversation(string jobId);
    }
}