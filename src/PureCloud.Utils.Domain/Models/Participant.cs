using System.Collections.Generic;

namespace PureCloud.Utils.Domain.Models
{
    public class Participant
    {
        public string participantId { get; set; }
        public string participantName { get; set; }
        public string purpose { get; set; }
        public List<Session> sessions { get; set; }
    }
}
