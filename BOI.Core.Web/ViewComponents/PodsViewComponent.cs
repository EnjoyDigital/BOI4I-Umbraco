using BOI.Core.Constants;
using BOI.Core.Extensions;
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

        private readonly IUmbracoHelperAccessor umbracoHelperAccessor;
      
        public const string bdmCookieKey = "bdm";

        public PodsViewComponent(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoHelperAccessor umbracoHelperAccessor, ICmsService cmsService, ISessionManager sessionManager)
        {
           
            this.umbracoHelperAccessor = umbracoHelperAccessor;
           
        }


        public IViewComponentResult Invoke(IPublishedContent publishedContent, bool isHeroPod = false)
        {
            var alias = publishedContent.ContentType.Alias;

            var partialPath = $"/Views/Partials/Pods/{alias}.cshtml";

            return alias switch
            {
                BDmpodAction.ModelTypeAlias => BDMPod(publishedContent, isHeroPod),

                _ => View(partialPath, publishedContent)
            };


        }



        private IViewComponentResult BDMPod(IPublishedContent publishedContent, bool isHeroPod)
        {
            var model = publishedContent as BDmpodAction;

            //check and retrieve the specific BDM if has been set
            // If no BDM is found use the fallback in the pod, change to suit
            var bdmCookie = string.Empty;
            if (Request.Cookies.TryGetValue(bdmCookieKey, out bdmCookie))
            {

                if (umbracoHelperAccessor.TryGetUmbracoHelper(out var umbracoHelper))
                {
                    if (bdmCookie != null && bdmCookie.HasValue())
                    {
                        model.BDMDetails = umbracoHelper.Content(bdmCookie.TryParseInt32().GetValueOrDefault()) as BDmcontact;
                    }
                }

            }

            if (!model.BDMFound)
            {
                if (isHeroPod)
                {
                    return View("~/Views/Partials/Pods/HeroBdmPod.cshtml", model.BDmfallbackContact);
                }

                return View("~/Views/Partials/Pods/BDMPod.cshtml", model.BDmfallbackContact);
            }
            else
            {
                if (isHeroPod)
                {
                    return View("~/Views/Partials/Pods/HeroBdmPod.cshtml", model.BDMDetails);
                }

                return View("~/Views/Partials/Pods/BDMPod.cshtml", model.BDMDetails);
            }


        }

    }
}


