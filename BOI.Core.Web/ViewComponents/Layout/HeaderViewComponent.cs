using BOI.Core.Constants;
using BOI.Core.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.UmbracoContext;
using Umbraco.Cms.Web.Common;
using Umbraco.Extensions;
using BOI.Core.Web.Models.ViewModels;
using BOI.Core.Web.Models.ViewModels.Layout;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace BOI.Core.Web.ViewComponents.Layout
{
    public class HeaderViewComponent : ViewComponent
    {

        private readonly IUmbracoContextAccessor umbracoContextAccessor;
        private readonly IUmbracoHelperAccessor umbracoHelperAccessor;
        private readonly ICmsService cmsService;
        private readonly ISessionManager sessionManager;
        private readonly IUrlHelperFactory urlHelperFactory;

        public HeaderViewComponent(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoHelperAccessor umbracoHelperAccessor, 
            ICmsService cmsService, ISessionManager sessionManager, IUrlHelperFactory urlHelperFactory)
        {
            this.umbracoContextAccessor = umbracoContextAccessor;
            this.umbracoHelperAccessor = umbracoHelperAccessor;
            this.cmsService = cmsService;
            this.sessionManager = sessionManager;
            this.urlHelperFactory = urlHelperFactory;
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
                    var viewModel = new BodyViewModel(currentPage);
                    viewModel.SiteRoot = cmsService.GetSiteRoot(currentPage);
                   
                    viewModel.SearcjAjaxUrl = CustRouePaths.AutoCompleteSearchAjax;
                    return View("Header", viewModel);
                }
            }

            return Content("");
        }
    }

}
