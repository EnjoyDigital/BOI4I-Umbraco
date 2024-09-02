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
    public class FAQLandingController : RenderController
    {
        private readonly IConfiguration config;
        private readonly IPublishedValueFallback publishedValueFallback;
        private readonly IElasticClient esClient;
        private readonly UmbracoHelper umbracoHelper;
        private readonly IShortStringHelper shortStringHelper;

        public FAQLandingController(IConfiguration config, IPublishedValueFallback publishedValueFallback,
            IElasticClient esClient, ILogger<FAQLandingController> logger, ICompositeViewEngine compositeViewEngine,
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
            var model = new FAQSearch();
            await TryUpdateModelAsync(model);
           
            var faqSearcher = new FAQSearcher(config, esClient,shortStringHelper);
            var search = faqSearcher.FAQFormValues();

            var categoryList = new List<SelectListItem>();
            categoryList.AddRange(search.Terms("FaqCategory").Buckets.Where(x => x.Key.HasValue()).Select(c => new SelectListItem
            {
                Text = c.Key,
                Value = c.Key,
                Selected = model.FAQCategory == c.Key
            }));
            categoryList.Sort((x, y) => string.Compare(x.Text, y.Text));
            categoryList = categoryList.Prepend(new SelectListItem { Text = "All Categories", Value = "null" }).ToList();

            var results = faqSearcher.ExecuteFAQ(model);
        
            return CurrentTemplate(new FAQResultsViewModel(CurrentPage,publishedValueFallback)
            {
                ListingUrl = CurrentPage.Url(),
                CategoryList = categoryList,
                Results = results,
            });
        }
    }
}
