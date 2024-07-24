using BOI.Core.Constants;
using BOI.Core.Services;
using BOI.Core.Web.Services;
using BOI.Core.Web.Services.CachedProxies;
using BOI.Umbraco.Models;
using DangEasy.Caching.MemoryCache;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common;
using Umbraco.Extensions;

namespace BOI.Core.Web.NotificationHandlers
{
    public class ContentCacheRefresherNotificationHandler : INotificationHandler<ContentCacheRefresherNotification>
    {
        private readonly IUmbracoHelperAccessor umbracoHelperAccessor;
        private readonly IServiceProvider serviceProvider;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IPublishedSnapshotAccessor publishedSnapshotAccessor;
        private readonly ILogger<ContentCacheRefresherNotificationHandler> logger;
        private readonly IOutputCacheService outputCacheService;
        private readonly ICacheTagHelperService cacheTagHelperService;

        public ContentCacheRefresherNotificationHandler(IUmbracoHelperAccessor umbracoHelperAccessor, IServiceProvider serviceProvider
            , IWebHostEnvironment webHostEnvironment, IPublishedSnapshotAccessor publishedSnapshotAccessor, ILogger<ContentCacheRefresherNotificationHandler> logger
            , IOutputCacheService outputCacheService, ICacheTagHelperService cacheTagHelperService)
        {
            this.umbracoHelperAccessor = umbracoHelperAccessor;
            this.serviceProvider = serviceProvider;
            this.webHostEnvironment = webHostEnvironment;
            this.publishedSnapshotAccessor = publishedSnapshotAccessor;
            this.logger = logger;
            this.outputCacheService = outputCacheService;
            this.cacheTagHelperService = cacheTagHelperService;
        }

        public async void Handle(ContentCacheRefresherNotification notification)
        {
            Cache.Instance.RemoveByPrefix(typeof(CmsServiceCachedProxy).ToString());
            Cache.Instance.RemoveByPrefix(typeof(SitemapXmlGeneratorCachedProxy).ToString());
            if (umbracoHelperAccessor.TryGetUmbracoHelper(out var umbracoHelper))
            {
                var messageObject = (ContentCacheRefresher.JsonPayload[])notification.MessageObject;

                foreach (var payload in messageObject)
                {
                    var nodeAlias = umbracoHelper.Content(payload.Id);
                    if (nodeAlias == null) continue;

                    if (nodeAlias.ContentType.Alias == Error.ModelTypeAlias && nodeAlias.Value<int>(Error.GetModelPropertyType(publishedSnapshotAccessor, m => m.StatusCode)?.Alias ?? "error") == 500)
                    {
                        await SaveStaticErrorPage(nodeAlias);
                    }
                }
            }

            try
            {
                if (outputCacheService.CacheEnabled())
                {
                    cacheTagHelperService.ClearCache();
                }

            }
            catch (Exception ex)
            {
                logger.LogError($"Error Refreshing Cache on cache refresh:{ex.Message}");
            }
        }

        private async Task SaveStaticErrorPage(IPublishedContent publishedContent)
        {
            using var scopeService = serviceProvider.CreateScope();
            var razorViewRenderService = scopeService.ServiceProvider.GetService<IRazorViewRenderService>();
            if (razorViewRenderService != null)
            {
                if (publishedContent != null)
                {
                    var tempData = scopeService.ServiceProvider.GetService<ISessionManager>();

                    tempData?.SetSessionValue(SessionConstants.PageId, Convert.ToString(publishedContent.Id));

                    var renderedPage =
                        await razorViewRenderService.RenderViewToStringAsync("/Views/Error.cshtml", publishedContent);

                    var host = new Uri(publishedContent.Url(null, UrlMode.Absolute)).Host;

                    await System.IO.File.WriteAllTextAsync(Path.Combine(webHostEnvironment.WebRootPath, $"500-{host}.html"),
                        renderedPage, System.Text.Encoding.UTF8);
                }
            }
        }
    }
}
