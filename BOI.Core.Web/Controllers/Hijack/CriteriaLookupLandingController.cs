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

            var results = criteriaLookupSearcher.Execute(model);
            var buyToLetResults = criteriaLookupSearcher.BuyToLetExecute(model);
            var bespokeResults = criteriaLookupSearcher.BespokeExecute(model);

            //Hacky fix for validation error state - no marker for required fields so not sure why this is doing what it is doing
            //ModelState.Clear();
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

        public ActionResult FindResidentialCriteria(string criteriaName, string criteriaCategory, int pageId, string searchCriteriaOnly)
        {
            var currentPage = umbracoHelper.Content(pageId);

            CriteriaLookupsResults results = new CriteriaLookupsResults();

            if (searchCriteriaOnly == null || !bool.Parse(searchCriteriaOnly))
            {
                results = SearchCriteriaWithCriteriaTab(criteriaName, criteriaCategory, FieldConstants.ResidentialProductType, currentPage);
            }
            else
            {
                results = SearchCriteriaOnly(criteriaName, FieldConstants.ResidentialProductType, currentPage);
            }

            return PartialView("~/Views/Partials/CriteriaLookupLanding/ResidentialProducts.cshtml", new CriteriaLookupResultsViewModel(currentPage, publishedValueFallback)
            {
                ListingUrl = currentPage.Url(),
                Results = results,
            });
        }

        public ActionResult FindBuyToLetCriteria(string criteriaName, string criteriaCategory, int pageId, string searchCriteriaOnly)
        {
            var currentPage = umbracoHelper.Content(pageId);

            CriteriaLookupsResults results = new CriteriaLookupsResults();

            if (searchCriteriaOnly == null || !bool.Parse(searchCriteriaOnly))
            {
                results = SearchCriteriaWithCriteriaTab(criteriaName, criteriaCategory, FieldConstants.BuyToLetProductType, currentPage);
            }
            else
            {
                results = SearchCriteriaOnly(criteriaName, FieldConstants.BuyToLetProductType, currentPage);
            }

            return PartialView("~/Views/Partials/CriteriaLookupLanding/BuyToLetProducts.cshtml", new CriteriaLookupResultsViewModel(currentPage, publishedValueFallback)
            {
                ListingUrl = currentPage.Url(),
                BuyToLetResults = results
            });
        }

        public ActionResult FindBespokeCriteria(string criteriaName, string criteriaCategory, int pageId, string searchCriteriaOnly)
        {
            var currentPage = umbracoHelper.Content(pageId);
            CriteriaLookupsResults results = new CriteriaLookupsResults();

            if (searchCriteriaOnly == null || !bool.Parse(searchCriteriaOnly))
            {
                results = SearchCriteriaWithCriteriaTab(criteriaName, criteriaCategory, FieldConstants.BespokeProductType, currentPage);
            }
            else
            {
                results = SearchCriteriaOnly(criteriaName, FieldConstants.BespokeProductType, currentPage);
            }

            return PartialView("~/Views/Partials/CriteriaLookupLanding/BespokeProducts.cshtml", new CriteriaLookupResultsViewModel(currentPage, publishedValueFallback)
            {
                ListingUrl = currentPage.Url(),
                BespokeResults = results
            });
        }

        private CriteriaLookupsResults SearchCriteriaWithCriteriaTab(string criteriaName, string criteriaCategory, string productType, IPublishedContent currentPage)
        {
            var wordsToIgnore = currentPage.Value<string>("wordsToIgnore")?.Split(' ');
            if (wordsToIgnore != null)
                criteriaName = string.Join(" ", criteriaName.Split(' ').Except(wordsToIgnore));

            var criteriaLookupSearcher = new CriteriaLookupSearcher(config, esClient,shortStringHelper);

            if (productType == FieldConstants.ResidentialProductType)
                return criteriaLookupSearcher.Execute(new CriteriaLookupSearch() { CriteriaCategory = criteriaCategory, CriteriaName = criteriaName });
            else if (productType == FieldConstants.BuyToLetProductType)
                return criteriaLookupSearcher.BuyToLetExecute(new CriteriaLookupSearch() { BuyToLetCriteriaCategory = criteriaCategory, BuyToLetCriteriaName = criteriaName });
            else
                return criteriaLookupSearcher.BespokeExecute(new CriteriaLookupSearch() { BespokeCriteriaCategory = criteriaCategory, BespokeCriteriaName = criteriaName });
        }

        private CriteriaLookupsResults SearchCriteriaOnly(string criteriaName, string productType, IPublishedContent currentPage)
        {
            var query = new AutocompleteQuery(esClient, config)
            {
                QueryString = criteriaName,
                CriteriaType = productType
            };
            var response = query.SearchCriteria();

            var criteriaLookupSearcher = new CriteriaLookupSearcher(config, esClient, shortStringHelper);
            var criteria = response.Documents.FirstOrDefault();
            var result = new CriteriaLookupResult()
            {
                Id = criteria.Id,
                NameId = criteria.Name.ToCleanString(shortStringHelper, CleanStringType.UrlSegment),
                CriteriaName = criteria.CriteriaName,
                SortOrder = criteria.SortOrder,
                CriteriaCategory = criteria.CriteriaCategory,
                BodyText = criteria.BodyText,
                IsBuyToLetProduct = criteria.BuyToLetProduct,
                IsResidentialProduct = criteria.ResidentialProduct,
                IsBespokeProduct = criteria.BespokeProduct,
                CriteriaTabs = criteriaLookupSearcher.SearchCriteriaTabs(criteria.Id),
                CriteriaUpdatedDate = criteria.CriteriaUpdateDate
            };

            return new CriteriaLookupsResults
            {
                QueryResults = result.AsEnumerableOfOne().ToList()
            };
        }

        public JsonResult AutoCompleteCriteriaLookup(string queryString, string criteriaType)
        {
            var query = new AutocompleteQuery(esClient, config)
            {
                QueryString = queryString,
                CriteriaType = criteriaType
            };

            var response = query.SearchCriteriaLookup();
            return Json(new AutocompleteSuggestion { suggestions = response });
        }

    }
}
