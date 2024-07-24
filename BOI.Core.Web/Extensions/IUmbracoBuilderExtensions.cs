using BOI.Core.Services;
using BOI.Core.Extensions;
using BOI.Core.Web.Services;
using BOI.Core.Web.Services.CachedProxies;
using BOI.Core.Constants.Aliases;
using BOI.Core.Web.ContentFinders;
using BOI.Core.Web.NotificationHandlers;
using DangEasy.Interfaces.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using DangEasy.Caching.MemoryCache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;
using Umbraco.Cms.Infrastructure.Examine.DependencyInjection;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Notifications;
using BOI.Core.Search.Services;
using BOI.Core.Search.Factory;
using Umbraco.Forms.Core.Cache;
using Microsoft.AspNetCore.DataProtection;
using BOI.Core.Infrastructure;
using Lucene.Net.Search;
using BOI.Core.Search.NotificationHandlers;
using BOI.Core.Search.Queries.SQL;


namespace BOI.Core.Web.Extensions
{
    public static class IUmbracoBuilderExtensions
    {
        public static IUmbracoBuilder AddCustomUmbracoDataContext(this IUmbracoBuilder builder)
        {
            builder.Services.AddDbContext<CustomUmbracoDataContext>(options => options.UseSqlServer("name=ConnectionStrings:umbracoDbDSN"));

            return builder;
        }

        public static IUmbracoBuilder AddDataProtection(this IUmbracoBuilder builder, IConfiguration _config)
        {
            var applicationName = _config.GetValue<string>("", "BOI");
            builder.Services.AddDataProtection()
                .SetApplicationName(applicationName);
                //.PersistKeysToDbContext<CustomUmbracoDataContext>();

            return builder;
        }

        public static IUmbracoBuilder AddServerRegistrar(this IUmbracoBuilder builder, IConfiguration configuration)
        {

            var serverRole = configuration.GetValue<string>("Website:ServerRole");

            if (!string.IsNullOrWhiteSpace(serverRole))
            {
                if (serverRole == ServerRegistrarNames.SchedulingPublisher)
                {
                    builder.SetServerRegistrar<SchedulingPublisherServerRoleAccessor>();
                }

                if (serverRole == ServerRegistrarNames.SubscriberServer)
                {
                    builder.SetServerRegistrar<SubscriberServerRoleAccessor>();
                }
            }

            return builder;
        }

        public static IUmbracoBuilder AddCustomServices(this IUmbracoBuilder builder, IConfiguration configuration)
        {
            builder.RegisterCustomCachedServices(configuration);
            builder.RegisterCustomServices();

            return builder;
        }  

        private static IUmbracoBuilder RegisterCustomCachedServices(this IUmbracoBuilder builder, IConfiguration configuration)
        {
            if (configuration.IsServiceCacheEnabled())
            {
                builder.Services.AddScoped<ICache, Cache>();

                builder.Services.AddScoped<CmsService, CmsService>();
                builder.Services.AddScoped<ICmsService, CmsServiceCachedProxy>();

                builder.Services.AddScoped<SitemapXmlGenerator, SitemapXmlGenerator>();
                builder.Services.AddScoped<ISitemapXmlGenerator, SitemapXmlGeneratorCachedProxy>();
            }
            else
            {
                builder.Services.AddScoped<ICmsService, CmsService>();
                builder.Services.AddScoped<ISitemapXmlGenerator, SitemapXmlGenerator>();
            }

            return builder;
        }

        private static IUmbracoBuilder RegisterCustomServices(this IUmbracoBuilder builder)
        {
            //TODO: Add edservices
            //builder.Services.AddScoped<IEdAdminService, EdAdminService>();
            builder.Services.AddScoped<IRazorViewRenderService, RazorViewRenderService>();
            builder.Services.AddScoped<ICacheTagHelperService, CacheTagHelperService>();
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<IOutputCacheService, OutputCacheService>();
            return builder;
        }

        public static IUmbracoBuilder RegisterQueryHandlers(this IUmbracoBuilder builder, IConfiguration configuration)
        {
            builder.Services.AddScoped<IndexingService, IndexingService>();
            builder.Services.AddSingleton(s =>
            {

                var esFactory = new EsSearchFactory(configuration);

                return esFactory.CreateClient();
            });

            builder.Services.AddScoped<IGetAllMediaLogs, GetAllMediaLogs>();


            builder.AddNotificationHandler<ContentSavedNotification, ContentSavedNotificationHandler>();
            builder.AddNotificationHandler<ContentPublishedNotification, ContentPublishedNotificationHandler>();
            builder.AddNotificationHandler<ContentMovingToRecycleBinNotification, ContentServiceDeletingHandler>();
            builder.AddNotificationHandler<ContentUnpublishingNotification, ContentUnPublishingNotificationHandler>();


            //builder.Services.AddScoped<IListingQueryHandler, Queries.Examine.ListingQueryHandler>();
            //builder.Services.AddScoped<IMediaQueryHandler, Queries.Examine.MediaQueryHandler>();
            //builder.Services.AddScoped<ISiteSearchQueryHandler, Queries.Examine.SiteSearchQueryHandler>();
            //builder.Services.AddScoped<IAutoCompleteQueryHandler, Queries.Examine.AutoCompleteQueryHandler>();

            return builder;
        }

        public static IUmbracoBuilder AddCustomNotificationHandlers(this IUmbracoBuilder builder)
        {
            builder.AddNotificationHandler<ContentCacheRefresherNotification, ContentCacheRefresherNotificationHandler>();
            builder.AddNotificationHandler<MediaCacheRefresherNotification, MediaCacheRefresherNotificationHandler>();
            builder.AddNotificationHandler<FormCacheRefresherNotification, FormCacheRefresherNotificationHandler>();

            return builder;
        }

        public static IUmbracoBuilder AddCustomContentFinders(this IUmbracoBuilder builder)
        {
            builder.ContentFinders().InsertAfter<ContentFinderByUrl, ListingFiltersContentFinder>();
            //ToDO: PB to uncomment the following
            //builder.SetContentLastChanceFinder<LastChanceContentFinder>();

            return builder;
        }
    }
}
