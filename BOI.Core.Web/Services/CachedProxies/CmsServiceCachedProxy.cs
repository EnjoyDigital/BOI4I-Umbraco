using BOI.Umbraco.Models;
using DangEasy.Caching.MemoryCache;
using DangEasy.Interfaces.Caching;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using static Umbraco.Cms.Core.Constants.Conventions;

namespace BOI.Core.Web.Services.CachedProxies
{
    public class CmsServiceCachedProxy : ICmsService
    {
        private readonly ICache cache;
        private readonly ICmsService cmsService;

        public CmsServiceCachedProxy(ICache cache, CmsService cmsService)
        {
            this.cache = cache;
            this.cmsService = cmsService;
        }

        public SiteRoot GetSiteRoot(int currentNodeId)
        {
            var cacheKey = CacheKey.Build<CmsServiceCachedProxy, SiteRoot>(currentNodeId.ToString());

            return cache.Get(cacheKey, () => cmsService.GetSiteRoot(currentNodeId));
        }

        public SiteRoot GetSiteRoot(IPublishedContent content)
        {
            var cacheKey = CacheKey.Build<CmsServiceCachedProxy, SiteRoot>(content.Id.ToString());

            return cache.Get(cacheKey, () => cmsService.GetSiteRoot(content));
        }

        public IPublishedContent GetHome(IPublishedContent content)
        {
            var siteNode = GetSiteRoot(content);
            var cacheKey = CacheKey.Build<CmsServiceCachedProxy, IPublishedContent>(siteNode.Id.ToString());

            return cache.Get(cacheKey, () => cmsService.GetHome(content));
        }

        public Tuple<IPublishedContent, IDomain> GetNodeAndDomainForUrl(string fullUrlOfNode)
        {
            //var siteNode = GetSiteRoot(content);
            var cacheKey = CacheKey.Build<CmsServiceCachedProxy, Tuple<IPublishedContent, IDomain>>(fullUrlOfNode);

            return cache.Get(cacheKey, () => cmsService.GetNodeAndDomainForUrl(fullUrlOfNode));


        }
            public DataRepositories GetSiteDataRepositories(int nodeId)
        {
            var cacheKey = CacheKey.Build<CmsServiceCachedProxy, DataRepositories>(nodeId.ToString());

            return cache.Get(cacheKey, () => cmsService.GetSiteDataRepositories(nodeId));
        }
    }
}
