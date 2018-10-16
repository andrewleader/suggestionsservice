using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuggestionsService.Model
{
    public class Suggestion : IComparable<Suggestion>
    {
        public TimeInfo Time { get; set; }

        public TimeInfo EndTime { get; set; }

        public TimeInfo BecomesRelevantAt { get; set; }

        public TimeInfo StopsBeingRelevantAt { get; set; }

        public string[] Categories { get; set; }

        public SuggestionAppInfo AppInfo { get; set; }

        public JObject AdaptiveCard { get; set; }

        public int CompareTo(Suggestion other)
        {
            if (Time?.Time != null && other.Time?.Time != null)
            {
                return Time.Time.Value.CompareTo(other.Time.Time.Value);
            }

            return 0;
        }
    }
}
