using BOI.Core.Extensions;
using BOI.Core.Search.Models;
using BOI.Core.Search.Queries.Elastic;
using BOI.Core.Search.Queries.PostcodeLookup;
using BOI.Core.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;

namespace BOI.Core.Web.Controllers.Hijack
{
    public class SolicitorLandingController : RenderController
    {
        private readonly IConfiguration config;
        private readonly ILogger<SolicitorLandingController> logger;
        private readonly IPublishedValueFallback publishedValueFallback;
        private readonly IElasticClient esClient;

        public SolicitorLandingController(IConfiguration config, ILogger<SolicitorLandingController> logger,
            IPublishedValueFallback publishedValueFallback,ICompositeViewEngine compositeViewEngine, IElasticClient esClient,
            IUmbracoContextAccessor umbracoContextAccessor) : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            this.config = config;
            this.logger = logger;
            this.publishedValueFallback = publishedValueFallback;
            this.esClient = esClient;
        }

        [NonAction]
        public sealed override IActionResult Index() => throw new NotImplementedException();

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var model = new SolicitorSearch();
            await TryUpdateModelAsync(model);

            if (model.Postcode.HasValue() || model.SolicitorName.HasValue())
            {
                SolicitorResultsViewModel viewModel = null;

                if (model.Postcode.HasValue())// || model.SolicitorName.HasValue())
                                              //if (Request.IsAjaxRequest())
                {
                    var lookupQuery = new PostcodeLookupQuery.Request();
                    lookupQuery.Postcode = model.Postcode;
                    var requestHandler = new PostcodeLookupQuery.RequestHandler(config);


                    var coordinates = requestHandler.Execute(lookupQuery);// GetLatLong(model.Postcode);
                    if (coordinates.Latitude != 0.0 && coordinates.Longitude != 0.0)
                    {
                        model.Lat = float.Parse(coordinates.Latitude.ToString());
                        model.Lon = float.Parse(coordinates.Longitude.ToString());
                    }
                    else
                    {
                        viewModel = new SolicitorResultsViewModel(CurrentPage, publishedValueFallback)
                        {
                            Results = new SolicitorsResults()
                        };
                        return Request.IsAjaxRequest() ? PartialView("partials/SolicitorLookup/SolicitorLookUpResultList", viewModel) : CurrentTemplate(viewModel);
                    }

                }

                var solicitorSearcher = new SolicitorSearcher(config, esClient);

                var results = solicitorSearcher.Execute(model);

                if (results.Total != 0)
                {
                    TryUpdateModelAsync(results);
                }
                viewModel = new SolicitorResultsViewModel(CurrentPage, publishedValueFallback)
                {
                    Results = results,
                    ListingUrl = CurrentPage.Url(),
                    Paging = new Page<SolicitorResult>(resultItems: results.QueryResults, totalItems: results.Total, currentPage: model.Page, pagesize: model.Size, activeClass: "-active")
                };
                return Request.IsAjaxRequest() ? PartialView("partials/SolicitorLookup/SolicitorLookUpResultList", viewModel) : CurrentTemplate(viewModel);
            }
            else
            {
                return CurrentTemplate(new SolicitorResultsViewModel(CurrentPage, publishedValueFallback) { ListingUrl = CurrentPage.Url() });
            }
        }
    }
}
