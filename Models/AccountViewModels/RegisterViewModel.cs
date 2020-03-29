using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace twothor.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        public string[] SubjectsKnown { get; set; }
        
        public List<Subject> Subjects { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set;}
        

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set;}
    }
}
