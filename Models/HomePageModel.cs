using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using twothor.Models;

namespace twothor.Models
{
    public class HomePageModel // denne klassen 
    {
        public HomePageModel(){}

        public int NewMessages { get; set; }

        public string[] SubjectList { get; set; }

        public bool IsTwoThor { get; set; }

        public string ScrollLocation { get; set; }
    }
}