using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.Collections.Sequences;
using twothor.Models;
using System.Linq;
using System;

namespace twothor.Models
{
    
    public class TwoThorJob // brukes for å lagre en instance av en jobbavtale mellom en 2Thor og en elev
    {
        [Required]
        public string Completed { get; set; }

        [Required]
        public string Reviewed { get; set; }

        public TwoThorJob() {}

        public TwoThorJob(string twoThorEmail, string otherName, string subject, DateTime time, DateTime date, string finished, string alreadyReviewed)
        {
            TwoThorEmail = twoThorEmail;
            OtherName = otherName;
            Time = time;
            Date = date;
            Completed = finished;
            Reviewed = alreadyReviewed;
            JobSubject = subject;
            SubjectsKnown = new List<Subject>();
        }

        public TwoThorJob(string twoThorEmail, string otherName)
        {
            TwoThorEmail = twoThorEmail;
            OtherName = otherName;
            Completed = "false";
            Reviewed = "false";
            SubjectsKnown = new List<Subject>();
        }

        public void setTime(DateTime time)
        {
            Time = time;
        }

        public int Id {get; set;}

        [Required]
        public string TwoThorEmail { get; set; }

        [Required]
        public string OtherName { get; set; }
        
        [Required]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd MMMM yyyy}")]
        public DateTime Date { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}")]
        public DateTime Time { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public string JobSubject { get; set; }

        public List<Subject> SubjectsKnown { get; set; }
    }
}