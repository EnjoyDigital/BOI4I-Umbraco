using BOI.Core.Web.Services;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;
using BOI.Core.Search.Constants;
using BOI.Core.Search.Models;
using BOI.Core.Web.Models.ViewModels;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;
using BOI.Core.Search.Queries.Elastic;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using Nest;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.Web;
using BOI.Umbraco.Models;

namespace BOI.Core.Web.Controllers.NonPage
{
    public class CustomRouteActionController : UmbracoPageController, IVirtualPageController
    {
        private readonly IDomainService domainService;
        private readonly UmbracoHelper umbracoHelper;
        private readonly IElasticClient esClient;
        private readonly IConfiguration config;
        private readonly IShortStringHelper shortStringHelper;
        private readonly IPublishedValueFallback publishedValueFallback;
        private readonly IUmbracoContextAccessor umbracoContextAccessor;

        public CustomRouteActionController(IDomainService domainService, UmbracoHelper umbracoHelper, IElasticClient esClient,
            ILogger<CustomRouteActionController> logger, ICompositeViewEngine compositeViewEngine, IConfiguration config,
            IShortStringHelper shortStringHelper, IPublishedValueFallback publishedValueFallback, IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, compositeViewEngine)
        {
            this.domainService = domainService;
            this.umbracoHelper = umbracoHelper;
            this.esClient = esClient;
            this.config = config;
            this.shortStringHelper = shortStringHelper;
            this.publishedValueFallback = publishedValueFallback;
            this.umbracoContextAccessor = umbracoContextAccessor;
        }

        [HttpPost]
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

        [HttpPost]

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

        [HttpPost]

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
            if (wordsToIgnore != null && criteriaName != null)
                criteriaName = string.Join(" ", criteriaName.Split(' ').Except(wordsToIgnore));

            var criteriaLookupSearcher = new CriteriaLookupSearcher(config, esClient, shortStringHelper);

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

        public IPublishedContent? FindContent(ActionExecutingContext actionExecutingContext)
        {
            if (umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
            {
                var casualtyDetail = umbracoContext.Content?.GetAtRoot().DescendantsOrSelf<CriteriaLookupLanding>().FirstOrDefault();

                if (casualtyDetail != null)
                {
                    return casualtyDetail;
                }
            }
            throw new NotImplementedException();
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
