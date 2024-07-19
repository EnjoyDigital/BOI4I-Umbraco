using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core.DependencyInjection;

namespace BOI.Core.Search.Extensions
{
    public static class IUmbracoBuilderExtensions
    {
        public static IUmbracoBuilder RegisterQueryHandlers(this IUmbracoBuilder builder, IConfiguration configuration)
        {

            //builder.Services.AddScoped<IListingQueryHandler, Queries.Examine.ListingQueryHandler>();
            //builder.Services.AddScoped<IMediaQueryHandler, Queries.Examine.MediaQueryHandler>();
            //builder.Services.AddScoped<ISiteSearchQueryHandler, Queries.Examine.SiteSearchQueryHandler>();
            //builder.Services.AddScoped<IAutoCompleteQueryHandler, Queries.Examine.AutoCompleteQueryHandler>();

            return builder;
        }
    }
}
