using BOI.Core.Constants;
using BOI.Core.Web.Models.ViewModels.Layout;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.UmbracoContext;
using Umbraco.Cms.Web.Common;
using Umbraco.Extensions;
using BOI.Core.Web.Services;

namespace BOI.Core.Web.ViewComponents.Layout
{
    public class ScrollBarBlockViewComponent : ViewComponent
    {

        private readonly IUmbracoContextAccessor umbracoContextAccessor;
        private readonly IUmbracoHelperAccessor umbracoHelperAccessor;
        private readonly ISessionManager sessionManager;
        private readonly ICmsService cmsService;

        public ScrollBarBlockViewComponent(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoHelperAccessor umbracoHelperAccessor, ISessionManager sessionManager,
            ICmsService cmsService)
        {
            this.umbracoContextAccessor = umbracoContextAccessor;
            this.umbracoHelperAccessor = umbracoHelperAccessor;
            this.sessionManager = sessionManager;
            this.cmsService = cmsService;
        }


        public IViewComponentResult Invoke()
        {
            if (umbracoContextAccessor.GetRequiredUmbracoContext() is UmbracoContext umbracoContext)
            {
                var currentPage = umbracoContext.PublishedRequest?.PublishedContent;
                if (currentPage == null)
                {
                    if (umbracoHelperAccessor.TryGetUmbracoHelper(out var helper))
                    {
                        currentPage = helper.Content(sessionManager.GetSessionValue(SessionConstants.PageId));
                    }
                    else
                    {
                        return Content("");
                    }
                }

                if (currentPage != null)
                {
                    var viewModel = new ScrollBarBlockViewModel(currentPage);
                    viewModel.SiteRoot = cmsService.GetSiteRoot(currentPage);
                    return View("ScrollBarBlock", viewModel);
                }
            }

            return Content("");
        }
    }

}
