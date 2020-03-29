using System.Collections.Generic;
using twothor.Models;
using static System.Net.Mime.MediaTypeNames;

namespace twothor.Models
{
    public class ChatOverviewViewModel // overview model som brukes for å kombinere de to chat-partialviews for å kunne både vise chat og gi funktionallitet for å sende meldinger.
    {
        public ChatMessage message { get; set; }
        public List<ChatMessage> messages { get; set; }
        public TwoThorJob newJob { get; set; }
        public TwoThorReview newReview { get; set;}

        public bool IsTwoThor { get; set; }

        public string YourPPUrl { get; set; }

        public string OtherPPUrl { get; set; }
    }
}