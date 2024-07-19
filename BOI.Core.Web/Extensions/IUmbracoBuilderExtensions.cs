using BOI.Core.Services;
using BOI.Core.Extensions;
using BOI.Core.Web.Services;
using DangEasy.Interfaces.Caching;
using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;
using Umbraco.Cms.Infrastructure.Examine.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using DangEasy.Caching.MemoryCache;
using BOI.Core.Web.Services.CachedProxies;
using BOI.Core.Constants.Aliases;
using Umbraco.Cms.Infrastructure.DependencyInjection;

namespace BOI.Core.Web.Extensions
{
    public static class IUmbracoBuilderExtensions
    {
        public static IUmbracoBuilder AddCustomContentFinders(this IUmbracoBuilder builder)
        {
            //builder.ContentFinders().InsertAfter<ContentFinderByUrl, ListingFiltersContentFinder>();
            //builder.SetContentLastChanceFinder<LastChanceContentFinder>();

            return builder;
        }

        public static IUmbracoBuilder AddCustomNotificationHandlers(this IUmbracoBuilder builder)
        {
            //ContentSavedNotificationHandler is to set default values on save. Currently used for news and event dates but could be used for setting 
            //listing summary text from the first portion of rte added through content blocks.
            //Or other things i.e. linking data
            //builder.AddNotificationHandler<ContentSavedNotification, ContentSavedNotificationHandler>();
            //builder.AddNotificationHandler<ContentCacheRefresherNotification, ContentCacheRefresherNotificationHandler>();
            //builder.AddNotificationHandler<UmbracoApplicationStartingNotification, UmbracoApplicationStartingNotificationHandler>();
            //builder.AddNotificationHandler<FormSavingNotification, FormSavingNotificationHandler>();
            //builder.AddNotificationHandler<MediaSavedNotification, MediaNotificationHandler>();
            //builder.AddNotificationHandler<ContentUnpublishedNotification, ContentUnPublishedNotificationHandler>();
            //builder.AddNotificationHandler<ContentMovedNotification, ContentMovedNotificationHandler>();
            //builder.AddNotificationHandler<ContentMovedToRecycleBinNotification, ContentRecyledNotificationHandler>();

            return builder;
        }


        public static IUmbracoBuilder AddCustomIndexOptions(this IUmbracoBuilder builder)
        {
            builder.Services.ConfigureOptions<ConfigureIndexOptions>();
            return builder;
        }

        public static IUmbracoBuilder AddCustomServices(this IUmbracoBuilder builder, IConfiguration configuration)
        {
            builder.RegisterCustomCachedServices(configuration);
            //builder.RegisterCustomServices();

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


        private static IUmbracoBuilder RegisterCustomServices(this IUmbracoBuilder builder)
        {
            //builder.Services.AddScoped<IEdAdminService, EdAdminService>();
            //builder.Services.AddScoped<IEncryptionService, EncryptionService>();
            //builder.Services.AddScoped<IUserSessionService, UserSessionService>();

            //builder.Services.AddScoped<IMultiLingualService, MultiLingualService>();
            //builder.Services.AddScoped<INavigationMenuBuilderService, MenuBuilderService>();
            //builder.Services.AddScoped<IRazorViewRenderService, RazorViewRenderService>();
            //builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();
            //builder.Services.AddScoped<ICacheTagHelperService, CacheTagHelperService>();

            //IProductImportService
            //IEmailNotificationService
            //builder.Services.AddMemoryCache();
            builder.Services.AddScoped<IOutputCacheService, OutputCacheService>();

            return builder;
        }

        public static IUmbracoBuilder AddCustomWorkFlows(this IUmbracoBuilder builder)
        {


            //builder.WithCollectionBuilder<WorkflowCollectionBuilder>()
            //    .Add<CampaignStarEmailWorkFlow>();

            return builder;
        }

    }

}
