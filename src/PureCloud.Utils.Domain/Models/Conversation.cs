using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace PureCloud.Utils.Domain.Models
{    
    public class Conversation : TableEntity
    {
        public int conversationId {get; set;}
        public DateTime conversationStart {get; set;}
        public DateTime conversationEnd {get; set;}

        public List<Participant> participants { get; set; }
        public List<string> divisionIds { get; set; }

        public string inteval {
            get {
                //"2016-06-01T00:00:00.000Z\/2016-07-01T00:00:00.000Z";
                return conversationStart.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'") + "\\/" + 
                    conversationEnd.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
            }
        }    
        public string order {
            get {
                return "asc";
            }
        }
        public string orderBy {get; set;}
    }
}