using BOI.Core.Search.Models;
using BOI.Umbraco.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace BOI.Core.Web.Models.ViewModels
{
    public class CriteriaLookupResultsViewModel : CriteriaLookupLanding
    {
        public CriteriaLookupResultsViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback) : base(content, publishedValueFallback) { }

        public CriteriaLookupsResults Results;
        public CriteriaLookupsResults BuyToLetResults;
        public CriteriaLookupsResults BespokeResults;
        public string ListingUrl { get; set; }
        public IEnumerable<SelectListItem> CategoryList { get; set; }
        public IEnumerable<SelectListItem> BuyToLetCategoryList { get; set; }
        public IEnumerable<SelectListItem> BespokeCategoryList { get; set; }
        public string CriteriaCategory { get; set; }
        public string CriteriaName { get; set; }
        public string BuyToLetCriteriaCategory { get; set; }
        public string BuyToLetCriteriaName { get; set; }
        public string BespokeCriteriaCategory { get; set; }
        public string BespokeCriteriaName { get; set; }
        public bool isBuyToLet { get; set; }
        public bool isBespoke { get; set; }
    }
}
