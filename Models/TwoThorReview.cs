using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.Collections.Sequences;
using twothor.Models;
using System.Linq;

namespace twothor.Models
{
    
    public class TwoThorReview // brukes for å lagre et review av en 2Thor
    {
        public TwoThorReview() {}

        public TwoThorReview(string twoThorName, string twothorsubject, string otherName)
        {
            TwoThorUsername = twoThorName;
            SubjectName = twothorsubject;
            OtherName = otherName;
            HammerAmount = 0;
            Review = "";
        }

        public TwoThorReview(string twoThorName, string twothorsubject, string otherName, int amount, string review)
        {
            TwoThorUsername = twoThorName;
            SubjectName = twothorsubject;
            OtherName = otherName;
            HammerAmount = amount;
            Review = review;
        }

        public void addReview(int hammerAmount, string review)
        {
            HammerAmount = hammerAmount;
            Review = review;
        }

        public int Id {get; set;}

        [Required]
        public string TwoThorUsername { get; set; }

        [Required]
        public string OtherName { get; set; }

        [Required]
        public string SubjectName { get; set; }

        [Required]
        public int HammerAmount { get; set; }
        
        [Required]
        public string Review { get; set; }
    }
}