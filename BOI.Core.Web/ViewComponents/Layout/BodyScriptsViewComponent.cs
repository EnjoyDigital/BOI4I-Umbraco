using BOI.Core.Constants;
using BOI.Core.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.UmbracoContext;
using Umbraco.Cms.Web.Common;
using Umbraco.Extensions;
using BOI.Core.Web.Models.ViewModels.Layout;
using BOI.Core.Web.Extensions;
using BOI.Umbraco.Models;

namespace BOI.Core.Web.ViewComponents.Layout
{
    public class BodyScriptsViewComponent : ViewComponent
    {

        private readonly IUmbracoContextAccessor umbracoContextAccessor;
        private readonly IUmbracoHelperAccessor umbracoHelperAccessor;
        private readonly ICmsService cmsService;
        private readonly IUmbracoContextFactory umbracoContextFactory;
        private readonly ISessionManager sessionManager;

        public BodyScriptsViewComponent(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoHelperAccessor umbracoHelperAccessor, ICmsService cmsService, IUmbracoContextFactory umbracoContextFactory, ISessionManager sessionManager)
        {
            this.umbracoContextAccessor = umbracoContextAccessor;
            this.umbracoHelperAccessor = umbracoHelperAccessor;
            this.cmsService = cmsService;
            this.umbracoContextFactory = umbracoContextFactory;
            this.sessionManager = sessionManager;
        }


        public IViewComponentResult Invoke(bool upperScripts)
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
                    var viewModel = new BodyScriptsViewModel(currentPage);
                    viewModel.SiteRoot = cmsService.GetSiteRoot(currentPage);
                    viewModel.SiteScripts = viewModel.SiteRoot?.SiteSettings?.GetElement<GlobalScripts>();

                    viewModel.IsUpper = upperScripts;

                    return View("BodyScripts", viewModel);
                }
            }

            return Content("");
        }
    }
}
