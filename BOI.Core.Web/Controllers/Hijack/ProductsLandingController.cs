using BOI.Core.Search.Queries.Elastic;
using BOI.Core.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nest;
using Umbraco.Cms.Web.Common.Controllers;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using Umbraco.Cms.Web.Common;

namespace BOI.Core.Web.Controllers.Hijack
{
    public class ProductsLandingController : RenderController
    {
        private readonly IConfiguration config;
        private readonly IPublishedValueFallback publishedValueFallback;
        private readonly IElasticClient esClient;
        private readonly UmbracoHelper umbracoHelper;

        public ProductsLandingController(IConfiguration config, IPublishedValueFallback publishedValueFallback,
            IElasticClient esClient, ILogger<NewsLandingController> logger, ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor, UmbracoHelper umbracoHelper) : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            this.config = config;
            this.publishedValueFallback = publishedValueFallback;
            this.esClient = esClient;
            this.umbracoHelper = umbracoHelper;
        }

        [NonAction]
        public sealed override IActionResult Index() => throw new NotImplementedException();

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var model = new ProductsSearch();
            await TryUpdateModelAsync(model);

            var productSearcher = new ProductSearcher(config, esClient);
            model.ProductVariant = CurrentPage.Value<string>("productVariant");

            var results = productSearcher.Execute(model);

            var typeList = new List<SelectListItem>();
            typeList.AddRange(results.RefineFilters.ProductTypeList.Select(c => new SelectListItem
            {
                Text = c.Value,
                Value = c.Value,
                Selected = model.ProductType == c.Value
            }));
            typeList.Sort((x, y) => string.Compare(x.Text, y.Text));
            typeList = typeList.Prepend(new SelectListItem { Text = "All products", Value = "null" }).ToList();

            var termList = new List<SelectListItem>();
            int term;
            termList.AddRange(results.RefineFilters.ProductTermList.Select(c => new SelectListItem
            {
                Text = c.Value + (int.TryParse(c.Value, out term) ? " Years" : " Rate"),
                Value = c.Value,
                Selected = model.ProductTerm == c.Value
            }));
            termList.Sort((x, y) => string.Compare(x.Text, y.Text));
            termList = termList.Prepend(new SelectListItem { Text = "All terms", Value = "null" }).ToList();

            var categoryList = new List<SelectListItem>();
            categoryList.AddRange(results.RefineFilters.ProductCategoryList.Select(c => new SelectListItem
            {
                Text = c.Value,
                Value = c.Value,
                Selected = model.ProductCategory == c.Value
            }));
            categoryList.Sort((x, y) => string.Compare(x.Text, y.Text));
            categoryList = categoryList.Prepend(new SelectListItem { Text = "All categories", Value = "null" }).ToList();

            var ltvList = new List<SelectListItem>();
            ltvList.AddRange(results.RefineFilters.ProductLTVList.Select(c => new SelectListItem
            {
                Text = c.Value,
                Value = c.Value,
                Selected = model.ProductLTV == c.Value
            }));
            ltvList.Sort((x, y) => string.Compare(x.Text, y.Text));
            ltvList = ltvList.Prepend(new SelectListItem { Text = "All LTVs", Value = "null" }).ToList();

            return CurrentTemplate(new ProductsLandingResultsViewModel(CurrentPage, publishedValueFallback)
            {
                ListingUrl = CurrentPage.Url(),
                ProductTypeList = typeList,
                ProductTermList = termList,
                ProductCategoryList = categoryList,
                ProductLTVList = ltvList,
                Results = results,
                InterestOnly = model.InterestOnly
            });
        }

        public ActionResult FindProductsLanding(string productType, string productTerm, string productCategory, string productLTV, bool interestOnly, int pageId)
        {
            var productSearcher = new ProductSearcher(config, esClient);
            var results = productSearcher.Execute(new ProductsSearch() { ProductType = productType, ProductTerm = productTerm, ProductCategory = productCategory, ProductLTV = productLTV, InterestOnly = interestOnly });

            var currentPage = umbracoHelper.Content(pageId);

            return PartialView("~/Views/Partials/ProductsLanding/ProductsLandingResults.cshtml", new ProductsLandingResultsViewModel(currentPage, publishedValueFallback)
            {
                ListingUrl = currentPage.Url(),
                Results = results,
            });
        }
    }

}
