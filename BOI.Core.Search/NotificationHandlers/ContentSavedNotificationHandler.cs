using BOI.Core.Search.Constants;
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
                if (content.HasProperty(FieldConstants.ProductVariant))
                {
                    if (content.GetValue<string>(FieldConstants.ProductVariant).IsNullOrWhiteSpace())
                    {
                        content.SetValue(FieldConstants.ProductVariant, JsonConvert.SerializeObject(new[] { "NC" }));
                    }
                    else
                    {
                        content.SetValue(FieldConstants.ProductVariant, content.GetValue<string>(FieldConstants.ProductVariant));
                    }
                }
            }

        }
    }
}
