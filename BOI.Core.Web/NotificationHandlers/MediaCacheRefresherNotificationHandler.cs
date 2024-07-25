using BOI.Core.Services;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace BOI.Core.Web.NotificationHandlers
{
    public class MediaCacheRefresherNotificationHandler : INotificationHandler<MediaCacheRefresherNotification>
    {

        private readonly ILogger<MediaCacheRefresherNotificationHandler> logger;
        private readonly IOutputCacheService outputCacheService;
        private readonly ICacheTagHelperService cacheTagHelperService;

        public MediaCacheRefresherNotificationHandler(ILogger<MediaCacheRefresherNotificationHandler> logger, IOutputCacheService outputCacheService, ICacheTagHelperService cacheTagHelperService)
        {
            this.logger = logger;
            this.outputCacheService = outputCacheService;
            this.cacheTagHelperService = cacheTagHelperService;
        }

        public void Handle(MediaCacheRefresherNotification notification)
        {
            try
            {
                if (outputCacheService.CacheEnabled())
                {
                    cacheTagHelperService.ClearCache();
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error Refreshing Cache on Media Refresh:{ex.Message}");
            }
        }
    }
}
