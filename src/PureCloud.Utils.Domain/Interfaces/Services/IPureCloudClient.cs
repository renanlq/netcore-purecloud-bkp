using PureCloudPlatform.Client.V2.Model;
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
        /// <returns>List<AnalyticsConversation>, list of purecloud conversations</returns>
        Task<List<AnalyticsConversation>> GetConversationsByInterval(DateTime begin, DateTime end, int page);

        /// <summary>
        /// Batch download recordings.
        /// Link: https://developer.mypurecloud.com/api/rest/v2/recording/#post-api-v2-recording-batchrequests
        /// </summary>
        /// <param name="conversationId">String, conversation id</param>
        /// <returns>BatchDownloadJobSubmissionResult, Job result object</returns>
        Task<BatchDownloadJobSubmissionResult> BatchRecordingDownloadByConversation(List<string> conversations);

        /// <summary>
        /// Check job batch recordings download results.
        /// Link: https://developer.mypurecloud.com/api/rest/v2/recording/#get-api-v2-recording-batchrequests--jobId-
        /// </summary>
        /// <param name="conversationId">String, conversation id</param>
        /// <returns>BatchDownloadJobStatusResult, object of bach request</returns>
        Task<BatchDownloadJobStatusResult> GetJobRecordingDownloadResultByConversation(string jobId);

        /// <summary>
        /// Get availabe users from PureCloud
        /// </summary>
        /// <returns>List<Users>, list of object user</returns>
        Task<List<User>> GetAvailableUsers();

    }
}
