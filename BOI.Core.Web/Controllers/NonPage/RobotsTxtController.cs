using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using System.Text;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using BOI.Core.Web.Services;
using Umbraco.Extensions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Web.Common;
using BOI.Umbraco.Models;

namespace BOI.Core.Web.Controllers.NonPage
{
    public class RobotsTxtController : UmbracoPageController
    {
        private readonly ICmsService cmsService;
        private readonly IDomainService domainService;
        private readonly IUmbracoContextAccessor umbracoContextAccessor;
        private readonly IUmbracoContextFactory umbracoContextFactory;
        private readonly IUmbracoHelperAccessor umbracoHelperAccessor;
        
        private readonly IPublishedValueFallback valueFallback;

        public RobotsTxtController(ICmsService cmsService, IDomainService domainService,ILogger<RobotsTxtController> logger,IUmbracoContextAccessor umbracoContextAccessor,
                 IUmbracoContextFactory umbracoContextFactory, ICompositeViewEngine compositeViewEngine, IPublishedValueFallback valueFallback)
                : base(logger, compositeViewEngine)
        {
            this.cmsService = cmsService;
            this.domainService = domainService;
            this.umbracoContextAccessor = umbracoContextAccessor;
            this.umbracoContextFactory = umbracoContextFactory;
            this.valueFallback = valueFallback;
        }

        public IPublishedContent? FindContent(ActionExecutingContext actionExecutingContext)
        {
            throw new NotImplementedException();
        }

        public ActionResult Robots() 
        {
            if (!umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext umbracoContext))
            {

            }
            
                
                var domain = domainService.GetByName(Request.Host.Host);
                UmbracoHelper umbracoHelper;

            using (var umbrFact = umbracoContextFactory.EnsureUmbracoContext())
            {
                var testFact = umbrFact.UmbracoContext.Content.GetById(domain.RootContentId.GetValueOrDefault());
                }
            if (!umbracoHelperAccessor.TryGetUmbracoHelper(out umbracoHelper))
            {
                throw new Exception("Umbraco helper not available");
            }

            var test = umbracoContext.Content.GetAtRoot();
            var siteRoot =  umbracoHelper.Content(domain.RootContentId.Value) as SiteRoot;
                //var siteRoot = cmsService.GetSiteRoot(domain.RootContentId.Value);
                var siteSeoSettings = siteRoot;

                string robotsTxt;
            //if (siteSeoSettings == null || siteSeoSettings.DisallowRobots)
            
                if(siteRoot == null )            
                {
                    robotsTxt = "x;
                }
                else
                {
                    var disallowRobots = siteRoot.Properties.FirstOrDefault(x => x.Alias == "disallowRobots");
                    if (disallowRobots != null && disallowRobots.Value<bool>(valueFallback))
                    {
                        robotsTxt = "User-agent: *\nDisallow: /";
                    }
                    else
                    {
                        robotsTxt = siteSeoSettings.RobotsMeta;
                    }
                
                }

                return Content(robotsTxt, "text/text", Encoding.UTF8);
            }
        
    }
}