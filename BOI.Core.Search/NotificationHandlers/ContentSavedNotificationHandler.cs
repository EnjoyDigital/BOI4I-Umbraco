using Newtonsoft.Json;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace BOI.Core.Search.NotificationHandlers
{
    public class ContentSavedNotificationHandler : INotificationHandler<ContentSavedNotification>
    {
        public void Handle(ContentSavedNotification notification)
        {
            foreach (var content in notification.SavedEntities)
            {
                if (content.HasProperty("productVariant"))
                {
                    if (content.GetValue<string>("productVariant").IsNullOrWhiteSpace())
                    {
                        content.SetValue("productVariant", JsonConvert.SerializeObject(new[] { "NC" }));
                    }
                    else
                    {
                        content.SetValue("productVariant", content.GetValue<string>("productVariant"));
                    }
                }
            }

        }
    }
}
