using BOI.Core.Web.Services;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;

namespace BOI.Core.Web.Controllers.NonPage
{
    public class SitemapXmlController : UmbracoPageController
    {
        private readonly IDomainService domainService;
        private readonly ISitemapXmlGenerator sitemapXmlGenerator;

        public SitemapXmlController(IDomainService domainService, ISitemapXmlGenerator sitemapXmlGenerator,
            ILogger<SitemapXmlController> logger, ICompositeViewEngine compositeViewEngine)
            : base(logger, compositeViewEngine)
        {
            this.domainService = domainService;
            this.sitemapXmlGenerator = sitemapXmlGenerator;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var baseUrl = Request.Host.Host;
            var domain = domainService.GetByName(Request.Host.Host);
            //var sitemapItems = sitemapXmlGenerator.GetSitemap(domain.RootContentId.Value, baseUrl);
            //TODO: PB to check why its not working
            var sitemapItems = sitemapXmlGenerator.GetSitemap(1093, baseUrl);

            Response.Headers.Clear();
            Response.ContentType = "text/xml";

            return View("SitemapXml", sitemapItems);
        }
    }
}
