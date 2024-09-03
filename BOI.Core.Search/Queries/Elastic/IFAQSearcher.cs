using BOI.Core.Search.Models;
using Nest;

namespace BOI.Core.Search.Queries.Elastic
{
    public interface IFAQSearcher
    {
        QueryContainer BuildQueryContainer(string faqName);
        IAggregationContainer BuildAggregationContainer(string faqFilterName);
        QueryContainer BuildPostFilterContainer(string faqCategoryFilter, string faqCategory);
        AggregateDictionary FAQFormValues();
        FAQResults ExecuteFAQ(FAQSearch model);
        void ParseHighLights(ISearchResponse<WebContent> response, string key);
        IEnumerable<FAQTabResult> SearchFAQTabs(int parentNodeId);
    }
}