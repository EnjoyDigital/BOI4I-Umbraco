using BOI.Core.Search.Services;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Web;

using Umbraco.Extensions;

namespace BOI.Core.Search.NotificationHandlers
{

    public class ContentPublishedNotificationHandler : INotificationHandler<ContentPublishedNotification>
    {
        private readonly ILogger<ContentPublishedNotificationHandler> logger;
        private readonly IUmbracoContextFactory umbracoContextFactory;
        private readonly IndexingService indexingService;

        public ContentPublishedNotificationHandler(ILogger<ContentPublishedNotificationHandler> logger, IUmbracoContextFactory umbracoContextFactory,
            IndexingService indexingService)
        {
            this.logger = logger;
            this.umbracoContextFactory = umbracoContextFactory;
            this.indexingService = indexingService;
        }

        public void Handle(ContentPublishedNotification notification)
        {
            logger.LogInformation("ContentServicePublished event");
            var indexAlias = indexingService.WebContentIndexAlias;
            var indexExists = indexingService.IndexExists(indexAlias);

            foreach (var c in notification.PublishedEntities)
            {
                var content = GetPublishedContentFromCache(c.Id);
                if (content == null)
                {
                    continue;
                }
                if (!indexExists && c.ContentType.Alias == "siteRoot")
                {
                    var results = new List<IPublishedContent>();

                    foreach (var child in content.Children)
                    {
                        if (TemplateCheck(child.ContentType.Alias)) continue;

                        results.Add(child);

                        if (child.Children.Any())
                        {
                            results.AddRange(GetDescendents(child));
                        }
                    }

                    var documents = results.Select(indexingService.DocBuilder).ToList();

                    //TODO: interface the following and use DI
                    //As is, object is newed up on every loop
                    //  var indexingService = new IndexingService(config, esClient);

                    indexingService.ReIndexWebContent(documents.WhereNotNull());

                    continue;
                }

                if (content.Ancestor("siteRoot") == null && !content.ContentType.Alias.Equals("product", StringComparison.InvariantCultureIgnoreCase) && !content.ContentType.Alias.Equals("productLTV", StringComparison.InvariantCultureIgnoreCase)) continue;

                logger.LogInformation("ContentServicePublished Indexing service called to build doc");
                var doc = indexingService.DocBuilder(content);
                logger.LogInformation( "ContentServicePublished Indexing service called to index doc");
                indexingService.IndexWebContent(doc);

            }
        }

        private IEnumerable<IPublishedContent> GetDescendents(IPublishedContent content)
        {
            var results = new List<IPublishedContent>();

            foreach (var child in content.Children)
            {
                if (TemplateCheck(child.ContentType.Alias)) continue;

                results.Add(child);
                if (child.Children.Any())
                {
                    results.AddRange(GetDescendents(child));
                }

            }

            return results;
        }

        private bool TemplateCheck(string alias)
        {
            //TODO: case statements can be combined
            switch (alias)
            {
                case "product":
                case "error":
                case "mainSearchResults":
                    return true;

            }

            return false;
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
