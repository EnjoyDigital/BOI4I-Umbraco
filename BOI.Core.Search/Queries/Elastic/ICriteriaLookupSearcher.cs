using BOI.Core.Search.Models;
using Nest;

namespace BOI.Core.Search.Queries.Elastic
{
    public interface ICriteriaLookupSearcher
    {
        CriteriaLookupsResults BespokeExecute(CriteriaLookupSearch model);
        QueryContainer BuildQueryContainer(string criteriaName);
        CriteriaLookupsResults BuyToLetExecute(CriteriaLookupSearch model);
        AggregateDictionary CriteriaLookupFormValues(string criteriaType);
        CriteriaLookupsResults Execute(CriteriaLookupSearch model);
        void ParseHighLights(ISearchResponse<WebContent> response, string key);
        IEnumerable<CriteriaTabResult> SearchCriteriaTabs(int parentNodeId);
    }
}