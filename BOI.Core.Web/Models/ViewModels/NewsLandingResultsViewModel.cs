using BOI.Core.Search.Models;
using BOI.Umbraco.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace BOI.Core.Web.Models.ViewModels
{
    public class NewsLandingResultsViewModel : NewsLanding
    {
        public NewsLandingResultsViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback) : base(content, publishedValueFallback) { }

        public NewsArticlesResults Results;

        public string ListingUrl { get; set; }

        public Page<NewsArticleResult> Paging { get; set; }
    }
}
