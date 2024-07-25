using BOI.Core.Web.Models.ViewModels;
using BOI.Umbraco.Models;
using BOI.Core.Extensions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace BOI.Core.Web.Services
{
    public interface ISitemapXmlGenerator
    {
        List<SitemapXmlItem> GetSitemap(int nodeId, string baseUrl);
    }

    public class SitemapXmlGenerator : ISitemapXmlGenerator
    {
        private readonly ICmsService cmsService;
        private readonly IUmbracoContextFactory umbracoContextFactory;
        private readonly IUmbracoContextAccessor umbracoContextAccessor;

        public SitemapXmlGenerator(ICmsService cmsService, IUmbracoContextFactory umbracoContextFactory, IUmbracoContextAccessor umbracoContextAccessor)
        {
            this.cmsService = cmsService;
            this.umbracoContextFactory = umbracoContextFactory;
            this.umbracoContextAccessor = umbracoContextAccessor;
        }

        public List<SitemapXmlItem> GetSitemap(int nodeId, string baseUrl)
        {
            using (var umbContextRef = umbracoContextFactory.EnsureUmbracoContext())
            {
                var sitemapItems = new List<SitemapXmlItem>();
                var home = umbContextRef.UmbracoContext.Content.GetById(nodeId);
                var siteRoot = home.AncestorOrSelf<SiteRoot>();

                sitemapItems.Add(CreateSitemapItem(baseUrl, home, defaultPath: "/"));
                sitemapItems = ProcessSitemapItems(baseUrl, sitemapItems, siteRoot.Children().Where(x => x.Id != home.Id));

                return sitemapItems;
            }
        }

        private List<SitemapXmlItem> ProcessSitemapItems(string baseUrl, List<SitemapXmlItem> sitemapItems, IEnumerable<IPublishedContent> nodes)
        {
            foreach (var node in nodes)
            {
                if (CanProcessNode(node))
                {
                    sitemapItems.Add(CreateSitemapItem(baseUrl, node));
                    if (node.Children.NotNullAndAny())
                    {
                        sitemapItems = ProcessSitemapItems(baseUrl, sitemapItems, node.Children);
                    }
                }
            }

            return sitemapItems;
        }

        private SitemapXmlItem CreateSitemapItem(string baseUrl, IPublishedContent node, string defaultPath = null)
        {
            using (var umbContextRef = umbracoContextFactory.EnsureUmbracoContext())
            {
                var item = new SitemapXmlItem
                {
                    ChangeFreq = GetValueOrDefault(node, "seoFrequency", "monthly"),
                    Priority = GetValueOrDefault(node, "seoPriority", "0.5"),
                    Url = string.Concat(baseUrl, defaultPath ?? node.Url(mode: UrlMode.Relative)),
                    LastModified = string.Format("{0:s}+00:00", node.UpdateDate),
                };

                //var hrefs = node.Value<IEnumerable<HreflangAttributes>>("hreflangTags");
                //if (hrefs.NotNullAndAny())
                //{
                //    item.Alternates = hrefs.Select(x => new HrefLangs { Href = x.Href, IsoCode = x.Hreflang });

                //}

                return item;
            }
        }


        private string GetValueOrDefault(IPublishedContent node, string alias, string defaultValue)
        {
            var settings = node.Value<IEnumerable<IPublishedElement>>("pageSettings");

            string value;
            if (settings != null)
            {
                //TODO:Resolve
                var element = settings.FirstOrDefault(x => x.ContentType.Alias == ElementSeoSettings.ModelTypeAlias);


                if (element == null)
                {
                    return defaultValue;
                }

                value = element.Value<string>(alias);
                if (string.IsNullOrEmpty(value))
                {
                    value = defaultValue;
                }
            }
            else
            {
                value = defaultValue;
            }

            return value;
        }

        private bool CanProcessNode(IPublishedContent page)
        {
            var item = page as IPageSettingsMixin;

            if (page.ContentType.Alias == Error.ModelTypeAlias)
            {
                return false;
            }

            if (item != null)
            {
                var hide = item.Value<bool>(ElementSeoSettings.ModelTypeAlias, "hideInSiteMap");
                if (!hide)
                {
                    return true;
                }
            }
            else if (item == null && page.TemplateId != 0)
            {
                return true;
            }

            return false;
        }
    }
}
