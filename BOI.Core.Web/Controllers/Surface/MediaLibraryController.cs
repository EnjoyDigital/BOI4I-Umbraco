using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;
using BOI.Umbraco.Models;
using Umbraco.Extensions;

namespace BOI.Core.Web.Controllers.Surface
{
    public class MediaLibraryController : SurfaceController
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public MediaLibraryController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider) 
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        [HttpGet]
        public ActionResult GetMedia(string mediaType, int listingNumber = 1)
        {
            if (_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
            {
                var helper = umbracoContext.Content;
                var siteRoot = helper?.GetSingleByXPath("//siteRoot");

                if (siteRoot != null)
                {
                    var mediaLibrary = siteRoot.Descendant<MediaLibrary>();

                    if (mediaLibrary != null)
                    {
                        switch (mediaType)
                        {
                            case "video":
                                var videos = mediaLibrary.Children<Video>()
                                    .OrderByDescending(o => o.ArticleDate)
                                    .Skip(listingNumber * 3)
                                    .Take(3);

                                return PartialView("~/Views/Partials/MediaLibrary/Videos.cshtml", videos);

                            case "podcast":
                                var podcasts = mediaLibrary.Children<Podcast>()
                                    .OrderByDescending(o => o.ArticleDate)
                                    .Skip(listingNumber * 3)
                                    .Take(3);

                                return PartialView("~/Views/Partials/MediaLibrary/Podcasts.cshtml", podcasts);

                            case "articles":
                                var articles = mediaLibrary.Children<Article>()
                                    .OrderByDescending(o => o.ArticleDate)
                                    .Skip(listingNumber * 3)
                                    .Take(3);

                                return PartialView("~/Views/Partials/MediaLibrary/Articles.cshtml", articles);

                            default:
                                var documents = mediaLibrary.Documents?.Skip(listingNumber * 3).Take(3);
                                return PartialView("~/Views/Partials/MediaLibrary/Documents.cshtml", documents);
                        }
                    }
                }
            }

            return new EmptyResult();
        }
    }
}