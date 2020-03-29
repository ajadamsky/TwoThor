using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using twothor.Models;

namespace twothor.Models
{
    
    public class ChatMessage // klassen som brukes for å lagre meldinger. En instance per melding sendt.
    {
        public ChatMessage() {}

        public ChatMessage(string myName, string otherName, string myMessage)
        {
            Name = myName;
            RecipientName = otherName;
            Time = System.DateTime.Now.ToString();
            Content = myMessage;
        }

        public ChatMessage(string myName, string otherName, string time, string myMessage)
        {
            Name = myName;
            RecipientName = otherName;
            Time = time;
            Content = myMessage;
        }

        public ChatMessage(string myName, string otherName)
        {
            Name = myName;
            RecipientName = otherName;
        }
        
        public void addContent(string myMessage)
        {
            Time = System.DateTime.Now.ToString();
            Content = myMessage;
        }
        public void addTime()
        {
            Time = System.DateTime.Now.ToString();
        }

        public int Id {get; set;}

        [Required]
        public string Name { get; set; }

        [Required]  
        public string RecipientName { get; set; }

        [Required]
        public string Time { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public bool read { get; set; }
    }
}