using BOI.Core.Search.Models;
using Nest;

namespace BankOfIreland.Intermediaries.Feature.Search.Queries.Elastic
{
    public interface ISearchResultSearcher
    {
        QueryContainer BuildQueryContainer(IMainSearchQuery model);
        MainSearchResults Execute(IMainSearchQuery model);
    }
}