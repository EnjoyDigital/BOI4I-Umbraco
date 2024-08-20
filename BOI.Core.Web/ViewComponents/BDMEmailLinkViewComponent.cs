using BOI.Core.Extensions;
using BOI.Core.Search.Queries.Elastic;
using BOI.Core.Web.ViewComponents.Layout;
using BOI.Umbraco.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace BOI.Core.Web.ViewComponents
{
    public class BDMEmailLinkViewComponent : ViewComponent
    {
        private readonly IBdmResolver bdmResolver;
        private readonly IHttpContextAccessor contextAccessor;

        public BDMEmailLinkViewComponent(IBdmResolver bdmResolver, IHttpContextAccessor contextAccessor)
        {
            this.bdmResolver = bdmResolver;
            this.contextAccessor = contextAccessor;
        }

        public IViewComponentResult Invoke(IPublishedElement publishedElement)
        {
            var emailLinkReceiver = publishedElement as BDmemailLinkReceiver;
            if (emailLinkReceiver == null) {
                return Content("");
            }
            var qsKey = emailLinkReceiver.LinkQueryStringKey.HasValue() ? emailLinkReceiver.LinkQueryStringKey : "bdm";
            var qsValue = Request.Query[qsKey];
            if (qsValue.NullOrEmpty())
            {
                return Content("");
            }
            var bdmQuery = new BdmResolverQuery() { BDMQueryString = qsValue.FirstOrDefault() };

            var bdmResult = bdmResolver.Execute(bdmQuery);

            if (bdmResult != null)
            {
                SetBDMCookie(bdmResult.ItemId.ToString());
                return Content(emailLinkReceiver.BDmfound.ToString());
            }
            else
            {
                SetBDMCookie("");
                return Content(emailLinkReceiver.BDmnotFound.ToString());
            }

            
        }

        private void SetBDMCookie(string cookieValue)
        {
            var existingCookieValue = Request.Cookies[PodsViewComponent.bdmCookieKey];
            var cookieOptions = new CookieOptions()
            {
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddYears(100),
                Secure = true,
                
            };
            if (!existingCookieValue.HasValue())
            {
                
                var newCookie = new Cookie(PodsViewComponent.bdmCookieKey, cookieValue );

                HttpContext.Response.Cookies.Append(PodsViewComponent.bdmCookieKey, cookieValue, cookieOptions);
            }
            else
            {
                if(existingCookieValue!= cookieValue)
                {
                    HttpContext.Response.Cookies.Append(PodsViewComponent.bdmCookieKey, cookieValue, cookieOptions);
                }
            }
            

        }

    }
}
