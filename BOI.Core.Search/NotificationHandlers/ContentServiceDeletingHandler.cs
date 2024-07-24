using BOI.Core.Search.Services;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace BOI.Core.Search.NotificationHandlers
{
    public class ContentServiceDeletingHandler : INotificationHandler<ContentMovingToRecycleBinNotification>
    {
        private readonly IUmbracoContextFactory umbracoContextFactory;
        private readonly IndexingService indexingService;
        private readonly IContentService contentService;
        private readonly IVariationContextAccessor variationContextAccessor;

        public ContentServiceDeletingHandler(IUmbracoContextFactory umbracoContextFactory, IndexingService indexingService,
            IContentService contentService, IVariationContextAccessor variationContextAccessor)
        {
            this.umbracoContextFactory = umbracoContextFactory;
            this.indexingService = indexingService;
            this.contentService = contentService;
            this.variationContextAccessor = variationContextAccessor;
        }

        public void Handle(ContentMovingToRecycleBinNotification notification)
        {
            foreach (var content in notification.MoveInfoCollection)
            {
                if (!content.Entity.Published) continue;
                UnpublishFromIndex(content.Entity.Id);
            }
        }

        private void UnpublishFromIndex(int id)
        {
            DeleteDoc(id);
        }

        private void DeleteDoc(int id)
        {
            indexingService.DeleteItemsFromIndex(id);
            //  var delete = esClient.Delete<WebContent>(Id, d => d.Index(config["WebContentEsIndexAlias"]));

            var content = GetPublishedContentFromCache(id);
            // int[] descendants =  new int[0];
            if (content != null)
            {//attempt to get from content service
                var descendants = content.Descendants(variationContextAccessor).Select(x => x.Id).ToArray();
                if (descendants.Any())
                {
                    indexingService.DeleteItemsFromIndex(descendants);
                }
            }
            else
            {
                var contentItem = contentService.GetById(id);
                var descendantsCount = contentService.CountDescendants(id);
                const int pageSize = 10000;
                var pageIndex = 0;
                var currentPageResultSize = 0;
                IContent[] descendantContent;
                do
                {
                    descendantContent = contentService.GetPagedDescendants(id, pageIndex, pageSize, out _).ToArray();
                    currentPageResultSize = descendantContent.Length;

                    if (descendantContent.Length > 0)
                    {
                        indexingService.DeleteItemsFromIndex(descendantContent.Select(x => x.Id).ToArray());
                    }

                } while (currentPageResultSize == pageSize);
            }
        }

        private IPublishedContent GetPublishedContentFromCache(int id)
        {
            using (var cref = umbracoContextFactory.EnsureUmbracoContext())
            {

                return cref.UmbracoContext.Content.GetById(id);
            }
        }       
    }
}
