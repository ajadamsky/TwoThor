using System.Collections.Generic;
using twothor.Models;

namespace twothor.Models
{
    public class SearchResultModel // model som sendes til search result page for å gi mulighet til å vise 2Thors i det relevante faget.
    {
        public string SubjectName { get; set; }
        public List<TwoThorProfile> Profiles { get; set; }
        public List<TwoThorReview> Reviews { get; set; }
    }
}