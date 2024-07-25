using BOI.Core.Search.Models;
using BOI.Core.Search.Services;
using BOI.Umbraco.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Extensions;

namespace BOI.Core.Web.Controllers.Backoffice.Api
{

    [PluginController("ElasticSearchPublish")]
    [UmbracoUserTimeoutFilter]
    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
    [IsBackOffice]

    public class ElasticSearchPublishApiController:  UmbracoAuthorizedApiController
    {
       
        private readonly IIndexingService indexingService;
        private readonly IUmbracoContextAccessor umbracoContextAccessor;

        public ElasticSearchPublishApiController(IIndexingService  indexingService,
            IUmbracoContextAccessor umbracoContextAccessor)
        {
            this.indexingService = indexingService;
            this.umbracoContextAccessor = umbracoContextAccessor;
        }

        public async Task<HttpResponseMessage> IndexContentItem(int id)
        {
            IUmbracoContext context = umbracoContextAccessor.GetRequiredUmbracoContext();
            var contentItem = context.Content?.GetById(id);
            if (contentItem != null)
            {
                var indexDoc = indexingService.DocBuilder(contentItem);
                indexingService.IndexWebContent(indexDoc);
            }

            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<HttpResponseMessage> IndexContentItemWithDescendants(int id)
        {
            IUmbracoContext context = umbracoContextAccessor.GetRequiredUmbracoContext();
            var contentItem = context.Content?.GetById(id);

            if (contentItem.ContentType.Alias == SiteContainer.ModelTypeAlias)
            {
                contentItem = contentItem.DescendantOfType(contentTypeAlias:SiteRoot.ModelTypeAlias);
            }

            var isSitecontent = contentItem.AncestorOrSelf(SiteRoot.ModelTypeAlias) != null;
            var isProduct = contentItem.AncestorOrSelf(ProductsContainer.ModelTypeAlias) != null;
            var isLTVTag = contentItem.AncestorOrSelf(ProductLtvcontainer.ModelTypeAlias) != null;
            //check valid for indexing
            if (!isSitecontent && !isProduct && !isLTVTag)
            {
                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK
                };
            }
            var contentItemList = new List<WebContent>();
            if(!contentItem.ContentType.Alias.Equals(SiteRoot.ModelTypeAlias) && !contentItem.ContentType.Alias.Equals(ProductsContainer.ModelTypeAlias) && !contentItem.ContentType.Alias.Equals(ProductLtvcontainer.ModelTypeAlias))
            {
                contentItemList.Add(indexingService.DocBuilder(contentItem));
            }

            var descendants = contentItem.Descendants().Select(indexingService.DocBuilder);
            contentItemList.AddRange(descendants);
            indexingService.IndexWebContent(contentItemList.ToArray());

            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK
            };
        }

    }
}
