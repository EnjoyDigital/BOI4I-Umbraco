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
    public class WithdrawalProductsLandingController : RenderController
    {
        private readonly IConfiguration config;
        private readonly IPublishedValueFallback publishedValueFallback;
        private readonly IElasticClient esClient;
        private readonly UmbracoHelper umbracoHelper;

        public WithdrawalProductsLandingController(IConfiguration config, IPublishedValueFallback publishedValueFallback,
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

            var results = productSearcher.ExecuteWithdrwalProducts(model);

            return CurrentTemplate(new ProductsLandingResultsViewModel(CurrentPage, publishedValueFallback)
            {
                ListingUrl = CurrentPage.Url(),
                Results = results,
                InterestOnly = model.InterestOnly
            });
        }
    }
}
