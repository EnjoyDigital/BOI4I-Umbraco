using BOI.Core.Constants;
using BOI.Core.Web.Models.ViewModels.Layout;
using BOI.Core.Web.Services;
using BOI.Core.Web.Extensions;
using BOI.Umbraco.Models;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.UmbracoContext;
using Umbraco.Extensions;

namespace BOI.Core.Web.ViewComponents.Layout
{
    public class HeadViewComponent : ViewComponent
    {
        private readonly IUmbracoContextAccessor umbracoContextAccessor;
        private readonly IUmbracoHelperAccessor umbracoHelperAccessor;
        private readonly ISessionManager sessionManager;
        private readonly UmbracoHelper umbracoHelper;
        private readonly ICmsService cmsService;

        public HeadViewComponent(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoHelperAccessor umbracoHelperAccessor,
            ISessionManager sessionManager, UmbracoHelper umbracoHelper, ICmsService cmsService)
        {
            this.umbracoContextAccessor = umbracoContextAccessor;
            this.umbracoHelperAccessor = umbracoHelperAccessor;
            this.sessionManager = sessionManager;
            this.umbracoHelper = umbracoHelper;
            this.cmsService = cmsService;
        }

        public IViewComponentResult Invoke(HeadViewModel model)
        {
            if (umbracoContextAccessor.GetRequiredUmbracoContext() is UmbracoContext umbracoContext)
            {
                var currentPage = umbracoContext.PublishedRequest?.PublishedContent;
                if (currentPage == null)
                {

                    currentPage = umbracoHelper.Content(sessionManager.GetSessionValue(SessionConstants.PageId));

                }
                if (currentPage != null)
                {
                    var viewModel = new HeadViewModel(currentPage);
                    viewModel.SiteRoot = cmsService.GetSiteRoot(currentPage);
                    viewModel.SiteScripts = viewModel.SiteRoot?.SiteSettings?.FirstOrDefault() as GlobalScripts;

                    if (viewModel.PageSettings?.PageSettings != null)
                    {
                        viewModel.PageScripts = viewModel.PageSettings.PageSettings.FirstOrDefault() as ElementScriptSettings;
                    }

                    return View("Head", viewModel);
                }
            }

            return Content("");
        }

    }
}
