using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using twothor.Models;

namespace twothor.Models
{
    public class ChatDisplay // denne klassen tar seg av å vise frem et overblikk over de aktive chattene en bruker har. (se ChatController for bruk)
    {
        public ChatDisplay(string chatname, string previousname, string chatContent, string time, bool r)
        {
            Chatname = chatname;
            Content = chatContent;
            PreviousName = previousname;
            Time = time;
            Read = r;
        }

        public int Id {get; set;}
        
        [Required]
        public string PreviousName { get; set; }

        [Required]
        public string Chatname { get; set; }
        
        
        [Required]
        public string Content { get; set; }
        
        [Required]
        public string Time { get; set; }

        [Required]
        public bool Read { get; set; }
    }
}