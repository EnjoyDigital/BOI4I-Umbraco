using BankOfIreland.Intermediaries.Feature.Search.Queries.Elastic;
using BOI.Core.Extensions;
using BOI.Core.Search.Models;
using BOI.Core.Web.Models.ViewModels;
using BOI.Umbraco.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace BOI.Core.Web.Controllers.Hijack
{

    public class SearchResultController : RenderController
    {

        // private readonly IElasticClient esClient;
        private readonly ISearchResultSearcher searchResultSearcher;
                private readonly ILogger<SolicitorLandingController> logger;

        public SearchResultController(ISearchResultSearcher searchResultSearcher,  ILogger<SolicitorLandingController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor) 
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            this.searchResultSearcher = searchResultSearcher;
            this.logger = logger;
        }

        [NonAction]
        public sealed override IActionResult Index() => throw new NotImplementedException();


        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var model = new MainSearchQuery();
            await TryUpdateModelAsync(model);

            if (model.Filters.NotNullAndAny())
            {
                var results = searchResultSearcher.Execute(model);

                

                return CurrentTemplate(new SearchResultsViewModel() { Content = CurrentPage as SearchResult ,
                    Results = results,
                    SearchTerm = model.SearchTerm,
                    Paging = new Page<IPagedResult>(resultItems: results.QueryResults, totalItems: results.Total, currentPage: model.Page, pagesize: model.Size, activeClass: "-active")
                });
            }
            else
            {
                return CurrentTemplate(new SearchResultsViewModel() { Content = CurrentPage as SearchResult });
            }
        }

    }
}
