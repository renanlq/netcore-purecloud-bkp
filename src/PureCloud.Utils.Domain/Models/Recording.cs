using System.Collections.Generic;

namespace PureCloud.Utils.Domain.Models
{    
    public class Recording
    {
        public int id {get; set;}
        public string name {get; set;}
        public string selfUri {get; set;}
        public List<BatchDownloadRequest> BatchDownloadRequests {get; set;}
    }

    public class BatchDownloadRequest
    {
        public int conversationId {get; set;}
        public int recordingId {get; set;}
    }
}