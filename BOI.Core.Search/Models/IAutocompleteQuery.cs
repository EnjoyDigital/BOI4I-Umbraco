using Nest;

namespace BOI.Core.Search.Models
{
    public interface IAutocompleteQuery
    {
        string CriteriaType { get; set; }
        string QueryString { get; set; }

        IEnumerable<AutocompleteSuggestionSubClass> Execute();
        ISearchResponse<WebContent> SearchCriteria();
        ISearchResponse<WebContent> SearchFAQ();
        IEnumerable<AutocompleteSuggestionSubClass> SearchCriteriaLookup();
        IEnumerable<AutocompleteSuggestionSubClass> SearchFAQs();
    }
}