using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using System.Xml;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common;
using Umbraco.Extensions;
using BOI.Umbraco.Models;
using Skybrud.Umbraco.Redirects.Services;
using Newtonsoft.Json.Serialization;
using BOI.Core.Web.Extensions;
using BOI.Core.Extensions;
using BOI.Core.Web.Models.Dtos.EdAdmin;



namespace BOI.Core.Web.Services
{
    public interface IEdAdminService
    {
        byte[] ExportMetaData();
        ImportResponse ImportHrefLang(string path);
        ImportResponse ImportMetaData(string path);
        byte[] ExportMembers();
    }

    //TODO:HrefLangSettings cms model resolve
    public class EdAdminService : IEdAdminService
    {
        private readonly IContentService contentService;
        private readonly ICmsService cmsService;
        private readonly IMemberService memberService;
        private readonly IRedirectsService redirectsService;
        private readonly IUmbracoContextAccessor umbracoContextAccessor;
        private readonly ILogger<EdAdminService> logger;
        private readonly IUmbracoHelperAccessor umbracoHelperAccessor;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;

        public EdAdminService(IContentService contentService, ICmsService cmsService, IMemberService memberService,
            IRedirectsService redirectsService, IUmbracoContextAccessor umbracoContextAccessor, ILogger<EdAdminService> logger, IUmbracoHelperAccessor umbracoHelperAccessor,
            IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            this.contentService = contentService;
            this.cmsService = cmsService;
            this.memberService = memberService;
            this.redirectsService = redirectsService;
            this.umbracoContextAccessor = umbracoContextAccessor;
            this.logger = logger;
            this.umbracoHelperAccessor = umbracoHelperAccessor;
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
        }

        public ImportResponse ImportHrefLang(string path)
        {
            string hreflangPropertyAlias = "hreflangTags";
            var response = new ImportResponse();
            var hreflangs = new List<HrefLangMap>();
            using (var stream = System.IO.File.OpenText(path))
            {
                var errors = new StringBuilder();
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(stream);
                var manager = new XmlNamespaceManager(xmlDoc.NameTable);
                if (!manager.HasNamespace("xhtml"))
                {
                    manager.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");
                }
                if (xmlDoc.DocumentElement == null || !xmlDoc.DocumentElement.Name.InvariantEquals("urlset"))
                {
                    errors.AppendLine("No urlset found.");
                    response.Errors = errors.ToString();
                    return response;
                }
                var urlSets = xmlDoc.DocumentElement.ChildNodes;
                foreach (XmlNode url in urlSets)
                {
                    string loc;
                    if (url["loc"] != null)
                    {
                        loc = url["loc"].InnerText;
                    }
                    else
                    {
                        continue;
                    }

                    var alternates = url.ChildNodes.Cast<XmlNode>().Where(x => x.LocalName.InvariantEquals("link")).ToList();
                    if (alternates.Any())
                    {
                        var map = new HrefLangMap { Loc = new Uri(loc) };
                        foreach (var alternate in alternates)
                        {
                            if (alternate.Attributes == null) continue;
                            var locale = alternate.Attributes["hreflang"].Value;

                            map.Hrefs.Add(new HrefLangModel
                            {
                                Hreflang = locale,
                                Href = new Uri(alternate.Attributes["href"].Value)
                            });
                        }
                        if (map.Hrefs.Any())
                        {
                            hreflangs.Add(map);
                        }
                    }
                }
            }

            if (!hreflangs.Any())
            {
                response.Errors += "No hreflangs found.";
                return response;
            }

            
            foreach (var hreflang in hreflangs)
            {
                var nodeDomain = cmsService.GetNodeAndDomainForUrl(hreflang.Loc.AbsoluteUri);
                if (nodeDomain == null || nodeDomain.Item1 == null) 
                {
                    continue;
                }
                var publishedContent = nodeDomain.Item1;

                if (publishedContent.ContentType.Alias.InvariantEquals(SiteRoot.ModelTypeAlias))
                {
                    publishedContent = publishedContent.FirstChild(Home.ModelTypeAlias);
                }

                if (!publishedContent.HasProperty(hreflangPropertyAlias)) continue;

                var content = contentService.GetById(publishedContent.Id);

                var value =
                    JsonConvert.SerializeObject(
                        hreflang.Hrefs.Where(a => !a.Hreflang.InvariantEquals(publishedContent.GetCultureFromDomains().ToString().ToLower())),
                        Newtonsoft.Json.Formatting.Indented,
                        new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }
                    );

                content.GetValue(hreflangPropertyAlias, value);

                contentService.SaveAndPublish(content);
            }

            return response;
        }

        public ImportResponse ImportMetaData(string path)
        {
            string metaTitleAlias = "metaTitle";
            string metaDescriptionAlias = "metaDescription";

            var response = new ImportResponse();
            List<string> errors = new List<string>();
            var metaData = GetDataFromCsv<PageMetaDataModel>(path, out errors);

            if (errors.NotNullAndAny())
            {
                response.Errors = string.Join(",   ",errors);
                return response;
            }

          
            foreach (var meta in metaData)
            {
                var nodeDomain = cmsService.GetNodeAndDomainForUrl(meta.Url);
                if (nodeDomain == null || nodeDomain.Item1 == null)
                {
                    continue;
                }
                var publishedContent = nodeDomain.Item1;

                if (publishedContent.ContentType.Alias.InvariantEquals(SiteRoot.ModelTypeAlias))
                {
                    publishedContent = publishedContent.FirstChild(Home.ModelTypeAlias);
                }

                if (publishedContent == null || publishedContent.As<IPageSettingsMixin>() == null) continue;


                if (contentService.GetById(publishedContent.Id) is IContent content && content.HasProperty("pageSettings"))
                {
                    var pageSettings = new List<Dictionary<string, object>>();

                    var pageSettingsValue = content.GetValue<string>("pageSettings");
                    if (pageSettingsValue.HasValue())
                    {
                        pageSettings = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(pageSettingsValue);
                    }

                    if (pageSettings?.FirstOrDefault(i => i.ContainsValue(ElementSeoSettings.ModelTypeAlias)) is Dictionary<string, object> elementSeoSettings)
                    {
                        elementSeoSettings[metaTitleAlias] = meta.MetaTitle;
                        elementSeoSettings[metaDescriptionAlias] = meta.MetaDescription;
                    }
                    else
                    {
                        if(pageSettings == null)
                        {
                            pageSettings = new List<Dictionary<string, object>>();
                        }
                        pageSettings.Add(
                            new Dictionary<string, object>
                            {
                                { "key", Guid.NewGuid() },
                                { "name", "SEO" },
                                { "ncContentTypeAlias", ElementSeoSettings.ModelTypeAlias },
                                { "metaTitle", meta.MetaTitle },
                                { "metaDescription", meta.MetaDescription },
                                { "canonicalURL", null },
                                { "seoFrequency", null },
                                { "seoPriority", null },
                                { "hideFromSitemapXml", 0 },
                                { "hideFromSitemapPage", 0 },
                            }
                        );
                    }

                    content.SetValue("pageSettings", JsonConvert.SerializeObject(pageSettings));

                    contentService.SaveAndPublish(content);
                }
            }

            return response;
        }

        public byte[] ExportMetaData()
        {
            var metaData = new List<PageMetaDataModel>();
            UmbracoHelper umbracoHelper;
            if(!umbracoHelperAccessor.TryGetUmbracoHelper(out umbracoHelper))
            {
                throw new Exception("Umbraco not found");
            }
            var rootNodes =umbracoHelper
                            .ContentAtRoot()
                            .Select(c => c.Descendant<SiteRoot>())
                            .Select(r => r.Children<IPageSettingsMixin>().Where(c => c.TemplateId != 0));

            foreach (var siteRoot in rootNodes)
            {
                foreach (var node in siteRoot)
                {
                    var seoSettings = node.PageSettings.GetElement<ElementSeoSettings>();
                    if (seoSettings != null)
                    {
                        metaData.Add(
                            new PageMetaDataModel
                            {
                                Url = (node.ContentType.Alias.InvariantEquals(Home.ModelTypeAlias) ? node.Parent : node).Url(mode: UrlMode.Absolute),
                                MetaTitle = seoSettings.MetaTitle,
                                MetaDescription = seoSettings.MetaDescription
                            }
                        );
                    }
                }
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream, Encoding.GetEncoding("iso-8859-1")))
                {
                    using (var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(metaData);
                    }
                    return memoryStream.ToArray();
                }
            }
        }

        private List<T> GetDataFromCsv<T>(string path, out List<string> errors)
        {
            var errorsBuilder = new List<string>();
            var csvConfig =
                new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    TrimOptions = TrimOptions.Trim | TrimOptions.InsideQuotes,
                    Encoding = Encoding.Unicode,
                    BadDataFound = context => { errorsBuilder.Add($"Bad data found on row '{context.RawRecord}'"); }
                };

            using (var stream = System.IO.File.OpenText(path))
            using (var csvReader = new CsvReader(stream, csvConfig))
            {
                try
                {
                    errors = new List<string>(errorsBuilder);

                    return csvReader.GetRecords<T>().ToList();
                }
                catch (CsvHelperException ex)
                {
                    errorsBuilder.Add($"Error with file format on line - {ex.Context.Reader.Parser.Row}");

                    errors = new List<string>(errorsBuilder);
                }

                return null;
            }
        }

        public byte[] ExportMembers()
        {
            var members =
                memberService.GetMembersByMemberType("member")
                .Select(member =>
                    new
                    {
                        Username = member.Email,
                        Email = member.Email,
                        Name = member.Name,
                        Roles = memberService.GetAllRoles(member.Username).ToList(),
                        DateCreated = member.CreateDate

                    }
                )
                .ToList();

            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream, Encoding.GetEncoding("iso-8859-1")))
                {
                    using (var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(members);
                    }

                    return memoryStream.ToArray();
                }
            }
        }

        private bool IsHrefAlternateValid(XmlNode hrefAlternate)
        {
            if (hrefAlternate.Attributes == null)
            {
                return false;
            }
            else
            {
                foreach (XmlAttribute att in hrefAlternate.Attributes)
                {
                    if (string.IsNullOrWhiteSpace(att.Value))
                    {
                        return false;
                    }
                }

                try
                {
                    var uri = new Uri(hrefAlternate.Attributes["href"].Value);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

    }
}