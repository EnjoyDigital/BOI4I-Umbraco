using BOI.Umbraco.Models;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace BOI.Core.Web.Services
{
    public interface ICmsService
    {
        SiteRoot GetSiteRoot(int currentNodeId);

        SiteRoot GetSiteRoot(IPublishedContent content);

        IPublishedContent GetHome(IPublishedContent content);

        DataRepositories GetSiteDataRepositories(int currentNodeId);
    }

    public class CmsService : ICmsService
    {
        private readonly ILogger<CmsService> logger;
        private readonly IUmbracoContextFactory umbracoContextFactory;

        public CmsService(IUmbracoContextFactory umbracoContextFactory, ILogger<CmsService> logger)
        {
            this.logger = logger;
            this.umbracoContextFactory = umbracoContextFactory;
        }

        public SiteRoot GetSiteRoot(IPublishedContent content)
        {
            var siteNode = content.AncestorOrSelf<SiteRoot>();

            if (siteNode == null)
            {
                logger.LogWarning("siteNode is null");
            }

            return siteNode;
        }

        public SiteRoot GetSiteRoot(int currentNodeId)
        {
            IPublishedContent node;
            using (UmbracoContextReference umbContextRef = umbracoContextFactory.EnsureUmbracoContext())
            {
                node = umbContextRef.UmbracoContext.Content.GetById(currentNodeId);
            }

            if (node == null)
            {
                logger.LogWarning("Node with id {CurrentNodeId} is null", currentNodeId);
                return null;
            }

            var siteNode = node.AncestorOrSelf<SiteRoot>();

            if (siteNode == null)
            {
                logger.LogWarning("siteNode is null");
            }

            return siteNode;
        }

        public IPublishedContent GetHome(IPublishedContent content)
        {
            using (var umbContextRef = umbracoContextFactory.EnsureUmbracoContext())
            {
                var siteNode = GetSiteRoot(content);
                var siteRootRedirectUdi = siteNode.Value<Udi>("umbracoInternalRedirectId");
                return umbContextRef.UmbracoContext.Content.GetById(siteRootRedirectUdi);
            }
        }

        public DataRepositories GetSiteDataRepositories(int currentNodeId)
        {
            using (var umbContextRef = umbracoContextFactory.EnsureUmbracoContext())
            {
                var node = umbContextRef.UmbracoContext.Content.GetById(currentNodeId);

                if (node == null)
                {
                    logger.LogWarning($"1.Node with id {currentNodeId} is null");
                    return null;
                }

                var dataRepositories = node.Ancestor<SiteRoot>().FirstChild<DataRepositories>();

                if (dataRepositories == null)
                {
                    logger.LogWarning("dataRepositories is null");
                }

                return dataRepositories;
            }
        }
    }
}
