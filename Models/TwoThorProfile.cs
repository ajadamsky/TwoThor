using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.Collections.Sequences;
using twothor.Models;
using System.Linq;
using twothor.Data;

namespace twothor.Models
{
    
    public class TwoThorProfile // brukes for å lagre informasjonen om en registrert 2Thor
    {
        public TwoThorProfile() {}

        public TwoThorProfile(string myName, string email, string description)
        {
            Name = myName;
            Email = email;
            Description = description;
            PictureUrl = "null";
        }

        public int Id {get; set;}

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }
        
        [Required]
        public string Description { get; set; }

        public string PictureUrl { get; set; }
    }
}