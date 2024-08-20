using BOI.Core.Constants;
using BOI.Core.Search.Constants;
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
            //context.EndpointRouteBuilder.MapControllerRoute(
            //    CustomRouteNames.RobotsTxt,
            //    "/robots.txt",
            //    new
            //    {
            //        controller = "RobotsTxt",
            //        action = "Robots"
            //    }
                //  "/robots/",
                //new
                //{
                //    controller = "CustomRouteAction",
                //    action = "RobotsTxt"
                //}
           // );

            //context.EndpointRouteBuilder.MapControllerRoute(
            //    CustomRouteNames.SitemapXml,
            //    "/sitemap.xml",
            //    new
            //    {
            //        controller = "SitemapXml",
            //        action = "Index"
            //    }
            //);

            context.EndpointRouteBuilder.MapControllerRoute(
                CustomRouteNames.CriteriaLookupAjax,
                "/findResidentialCriteria/",
                new
                {
                    controller = "CustomRouteAction",
                    action = "FindResidentialCriteria"
                }
            );

            context.EndpointRouteBuilder.MapControllerRoute(
                CustomRouteNames.AutoCompleteCriteriaLookupAjax,
                "/autoCompleteResidentialCriteria/",
                new
                {
                    controller = "CustomRouteAction",
                    action = "AutoCompleteCriteriaLookup",
                    criteriaType = FieldConstants.ResidentialProductType
                }
            );

            context.EndpointRouteBuilder.MapControllerRoute(
                CustomRouteNames.CriteriaLookupAjaxBuyToLet,
                "/findBuyToLetCriteria/",
                new
                {
                    controller = "CustomRouteAction",
                    action = "FindBuyToLetCriteria",
                }
            );

            context.EndpointRouteBuilder.MapControllerRoute(
                CustomRouteNames.AutoCompleteCriteriaLookupAjax,
                "/autoCompleteBuyToLetCriteria/",
                new
                {
                    controller = "CustomRouteAction",
                    action = "AutoCompleteCriteriaLookup",
                    criteriaType = FieldConstants.BuyToLetProductType
                }
            );

            context.EndpointRouteBuilder.MapControllerRoute(
                CustomRouteNames.CriteriaLookupAjaxBespoke,
                "/findBespokeCriteria/",
                new
                {
                    controller = "CustomRouteAction",
                    action = "FindBespokeCriteria",
                }
            );

            context.EndpointRouteBuilder.MapControllerRoute(
                CustomRouteNames.AutoCompleteCriteriaLookupBespokeAjax,
                "/autoCompleteBespokeCriteria/",
                new
                {
                    controller = "CustomRouteAction",
                    action = "AutoCompleteCriteriaLookup",
                    criteriaType = FieldConstants.BespokeProductType
                }
            );


            context.EndpointRouteBuilder.MapControllerRoute(
                CustomRouteNames.ProductLandingAjax,
                "/findProductsLanding",
                new
                {
                    controller = "ProductsLanding",
                    action = "FindProductsLanding"
                }
            );

            context.EndpointRouteBuilder.MapControllerRoute(
               CustomRouteNames.AutoCompleteSearchAjax,
               CustRouePaths.AutoCompleteSearchAjax,
               new
               {
                   controller = "CustomRouteAction",
                   action = "Site"
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
