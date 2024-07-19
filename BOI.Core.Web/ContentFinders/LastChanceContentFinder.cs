using BOI.Umbraco.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace BOI.Core.Web.ContentFinders
{
    public class LastChanceContentFinder : IContentLastChanceFinder
    {
        private readonly IDomainService domainService;
        private readonly IUmbracoContextAccessor umbracoContextAccessor;
        private readonly IPublishedSnapshotAccessor publishedSnapshotAccessor;

        public LastChanceContentFinder(IDomainService domainService, IUmbracoContextAccessor umbracoContextAccessor, IPublishedSnapshotAccessor publishedSnapshotAccessor)
        {
            this.domainService = domainService;
            this.umbracoContextAccessor = umbracoContextAccessor;
            this.publishedSnapshotAccessor = publishedSnapshotAccessor;
        }

        /// <summary>
        /// https://our.umbraco.com/documentation/reference/routing/request-pipeline/IContentFinder#notfoundhandlers
        /// </summary>
        public async Task<bool> TryFindContent(IPublishedRequestBuilder request)
        {
            // Find the root node with a matching domain to the incoming request
            var allDomains = domainService.GetAll(true).ToList();
            var domain = allDomains?
                .FirstOrDefault(f => f.DomainName == request.Uri.Authority
                    || f.DomainName == $"https://{request.Uri.Authority}"
                    || f.DomainName == $"http://{request.Uri.Authority}");

            var siteId = domain != null ? domain.RootContentId : allDomains.Any() ? allDomains.FirstOrDefault()?.RootContentId : null;

            if (!umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
            {
                return false;
            }
            var siteRoot = umbracoContext.Content.GetById(false, siteId ?? -1);

            if (siteRoot is null)
            {
                return false;
            }

            //TODO: add 404 page handler
            var notFoundNode = (siteRoot as SiteRoot).FirstChildOfType(Error.ModelTypeAlias);

            if (notFoundNode == null)
            {
                notFoundNode =
                    siteRoot.Children.FirstOrDefault(f => f.ContentType.Alias == Error.ModelTypeAlias
                        && f.Value<int>(Error.GetModelPropertyType(publishedSnapshotAccessor, e => e.StatusCode).Alias) == 404);
            }

            if (notFoundNode is not null)
            {
                request.SetPublishedContent(notFoundNode);
            }

            // Return true or false depending on whether our custom 404 page was found
            return request.PublishedContent is not null;
        }
    }
}
