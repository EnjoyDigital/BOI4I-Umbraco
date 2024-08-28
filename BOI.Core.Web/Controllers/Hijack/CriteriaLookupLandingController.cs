using BOI.Core.Search.Queries.Elastic;
using BOI.Core.Web.Controllers.Hijack;
using BOI.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;
using BOI.Core.Web.Models.ViewModels;
using BOI.Core.Search.Models;
using BOI.Core.Search.Constants;

namespace BOI.Core.Web.Controllers.Hijacks
{
    public class CriteriaLookupLandingController : RenderController
    {
        private readonly IConfiguration config;
        private readonly IPublishedValueFallback publishedValueFallback;
        private readonly IElasticClient esClient;
        private readonly UmbracoHelper umbracoHelper;
        private readonly IShortStringHelper shortStringHelper;

        public CriteriaLookupLandingController(IConfiguration config, IPublishedValueFallback publishedValueFallback,
            IElasticClient esClient, ILogger<NewsLandingController> logger, ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor, UmbracoHelper umbracoHelper, IShortStringHelper shortStringHelper) : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            this.config = config;
            this.publishedValueFallback = publishedValueFallback;
            this.esClient = esClient;
            this.umbracoHelper = umbracoHelper;
            this.shortStringHelper = shortStringHelper;
        }

        [NonAction]
        public sealed override IActionResult Index() => throw new NotImplementedException();

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var model = new CriteriaLookupSearch();
            await TryUpdateModelAsync(model);
           
            var criteriaLookupSearcher = new CriteriaLookupSearcher(config, esClient,shortStringHelper);
            var search = criteriaLookupSearcher.CriteriaLookupFormValues(FieldConstants.ResidentialProductType);
            var buyToLetSearch = criteriaLookupSearcher.CriteriaLookupFormValues(FieldConstants.BuyToLetProductType);
            var bespokeSearch = criteriaLookupSearcher.CriteriaLookupFormValues(FieldConstants.BespokeProductType);

            var categoryList = new List<SelectListItem>();
            categoryList.AddRange(search.Terms("CriteriaCategory").Buckets.Where(x => x.Key.HasValue()).Select(c => new SelectListItem
            {
                Text = c.Key,
                Value = c.Key,
                Selected = model.CriteriaCategory == c.Key
            }));
            categoryList.Sort((x, y) => string.Compare(x.Text, y.Text));
            categoryList = categoryList.Prepend(new SelectListItem { Text = "All Categories", Value = "null" }).ToList();

            var buyToLetCategoryList = new List<SelectListItem>();
            buyToLetCategoryList.AddRange(buyToLetSearch.Terms("BuyToLetCriteriaCategory").Buckets.Where(x => x.Key.HasValue()).Select(c => new SelectListItem
            {
                Text = c.Key,
                Value = c.Key,
                Selected = model.BuyToLetCriteriaCategory == c.Key
            }));
            buyToLetCategoryList.Sort((x, y) => string.Compare(x.Text, y.Text));
            buyToLetCategoryList = buyToLetCategoryList.Prepend(new SelectListItem { Text = "All Categories", Value = "null" }).ToList();

            var bespokeCategoryList = new List<SelectListItem>();
            bespokeCategoryList.AddRange(bespokeSearch.Terms("BespokeCriteriaCategory").Buckets.Where(x => x.Key.HasValue()).Select(c => new SelectListItem
            {
                Text = c.Key,
                Value = c.Key,
                Selected = model.BespokeCriteriaCategory == c.Key
            }));
            bespokeCategoryList.Sort((x, y) => string.Compare(x.Text, y.Text));
            bespokeCategoryList = bespokeCategoryList.Prepend(new SelectListItem { Text = "All Categories", Value = "null" }).ToList();



            var results = criteriaLookupSearcher.ExecuteCriteriaLookup(model, FieldConstants.ResidentialProductType);
            var buyToLetResults = criteriaLookupSearcher.ExecuteCriteriaLookup(model, FieldConstants.BuyToLetProductType);
            var bespokeResults = criteriaLookupSearcher.ExecuteCriteriaLookup(model, FieldConstants.BespokeProductType);

            return CurrentTemplate(new CriteriaLookupResultsViewModel(CurrentPage,publishedValueFallback)
            {
                ListingUrl = CurrentPage.Url(),
                CategoryList = categoryList,
                BuyToLetCategoryList = buyToLetCategoryList,
                BespokeCategoryList = bespokeCategoryList,
                Results = results,
                BuyToLetResults = buyToLetResults,
                BespokeResults = bespokeResults,
                isBuyToLet = model.IsBuyToLet,
                isBespoke = model.IsBespoke
            });
        }
    }
}
