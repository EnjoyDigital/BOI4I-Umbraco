using BOI.Core.Search.Models;

namespace BankOfIreland.Intermediaries.Feature.Search.Queries.Elastic
{
    public interface IMainSearchQuery
    {
        IEnumerable<SearchFilter> Filters { get; }
        int Page { get; set; }
        string SearchTerm { get; set; }
        int Size { get; set; }
    }
}