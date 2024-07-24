using BOI.Umbraco.Models;
using Microsoft.AspNetCore.Http;
using System.Runtime.Caching;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace BOI.Core.Web.ContentFinders
{
    public class ListingFiltersContentFinder : IContentFinder
    {
        public const string RequestItemTagIdKey = "listingTagId_cache";
        private readonly IUmbracoContextAccessor umbracoContextAccessor;
        private readonly IHttpContextAccessor httpContextAccessor;
        private static readonly MemoryCache FormCache = new MemoryCache("FacetedUrlCache");
        private static readonly CacheItemPolicy Policy = new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(86400), Priority = CacheItemPriority.Default };


        public ListingFiltersContentFinder(IUmbracoContextAccessor umbracoContextAccessor, IHttpContextAccessor httpContextAccessor)
        {
            this.umbracoContextAccessor = umbracoContextAccessor;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> TryFindContent(IPublishedRequestBuilder request)
        {

            if (request != null && umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext) && umbracoContext != null)
            {
                var path = request.Uri.AbsolutePath;
                var segments = request.Uri.Segments;

                if (segments.Count() > 5 || path.Contains("umbraco"))
                {
                    return false;
                }

                var tagId = FormCache.Get(path) as PageTagIdCache;
                if (tagId != null)
                {
                    var content = umbracoContext.Content.GetById(tagId.PageId);
                    var tag = umbracoContext.Content.GetById(tagId.TagId);
                    if (content != null && tag != null)
                    {
                        httpContextAccessor.HttpContext.Items.Add(RequestItemTagIdKey, tag.Id);
                        request.SetPublishedContent(content);
                        return true;
                    }
                    FormCache.Remove(path);
                }

                if (request.Domain?.ContentId < 1)
                {
                    return false;
                }

                var siteRoot = umbracoContext.Content.GetById(request.Domain.ContentId) as SiteRoot;
                if (siteRoot == null)
                {
                    return false;
                }
                var sharedAssets = siteRoot.DescendantOfType(DataRepositories.ModelTypeAlias);

                if (siteRoot == null || sharedAssets == null)
                {
                    return false;
                }

                //var newsListingPages = siteRoot.DescendantsOfType(NewsListing.ModelTypeAlias);
                //if (newsListingPages.Any(x => path.StartsWith(x.Url())))
                //{
                //    var listingPage = newsListingPages.FirstOrDefault(x => path.StartsWith(x.Url()));
                //    var newsTags = sharedAssets.DescendantsOfType(NewsTag.ModelTypeAlias);
                //    var newsTag = newsTags.FirstOrDefault(a => path.EndsWith($"/{a.UrlSegment}"));
                //    if (newsTag != null)
                //    {
                //        //Set the news tag in the request for the controller
                //        httpContextAccessor.HttpContext.Items.Add(RequestItemTagIdKey, newsTag.Id);
                //        request.SetPublishedContent(listingPage);
                //        FormCache.Add(new CacheItem(path, listingPage.Id), Policy);
                //        return true;
                //    }
                //}

                //var offersListingPages = siteRoot.DescendantsOfType(OfferListing.ModelTypeAlias);
                //if (offersListingPages.Any(x => path.StartsWith(x.Url())))
                //{
                //    var listingPage = offersListingPages.FirstOrDefault(x => path.StartsWith(x.Url()));
                //    var offerTags = sharedAssets.DescendantsOfType(OfferTag.ModelTypeAlias);
                //    var offerTag = offerTags.FirstOrDefault(a => path.EndsWith($"/{a.UrlSegment}"));
                //    if (offerTag != null)
                //    {
                //        //Set the news tag in the request for the controller
                //        httpContextAccessor.HttpContext.Items.Add(RequestItemTagIdKey, offerTag.Id);
                //        request.SetPublishedContent(listingPage);
                //        FormCache.Add(new CacheItem(path, listingPage.Id), Policy);
                //        return true;
                //    }
                //}

                //var recipeListingPages = siteRoot.DescendantsOfType(RecipeListing.ModelTypeAlias);
                //if (recipeListingPages.Any(x => path.StartsWith(x.Url())))
                //{
                //    var listingPage = recipeListingPages.FirstOrDefault(x => path.StartsWith(x.Url()));
                //    var recipeMainIngredientTags = (sharedAssets.DescendantsOfType(MainIngredientTag.ModelTypeAlias) ?? Enumerable.Empty<IPublishedContent>()).ToList();
                //    var recipeDishTypeTags = sharedAssets.DescendantsOfType(DishTypeTag.ModelTypeAlias).ToList();
                //    var recipeOccasionTags = sharedAssets.DescendantsOfType(OccasionTag.ModelTypeAlias).ToList();
                //    var recipeCuisineTags = sharedAssets.DescendantsOfType(CuisineTag.ModelTypeAlias).ToList();
                //    var recipeDietTags = sharedAssets.DescendantsOfType(DietTag.ModelTypeAlias).ToList();

                //    if (recipeDishTypeTags.NotNullAndAny())
                //    {
                //        recipeMainIngredientTags.AddRange(recipeDishTypeTags);
                //    }
                //    if (recipeOccasionTags.NotNullAndAny())
                //    {
                //        recipeMainIngredientTags.AddRange(recipeOccasionTags);
                //    }
                //    if (recipeCuisineTags.NotNullAndAny())
                //    {
                //        recipeMainIngredientTags.AddRange(recipeCuisineTags);
                //    }
                //    if (recipeDietTags.NotNullAndAny())
                //    {
                //        recipeMainIngredientTags.AddRange(recipeDietTags);
                //    }


                //    var recipeTag = recipeMainIngredientTags.FirstOrDefault(a => path.EndsWith($"/{a.UrlSegment}"));
                //    if (recipeTag != null)
                //    {
                //        //Set the news tag in the request for the controller
                //        httpContextAccessor.HttpContext.Items.Add(RequestItemTagIdKey, recipeTag.Id);
                //        request.SetPublishedContent(listingPage);
                //        FormCache.Add(new CacheItem(path, listingPage.Id), Policy);
                //        return true;
                //    }
                //}

                //var careerListingPages = siteRoot.DescendantsOfType(Careers.ModelTypeAlias);
                //if (careerListingPages.Any(x => path.StartsWith(x.Url())))
                //{
                //    var listingPage = careerListingPages.FirstOrDefault(x => path.StartsWith(x.Url()));
                //    var jobTags = sharedAssets.DescendantsOfType(JobTag.ModelTypeAlias).ToList();
                //    var jobTag = jobTags.FirstOrDefault(a => path.EndsWith($"/{a.UrlSegment}"));
                //    if (jobTag != null)
                //    {
                //        //Set the news tag in the request for the controller
                //        httpContextAccessor.HttpContext.Items.Add(RequestItemTagIdKey, jobTag.Id);
                //        request.SetPublishedContent(listingPage);
                //        FormCache.Add(new CacheItem(path, listingPage.Id), Policy);
                //        return true;
                //    }
                //}


            }

            return false;
        }
    }

    public class PageTagIdCache
    {
        public int PageId { get; set; }
        public int TagId { get; set; }
    }
}
