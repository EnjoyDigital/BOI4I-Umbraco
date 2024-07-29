using BOI.Core.Extensions;
using BOI.Core.Web.Constants;
using BOI.Core.Web.Models.Dtos;
using BOI.Umbraco.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Extensions;

namespace BOI.Core.Web.Controllers.Backoffice
{
    [PluginController("Product")]
    [UmbracoUserTimeoutFilter]
    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
    [IsBackOffice]
    public class ProductApiController : UmbracoAuthorizedApiController
    {
        private static readonly IEnumerable<string> AllowedExtension = new List<string>() { ".csv", ".json" };
        private static string FileName { get; set; }
        private static string FileUploadPath = "App_Data\\TEMP\\CSVUploads";
        private static readonly Regex CsvRegex = new Regex(@"(?x)\s*,\s*(?=(?:[^""]*""[^""]*"")*[^""]*$)");

        private readonly IContentService contentService;
        private readonly ILogger<ProductApiController> logger;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IUmbracoHelperAccessor umbracoHelperAccessor;

        public ProductApiController(IContentService contentService, ILogger<ProductApiController> logger,
            IWebHostEnvironment webHostEnvironment, IUmbracoHelperAccessor umbracoHelperAccessor)
        {
            this.contentService = contentService;
            this.logger = logger;
            this.webHostEnvironment = webHostEnvironment;
            this.umbracoHelperAccessor = umbracoHelperAccessor;
        }


        private async Task<SaveResponseDto> SaveFile(IFormFile file)
        {
            try
            {
                var uploadFolder = Path.Combine(webHostEnvironment.ContentRootPath, FileUploadPath);
                FileName = file.FileName;
                var filePath = Path.Combine(uploadFolder, file.FileName);

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return new SaveResponseDto { FilePath = filePath };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving file: {FileName}", file.FileName);
                return new SaveResponseDto { Errors = new List<string> { "Error saving file: " + file.FileName } };
            }
        }

        public async Task<SaveResponseDto> ValidateAndSaveFile(IFormFile file)
        {
            if (file == null)
                return new SaveResponseDto { Errors = new List<string> { "File cannot be null." } };

            if (!AllowedExtension.Any(x => x.Equals(Path.GetExtension(file.FileName).ToLower())))
                return new SaveResponseDto { Errors = new List<string> { "File must be a valid csv or json file" } };

            return await SaveFile(file);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> ProcessFile()
        {
            var response = await ValidateAndSaveFile(Request?.Form.Files[0]);

            if (response.Errors.NotNullAndAny())
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(string.Join(Environment.NewLine, response.Errors))
                };
            }

            try
            {
                var productsData = GetFileData(response.FilePath).RemoveNulls().Where(x => x.Code.HasValue()).ToList();

                CreateNodes(productsData);

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                };
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(string.Join(Environment.NewLine, response.Errors))
                };
            }
        }

        private void CreateNodes(List<Search.Models.Product> productsData)
        {
            if (umbracoHelperAccessor.TryGetUmbracoHelper(out var umbracoHelper))
            {
                var rootDataRepositories = umbracoHelper.ContentAtRoot().FirstOrDefault().Children?.OfType<DataRepositories>().FirstOrDefault();
                if (rootDataRepositories == null)
                {
                    var node = contentService.Create("Data Repositories", umbracoHelper.ContentAtRoot().FirstOrDefault().Key, DataRepositories.ModelTypeAlias);
                    contentService.SaveAndPublish(node);
                }

                var productsRoot = CreateNode(rootDataRepositories, ProductsContainer.ModelTypeAlias, "Products");

                //Check if Product Tags are present, if not create a new tag under Data Repositories

                var tagsRoot = CreateNode(rootDataRepositories, TagsContainer.ModelTypeAlias, "Tags");

                var productTypesRoot = CreateNode(tagsRoot, ProductTypesContainer.ModelTypeAlias, "Product Types Container");
                var productTermsRoot = CreateNode(tagsRoot, ProductTermsContainer.ModelTypeAlias, "Product Terms Container");
                var productLTVsRoot = CreateNode(tagsRoot, ProductLtvcontainer.ModelTypeAlias, "Product LTV Container");
                var productCategoriesRoot = CreateNode(tagsRoot, ProductCategoriesContainer.ModelTypeAlias, "Product Categories Container");
                var importId = Guid.NewGuid();
                LogAction("Product Import Start. Product Count :" + productsData.Count + ". Import id:" + importId);
                var newProductcodes = new Dictionary<string, IContent>();
                foreach (var product in productsData)
                {
                    LogAction("Product code:" + product.Code);
                    if (newProductcodes.ContainsKey(product.Code))
                    {
                        LogAction("Duplicate in parsed file data. Product code:" + product.Code);
                        continue;
                    }

                    IContent node = null;

                    //Create Tags if not exists
                    Guid productTypeNode = CreateNode(productTypesRoot, ProductType.ModelTypeAlias, product.ProductType).Key;
                    Guid productTermNode = CreateNode(productTermsRoot, ProductTerm.ModelTypeAlias, product.Term).Key;
                    Guid productCategoryNode = CreateNode(productCategoriesRoot, ProductCategory.ModelTypeAlias, product.Category).Key;
                    Guid productLTVNode = CreateNode(productLTVsRoot, ProductLtv.ModelTypeAlias, product.LTVTitle, product.LTVFilterText).Key;

                    //IF the product exists, update the fields
                    if (productsRoot.Children != null && productsRoot.Children.Any() && productsRoot.Children.OfType<Product>().Select(x => x.Name).Contains(product.Code))
                    {
                        node = contentService.GetById(productsRoot.Children.OfType<Product>().Where(x => x.Name.Equals(product.Code, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault().Id);
                    }
                    else
                    {
                        //If product doesn't exists, create a new one
                        var nodeName = product.Code;
                        node = contentService.Create(nodeName, productsRoot.Key, Product.ModelTypeAlias);
                        if (!newProductcodes.ContainsKey(product.Code))
                        {
                            newProductcodes.Add(product.Code, node);
                        }
                        LogAction("Product created. code:" + product.Code + ". node id:" + node.Id);
                    }

                    node.SetValue(ProductConstants.Category, string.Concat("umb://document/", productCategoryNode.ToString().Replace("-", "")));
                    node.SetValue(ProductConstants.Description, product.Description);
                    node.SetValue(ProductConstants.EarlyRepaymentCharges, product.EarlyRepaymentCharges);
                    node.SetValue(ProductConstants.Features, FormatTextToList(product.Features));
                    node.SetValue(ProductConstants.InterestOnly, product.InterestOnly);
                    node.SetValue(ProductConstants.IsFixedRate, product.IsFixedRate);
                    node.SetValue(ProductConstants.LaunchDateTime, product.LaunchDateTime.HasValue() ? DateTime.Parse(product.LaunchDateTime) : (DateTime?)null);
                    node.SetValue(ProductConstants.IsNew, product.IsNew);
                    node.SetValue(ProductConstants.LTVTitle, string.Concat("umb://document/", productLTVNode.ToString().Replace("-", "")));
                    node.SetValue(ProductConstants.OverallCost, product.OverallCost);
                    node.SetValue(ProductConstants.ProductFees, product.ProductFees);
                    node.SetValue(ProductConstants.ProductType, string.Concat("umb://document/", productTypeNode.ToString().Replace("-", "")));
                    node.SetValue(ProductConstants.Rate, product.Rate);
                    node.SetValue(ProductConstants.Term, string.Concat("umb://document/", productTermNode.ToString().Replace("-", "")));
                    node.SetValue(ProductConstants.Code, product.Code);
                    node.SetValue(ProductConstants.ExcludeFromSearch, false);
                    node.SetValue(ProductConstants.AIPDeadlineDateTime, product.AIPDeadlineDateTime.HasValue() ? DateTime.Parse(product.AIPDeadlineDateTime) : (DateTime?)null);
                    node.SetValue(ProductConstants.WithdrawalProductDateTime, product.WithdrawalDateTime.HasValue() ? DateTime.Parse(product.WithdrawalDateTime) : (DateTime?)null);
                    node.SetValue(ProductConstants.ProductVariant, JsonConvert.SerializeObject(new[] { product.ProductVariant }));

                    contentService.SaveAndPublish(node);
                    LogAction("Product Save and publish. code:" + product.Code + ". node id:" + node.Id);
                }

                LogAction("Product Import end. Import id:" + importId);
            }

        }

        private void LogAction(string v)
        {
            logger.LogWarning(v);
        }

        private string FormatTextToList(string input)
        {
            var cleanInput = HttpUtility.HtmlDecode(StripHTML(input)).Replace("Â", string.Empty);

            if (!cleanInput.Contains(";")) return cleanInput;

            var html = cleanInput.Split(';').Aggregate("<ul>", (current, listItem) => current + $"<li><span class=\"list-bullets\">{listItem}</span></li>");

            html += "</ul>";

            return html;

        }

        private static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        private IPublishedContent CreateNode(IPublishedContent root, string modelTypeAlias, string tagName, string lTVFilterText = null)
        {
            if (umbracoHelperAccessor.TryGetUmbracoHelper(out var umbracoHelper))
            {
                //TODO: root.children doesn't return any child node?
                var productRoot = umbracoHelper.Content(root.Id).Children?.FirstOrDefault(x => x.ContentType.Alias.Equals(modelTypeAlias, StringComparison.InvariantCultureIgnoreCase) && x.Name.Equals(tagName.Trim(), StringComparison.InvariantCultureIgnoreCase));
                IPublishedContent node = productRoot ?? null;
                if (node == null)
                {
                    var newNode = contentService.Create(tagName.Trim(), root.Key, modelTypeAlias);
                    if (modelTypeAlias.Equals("productLtv", StringComparison.InvariantCultureIgnoreCase))
                    {
                        newNode.SetValue(ProductConstants.LTVFilterText, lTVFilterText);
                    }
                    contentService.SaveAndPublish(newNode);

                    node = umbracoHelper.Content(newNode.Id);
                }

                return node;
            }
            return null;
        }

        private IEnumerable<Search.Models.Product> GetFileData(string path)
        {
            using var stream = System.IO.File.OpenText(path);
            switch (Path.GetExtension(path))
            {
                case ".csv":
                    try
                    {
                        var csvConfig =
                       new CsvConfiguration(CultureInfo.InvariantCulture)
                       {
                           Delimiter = ",",
                           IgnoreBlankLines = true,
                           HeaderValidated = null,
                           HasHeaderRecord = true
                       };
                        using var csv = new CsvReader(stream, csvConfig);
                        return csv.GetRecords<Search.Models.Product>().ToList();
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                case ".json":
                    try
                    {
                        StreamReader r = new StreamReader(path);
                        string jsonString = r.ReadToEnd();
                        var records = JsonConvert.DeserializeObject<List<Search.Models.Product>>(jsonString);
                        return records;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                default:
                    return Enumerable.Empty<Search.Models.Product>();
            }
            return Enumerable.Empty<Search.Models.Product>();
        }

        [HttpGet]
        public IActionResult ExportProducts()
        {
            try
            {
                var stream = new MemoryStream(ExportProductsData());

                return new FileStreamResult(stream, "text/csv")
                {
                    FileDownloadName = $"MetaData-{DateTime.Now:dd-MMM-yy}.csv"
                };
            }

            catch (Exception ex)
            {
                logger.LogError($"Error - Exporting Meta: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }

        public byte[] ExportProductsData()
        {
            if (umbracoHelperAccessor.TryGetUmbracoHelper(out var umbracoHelper))
            {
                var products = new List<Search.Models.Product>();
                var rootDataRepositories = umbracoHelper.ContentAtRoot().FirstOrDefault().FirstChildOfType(DataRepositories.ModelTypeAlias);
                if (rootDataRepositories == null)
                {
                    LogAction("Data Repository doesn't exist");
                    return null;
                }

                var productsContainer = rootDataRepositories.FirstChildOfType(ProductsContainer.ModelTypeAlias);
                if (productsContainer == null)
                {
                    LogAction("Product Container doesn't exist");
                    return null;
                }

                foreach (var product in productsContainer.Children.OfType<Product>())
                {
                    products.Add(
                        new Search.Models.Product
                        {
                            ProductType = product.ProductType?.Name,
                            Category = product.Category?.Name,
                            LTVTitle = product.LTvtitle?.Name,
                            LTVFilterText = product.LTvtitle?.Value<string>(ProductConstants.LTVFilterText),
                            InterestOnly = product.InterestOnly,
                            LaunchDateTime = product.LaunchDateTime == DateTime.MinValue ? null : product.LaunchDateTime.ToString(),
                            IsNew = product.IsNew,
                            Term = product.Term?.Name,
                            Rate = product.Rate,
                            IsFixedRate = product.IsFixedRate,
                            Description = product.Description?.ToString().StripHTML(),
                            OverallCost = product.OverallCost,
                            ProductFees = product.ProductFees,
                            Features = product.Features?.ToString().ReplaceAllButFirst("<span class=\"list-bullets\">", ";").StripHTML().ReplaceAllButFirst("\n", ""),
                            EarlyRepaymentCharges = product.EarlyRepaymentCharges,
                            Code = product.Code,
                            WithdrawalDateTime = product.WithdrawalProductDateTime == DateTime.MinValue ? null : product.WithdrawalProductDateTime.ToString(),
                            AIPDeadlineDateTime = product.AIpdeadlineDateTime == DateTime.MinValue ? null : product.AIpdeadlineDateTime.ToString(),
                            ProductVariant = product.ProductVariant
                        }
                    );
                }

                using (var memoryStream = new MemoryStream())
                {
                    using (var streamWriter = new StreamWriter(memoryStream, Encoding.GetEncoding("iso-8859-1")))
                    {
                        using (var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                        {
                            csv.WriteRecords(products);
                        }
                        return memoryStream.ToArray();
                    }
                }
            }
            return null;
        }
    }
}