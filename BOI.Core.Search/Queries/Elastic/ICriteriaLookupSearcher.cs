using BOI.Core.Search.Models;
using Nest;

namespace BOI.Core.Search.Queries.Elastic
{
    public interface ICriteriaLookupSearcher
    {
        QueryContainer BuildQueryContainer(string criteriaName, string productType);
        IAggregationContainer BuildAggregationContainer(string criteriaFilterName);
        QueryContainer BuildPostFilterContainer(string criteriaCategoryFilter, string criteriaCategory);
        AggregateDictionary CriteriaLookupFormValues(string criteriaType);
        CriteriaLookupsResults ExecuteCriteriaLookup(CriteriaLookupSearch model, string criteriaType);
        void ParseHighLights(ISearchResponse<WebContent> response, string key);
        IEnumerable<CriteriaTabResult> SearchCriteriaTabs(int parentNodeId);
    }
}