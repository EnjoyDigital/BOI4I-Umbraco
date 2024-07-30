using BOI.Core.Search.Models;
using BOI.Umbraco.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace BOI.Core.Web.Models.ViewModels
{
    public class ProductsLandingResultsViewModel : ProductsLanding
    {
        public ProductsLandingResultsViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback) : base(content, publishedValueFallback) { }

        public ProductsResults Results;
        public string ListingUrl { get; set; }
        public IEnumerable<SelectListItem> ProductTypeList { get; set; }
        public IEnumerable<SelectListItem> ProductTermList { get; set; }
        public IEnumerable<SelectListItem> ProductCategoryList { get; set; }
        public IEnumerable<SelectListItem> ProductLTVList { get; set; }
        public string ProductType { get; set; }
        public string ProductTerm { get; set; }
        public string ProductCategory { get; set; }
        public string ProductLTV { get; set; }
        public bool InterestOnly { get; set; }
    }

}
