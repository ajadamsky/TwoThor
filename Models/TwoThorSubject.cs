using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.Collections.Sequences;
using twothor.Models;
using System.Linq;

namespace twothor.Models
{
    
    public class TwoThorSubjects // brukes for å lagre et fag en 2THor er toutor i
    {
        public TwoThorSubjects() {}

        public TwoThorSubjects(string twoThorName, string subjectName)
        {
            TwoThorName = twoThorName;
            SubjectName = subjectName;
        }

        public int Id {get; set;}

        [Required]
        public string TwoThorName { get; set; }

        [Required]
        public string SubjectName { get; set; }
    }
}