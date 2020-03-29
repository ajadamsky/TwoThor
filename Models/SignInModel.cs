using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using twothor.Models;

namespace twothor.Models
{
    public class SignInModel // denne klassen tar seg av å vise frem et overblikk over de aktive chattene en bruker har. (se ChatController for bruk)
    {
        public SignInModel(){}

        public int Id {get; set;}
        
        [Required]
        public string username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string password { get; set; }

        
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}