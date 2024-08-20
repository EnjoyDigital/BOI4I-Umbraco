using Nest;

namespace BOI.Core.Search.Models
{
    public interface IAutocompleteQuery
    {
        string CriteriaType { get; set; }
        string QueryString { get; set; }

        IEnumerable<AutocompleteSuggestionSubClass> Execute();
        ISearchResponse<WebContent> SearchCriteria();
        IEnumerable<AutocompleteSuggestionSubClass> SearchCriteriaLookup();
    }
}