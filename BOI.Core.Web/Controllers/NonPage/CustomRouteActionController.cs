using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Web.Common.Controllers;
using BOI.Core.Search.Constants;
using BOI.Core.Search.Models;
using BOI.Core.Web.Models.ViewModels;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;
using BOI.Core.Search.Queries.Elastic;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.Web;
using BOI.Umbraco.Models;

namespace BOI.Core.Web.Controllers.NonPage
{
    public class CustomRouteActionController : UmbracoPageController, IVirtualPageController
    {
        private readonly IAutocompleteQuery autocomplete;
        private readonly UmbracoHelper umbracoHelper;
        private readonly ICriteriaLookupSearcher criteriaLookupSearcher;
        private readonly IFAQSearcher faqSearcher;
        private readonly IShortStringHelper shortStringHelper;
        private readonly IPublishedValueFallback publishedValueFallback;
        private readonly IUmbracoContextAccessor umbracoContextAccessor;

        public CustomRouteActionController(IAutocompleteQuery autocomplete, UmbracoHelper umbracoHelper, ICriteriaLookupSearcher criteriaLookupSearcher, IFAQSearcher faqSearcher,
            IShortStringHelper shortStringHelper, ILogger<CustomRouteActionController> logger, ICompositeViewEngine compositeViewEngine, IPublishedValueFallback publishedValueFallback, IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, compositeViewEngine)
        {
            this.autocomplete = autocomplete;
            this.umbracoHelper = umbracoHelper;
            this.criteriaLookupSearcher = criteriaLookupSearcher;
            this.faqSearcher = faqSearcher;
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

            if (productType == FieldConstants.ResidentialProductType)
                return criteriaLookupSearcher.ExecuteCriteriaLookup(new CriteriaLookupSearch() { CriteriaCategory = criteriaCategory, CriteriaName = criteriaName }, FieldConstants.ResidentialProductType);
            else if (productType == FieldConstants.BuyToLetProductType)
                return criteriaLookupSearcher.ExecuteCriteriaLookup(new CriteriaLookupSearch() { BuyToLetCriteriaCategory = criteriaCategory, BuyToLetCriteriaName = criteriaName }, FieldConstants.BuyToLetProductType);
            else
                return criteriaLookupSearcher.ExecuteCriteriaLookup(new CriteriaLookupSearch() { BespokeCriteriaCategory = criteriaCategory, BespokeCriteriaName = criteriaName }, FieldConstants.BespokeProductType);
        }

        private CriteriaLookupsResults SearchCriteriaOnly(string criteriaName, string productType, IPublishedContent currentPage)
        {
            autocomplete.QueryString = criteriaName;
            autocomplete.CriteriaType = productType;

            var response = autocomplete.SearchCriteria();
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

        [HttpPost]
        public ActionResult FindFAQ(string faqQuestion, string faqCategory, int pageId, string searchFaqOnly)
        {
            var currentPage = umbracoHelper.Content(pageId);

            FAQResults results = new FAQResults();

            if (searchFaqOnly == null || !bool.Parse(searchFaqOnly))
            {
                results = SearchFAQWithFAQTab(faqQuestion, faqCategory, currentPage);
            }
            else
            {
                results = SearchFAQOnly(faqQuestion, currentPage);
            }

            return PartialView("~/Views/Partials/FAQLanding/FAQs.cshtml", new FAQResultsViewModel(currentPage, publishedValueFallback)
            {
                ListingUrl = currentPage.Url(),
                Results = results,
            });
        }

        private FAQResults SearchFAQWithFAQTab(string faqQuestion, string faqCategory, IPublishedContent currentPage)
        {
            var wordsToIgnore = currentPage.Value<string>("wordsToIgnore")?.Split(' ');
            if (wordsToIgnore != null && faqQuestion != null)
                faqQuestion = string.Join(" ", faqQuestion.Split(' ').Except(wordsToIgnore));

            return faqSearcher.ExecuteFAQ(new FAQSearch() { FAQCategory = faqCategory, FAQQuestion = faqQuestion });
        }

        private FAQResults SearchFAQOnly(string faqQuestion, IPublishedContent currentPage)
        {
            autocomplete.QueryString = faqQuestion;

            var response = autocomplete.SearchFAQ();
            var criteria = response.Documents.FirstOrDefault();
            var result = new FAQResult()
            {
                Id = criteria.Id,
                NameId = criteria.Name.ToCleanString(shortStringHelper, CleanStringType.UrlSegment),
                FAQQuestion = criteria.FaqQuestion,
                SortOrder = criteria.SortOrder,
                FAQCategory = criteria.FaqCategory,
                FAQAnswer = criteria.FaqAnswer,
                FAQTabs = faqSearcher.SearchFAQTabs(criteria.Id)
            };

            return new FAQResults
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
            autocomplete.QueryString = queryString;
            autocomplete.CriteriaType = criteriaType;

            var response = autocomplete.SearchCriteriaLookup();
            return Json(new AutocompleteSuggestion { suggestions = response });
        }

        public JsonResult AutoCompleteFAQ(string queryString)
        {
            autocomplete.QueryString = queryString;

            var response = autocomplete.SearchFAQs();
            return Json(new AutocompleteSuggestion { suggestions = response });
        }

        [HttpGet]
        public ActionResult Site(string queryString)
        {
            autocomplete.QueryString = queryString;

            var response = autocomplete.Execute();
            return Json(new AutocompleteSuggestion { suggestions = response });
        }
    }
}
