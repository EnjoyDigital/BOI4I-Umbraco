namespace BOI.Core.Search.Models
{
    public class AutocompleteSuggestion
    {
        public IEnumerable<AutocompleteSuggestionSubClass> suggestions { get; set; }
    }

    public class AutocompleteSuggestionSubClass
    {
        public string value { get; set; }
        public string data { get; set; }
    }
}
