using BOI.Core.Services;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Forms.Core.Cache;

namespace BOI.Core.Web.NotificationHandlers
{
    public class FormCacheRefresherNotificationHandler : INotificationHandler<FormCacheRefresherNotification>
    {

        private readonly ILogger<MediaCacheRefresherNotificationHandler> logger;
        private readonly IOutputCacheService outputCacheService;
        private readonly ICacheTagHelperService cacheTagHelperService;

        public FormCacheRefresherNotificationHandler(ILogger<MediaCacheRefresherNotificationHandler> logger, IOutputCacheService outputCacheService, ICacheTagHelperService cacheTagHelperService)
        {
            this.logger = logger;
            this.outputCacheService = outputCacheService;
            this.cacheTagHelperService = cacheTagHelperService;
        }

        public void Handle(FormCacheRefresherNotification notification)
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
                logger.LogError($"Error Refreshing Cache on Form cache refresh:{ex.Message}");
            }
        }
    }
}
