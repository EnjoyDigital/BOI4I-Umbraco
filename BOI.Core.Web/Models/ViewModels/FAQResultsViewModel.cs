using BOI.Core.Search.Models;
using BOI.Umbraco.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace BOI.Core.Web.Models.ViewModels
{
    public class FAQResultsViewModel : FAqlanding
    {
        public FAQResultsViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback) : base(content, publishedValueFallback) { }

        public FAQResults Results;
       
        public string ListingUrl { get; set; }
        public IEnumerable<SelectListItem> CategoryList { get; set; }
        public string FAQCategory { get; set; }
        public string FAQName { get; set; }
    }
}
