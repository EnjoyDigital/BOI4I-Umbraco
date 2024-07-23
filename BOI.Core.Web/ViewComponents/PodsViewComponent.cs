using BOI.Core.Constants;
using BOI.Core.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.UmbracoContext;
using Umbraco.Cms.Web.Common;
using Umbraco.Extensions;
using BOI.Core.Web.Models.ViewModels;
using Umbraco.Cms.Core.Models.PublishedContent;
using BOI.Umbraco.Models;

namespace BOI.Core.Web.ViewComponents.Layout
{
    public class PodsViewComponent : ViewComponent
    {

        private readonly IUmbracoContextAccessor umbracoContextAccessor;
        private readonly IUmbracoHelperAccessor umbracoHelperAccessor;
        private readonly ICmsService cmsService;
        private readonly ISessionManager sessionManager;

        public PodsViewComponent(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoHelperAccessor umbracoHelperAccessor, ICmsService cmsService, ISessionManager sessionManager)
        {
            this.umbracoContextAccessor = umbracoContextAccessor;
            this.umbracoHelperAccessor = umbracoHelperAccessor;
            this.cmsService = cmsService;
            this.sessionManager = sessionManager;
        }


        public IViewComponentResult Invoke(IPublishedContent publishedContent, bool isHeroPod =false)
        {
            var alias = publishedContent.ContentType.Alias;

            var partialPath = $"/Views/Partials/Pods/{alias}.cshtml";

            return alias switch
            {


                _ => View(partialPath, publishedContent)
            };

            return Content("");
        }

        private const string bdmCookieKey = "bdm";

        //private BDmcontact BDMPod(IPublishedContent publishedContent)
    //    {

    //        //check and retrieve the specific BDM if has been set
    //        // If no BDM is found use the fallback in the pod, change to suit
    //        var bdmCookie = string.Empty;
    //        if( Request.Cookies.TryGetValue(bdmCookieKey, out bdmCookie))
    //        { 
    //        }
    //        if (bdmCookie != null && bdmCookie.Value.HasValue())
    //        {
    //            model.BDMDetails = Umbraco.Content(bdmCookie.Value) as BDmcontact;
    //        }


    //        if (!model.BDMFound)
    //        {
    //            if (heroPod)
    //            {
    //                return PartialView("~/Views/Partials/Pods/HeroBdmPod.cshtml", model.BDmfallbackContact);
    //            }

    //            return PartialView("~/Views/Partials/Pods/BDMPod.cshtml", model.BDmfallbackContact);
    //        }
    //        else
    //        {
    //            if (heroPod)
    //            {
    //                return PartialView("~/Views/Partials/Pods/HeroBdmPod.cshtml", model.BDMDetails);
    //            }

    //            return PartialView("~/Views/Partials/Pods/BDMPod.cshtml", model.BDMDetails);
    //        }


    //    }

    }

}
