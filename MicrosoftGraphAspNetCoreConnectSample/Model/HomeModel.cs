using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuggestionsService.Model
{
    public class HomeModel
    {
        public IEnumerable<Suggestion> Suggestions { get; set; }
    }
}
