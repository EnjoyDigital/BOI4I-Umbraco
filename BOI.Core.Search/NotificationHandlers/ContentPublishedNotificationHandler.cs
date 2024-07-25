using BOI.Core.Search.Constants;
using BOI.Core.Search.Models.ElasticSearch;
using BOI.Core.Search.Services;
using Microsoft.Extensions.Configuration;
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
        private readonly IIndexingService indexingService;
        private readonly IConfiguration configuration;

        public ContentPublishedNotificationHandler(ILogger<ContentPublishedNotificationHandler> logger, IUmbracoContextFactory umbracoContextFactory,
            IIndexingService indexingService, IConfiguration configuration)
        {
            this.logger = logger;
            this.umbracoContextFactory = umbracoContextFactory;
            this.indexingService = indexingService;
            this.configuration = configuration;
        }

        private ElasticSettings EsIndexes => new(configuration);


        public void Handle(ContentPublishedNotification notification)
        {
            logger.LogInformation("ContentServicePublished event called");
            var indexAlias = EsIndexes.WebContentEsIndexAlias;
            var indexExists = indexingService.IndexExists(indexAlias);

            foreach (var c in notification.PublishedEntities)
            {
                var content = GetPublishedContentFromCache(c.Id);
                if (content == null)
                {
                    continue;
                }
                if (!indexExists && c.ContentType.Alias.Equals(DocTypeConstants.Siteroot,StringComparison.InvariantCultureIgnoreCase))
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

                if (content.Ancestor(DocTypeConstants.Siteroot) == null && !content.ContentType.Alias.Equals(DocTypeConstants.Product, StringComparison.InvariantCultureIgnoreCase) && !content.ContentType.Alias.Equals(FieldConstants.ProductLTV, StringComparison.InvariantCultureIgnoreCase)) continue;

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
                case DocTypeConstants.Product:
                case DocTypeConstants.Error:
                case DocTypeConstants.MainSearchResults:
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
