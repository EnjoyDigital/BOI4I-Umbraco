using BOI.Core.Search.Models;
using BOI.Core.Search.Queries.Elastic;
using BOI.Core.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;

namespace BOI.Core.Web.Controllers.Hijack
{
    public class NewsLandingController : RenderController
    {
        private readonly IConfiguration config;
        private readonly IPublishedValueFallback publishedValueFallback;
        private readonly IElasticClient esClient;
        private readonly ILogger<NewsLandingController> logger;

        public NewsLandingController(IConfiguration config, IPublishedValueFallback publishedValueFallback, IElasticClient esClient, ILogger<NewsLandingController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor) : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            this.config = config;
            this.publishedValueFallback = publishedValueFallback;
            this.esClient = esClient;
            this.logger = logger;
        }

        [NonAction]
        public sealed override IActionResult Index() => throw new NotImplementedException();

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var model = new NewsArticleSearch();
            await TryUpdateModelAsync(model);

            var newsArticleSearcher = new NewsArticleSearcher(config, esClient);

            var results = newsArticleSearcher.Execute(model);

            return CurrentTemplate(new NewsLandingResultsViewModel(CurrentPage, publishedValueFallback)
            {
                ListingUrl = CurrentPage.Url(),
                Results = results,
                Paging = new Page<IPagedResult>(resultItems: results.QueryResults, totalItems: results.Total, currentPage: model.Page, pagesize: model.Size, activeClass: "-active")
            });
        }
    }
}
