using System.Collections.Generic;
using twothor.Models;
using twothor.Models.AccountViewModels;

namespace twothor.Models
{
    public class TwoThorOverviewViewModel // overview model som brukes for å vise en profil til hvem som helst, men gir mulighet for endringer for den som eier siden
    {
        public bool ownProfile { get; set; }
        public RegisterViewModel RegisterModel { get; set; }

        public TwoThorProfile Profile { get; set; }

        public List<TwoThorJob> Jobs { get; set; }

        public List<TwoThorReview> Reviews { get; set; }

        public List<TwoThorSubjects> Subjects { get; set; }

        public ChatMessage Message { get; set; }

    }
}