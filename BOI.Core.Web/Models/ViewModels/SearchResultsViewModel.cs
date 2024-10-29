using BOI.Core.Search.Models;
using BOI.Umbraco.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace BOI.Core.Web.Models.ViewModels
{
    public class SearchResultsViewModel : SearchResult
    {
        public SearchResultsViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback) : base(content, publishedValueFallback)
        {
        }

        public SearchResult Content { get; set; }
        public MainSearchResults Results { get; set; }
        public Page<IPagedResult> Paging { get; set; }
        public string SearchTerm { get; set; }
    }
}
