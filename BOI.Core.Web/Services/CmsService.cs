using BOI.Umbraco.Models;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace BOI.Core.Web.Services
{
    public interface ICmsService
    {
        SiteRoot GetSiteRoot(int currentNodeId);

        SiteRoot GetSiteRoot(IPublishedContent content);

        IPublishedContent GetHome(IPublishedContent content);
        Tuple<IPublishedContent, IDomain> GetNodeAndDomainForUrl(string fullUrlOfNode);


        DataRepositories GetSiteDataRepositories(int currentNodeId);
    }

    public class CmsService : ICmsService
    {
        private readonly ILogger<CmsService> logger;
        private readonly IDomainService domainService;
        private readonly IUmbracoContextFactory umbracoContextFactory;

        public CmsService(IUmbracoContextFactory umbracoContextFactory, ILogger<CmsService> logger, IDomainService domainService)
        {
            this.logger = logger;
            this.domainService = domainService;
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

        public Tuple<IPublishedContent, IDomain> GetNodeAndDomainForUrl(string fullUrlOfNode)
        {
            using (UmbracoContextReference umbContextRef = umbracoContextFactory.EnsureUmbracoContext())
            {
                var allDomains = domainService.GetAll(false);
                string domainName = allDomains.OrderByDescending(x => x.DomainName.Length).FirstOrDefault(d => fullUrlOfNode.Contains(d.DomainName)).DomainName;
                var domain = allDomains.FirstOrDefault(d => d.DomainName == domainName);
                if (domain == null)
                {
                    logger.LogInformation("Domain not found for {ullUrlOfNode}", fullUrlOfNode);
                    logger.LogInformation("Domain list :{Domains}", string.Join(",", allDomains.Select(x => x.DomainName)));
                    return null;
                }
                else
                {
                    logger.LogInformation("Domain resolved as {DomainName} for {FullUrlOfNode}", domain.DomainName, fullUrlOfNode);
                }

                try
                {
                    var fullPathUri = new Uri(fullUrlOfNode);
                    var isoPath = string.Concat("/", domain.LanguageIsoCode.ToLower(), "/");
                    var pathToNode = string.Concat(domain.RootContentId, "/", fullPathUri.AbsolutePath.Replace(isoPath, ""));

                    var node = umbContextRef.UmbracoContext.Content.GetByRoute(false, pathToNode, culture: domain.LanguageIsoCode);

                    return new Tuple<IPublishedContent, IDomain>(node, domain);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error retreiving node and domain for {FullUrlOfNode}", fullUrlOfNode);

                    return null;
                }

            }
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
