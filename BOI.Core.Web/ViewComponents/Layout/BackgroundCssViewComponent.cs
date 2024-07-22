using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using BOI.Core.Constants;
using BOI.Core.Extensions;

namespace BOI.Core.Web.ViewComponents.Layout
{
    public class BackgroundCssViewComponent : ViewComponent
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public BackgroundCssViewComponent(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public IViewComponentResult Invoke()
            => View(RenderBackgroundCss());

        private HtmlString RenderBackgroundCss()
        {
            var outputCss = new StringBuilder();
            var backgroundImages = httpContextAccessor.HttpContext.Items[HttpContextItems.BackgroundImages] as Dictionary<string, string> ?? new Dictionary<string, string>();

            if (backgroundImages.NotNullAndAny())
            {
                var backgroundsKeys = backgroundImages.Keys;

                foreach (var key in backgroundsKeys)
                {
                    if (key == "0")
                    {
                        outputCss.Append(backgroundImages[key]);
                    }
                    else
                    {
                        outputCss.AppendFormat("@media only screen and (min-width: {0}px){{{1}}}", key, backgroundImages[key]);
                    }
                }
            }

            return new HtmlString(outputCss.ToString());
        }

    }
}
