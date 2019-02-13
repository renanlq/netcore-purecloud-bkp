using System;
using System.Collections.Generic;

namespace PureCloud.Utils.Domain.Models
{    
    public class Conversation
    {
        public int conversationId {get; set;}
        public DateTime conversationStart {get; set;}
        public DateTime conversationEnd {get; set;}
        public string inteval {
            get {
                //"2016-06-01T00:00:00.000Z\/2016-07-01T00:00:00.000Z";
                return conversationStart.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'") + "\\/" + 
                    conversationEnd.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
            }
        }    
        public List<SegmentFilter> SegmentFilters {get; set;}
        public string order {
            get {
                return "asc";
            }
        }
        public string orderBy {get; set;}
    }

    public class SegmentFilter
    {
        public string type {get; set;}
        public List<Predicate> predicates {get; set;}
    }

    public class Predicate
    {
        public string dimension {get; set;}
        public string value {get; set;}
    }
}

/*
{
  "interval": "2016-06-01T00:00:00.000Z\/2016-07-01T00:00:00.000Z",
  "segmentFilters": [
    {
      "type": "or",
      "predicates": [
        {
          "dimension": "purpose",
          "value": "Customer"
        }
      ]
    }
  ],
  "order": "asc",
  "orderBy": "conversationStart"
}
*/