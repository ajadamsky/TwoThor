using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.Collections.Sequences;
using twothor.Models;
using System.Linq;

namespace twothor.Models
{
    
    public class Subject // brukes for å lagre en liste av fag
    {
        public Subject() {}

        public Subject(string subjectName)
        {
            SubjectName = subjectName;
        }

        public int Id {get; set;}

        [Required]
        public string SubjectName { get; set; }
    }
}