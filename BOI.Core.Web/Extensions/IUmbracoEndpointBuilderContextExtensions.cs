using BOI.Core.Constants;
using Microsoft.AspNetCore.Builder;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace BOI.Core.Web.Extensions
{
    public static class IUmbracoEndpointBuilderContextExtensions
    {
        public static IUmbracoEndpointBuilderContext UseCustomRoutes(this IUmbracoEndpointBuilderContext context)
        {
            //Constants should be used for the route name
            //then Url.RouteUrl can be called in views to generate the url.
            //please dont hard code the url in views, or use magic strings for the route names
            context.EndpointRouteBuilder.MapControllerRoute(
                CustomRouteNames.RobotsTxt,
                "/robots.txt",
                new
                {
                    controller = "RobotsTxt",
                    action = "Index"
                }
            );

            context.EndpointRouteBuilder.MapControllerRoute(
                CustomRouteNames.SitemapXml,
                "/sitemap.xml",
                new
                {
                    controller = "SitemapXml",
                    action = "Index"
                }
            );

            context.EndpointRouteBuilder.MapControllerRoute(
                CustomRouteNames.Autocomplete,
                "/api/autocomplete/{action}",
                new
                {
                    controller = "AutoComplete",
                    action = "product"
                }
            );
            return context;
        }
    }
}
