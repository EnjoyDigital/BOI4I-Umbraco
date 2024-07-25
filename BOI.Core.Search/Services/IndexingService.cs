using BOI.Core.Constants;
using BOI.Core.Search.Constants;
using BOI.Core.Search.Models;
using BOI.Core.Extensions;
using BOI.Core.Search.Extensions;
using BOI.Umbraco.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using System.Text;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;
using BOI.Core.Search.Queries.SQL;
using BOI.Core.Search.Models.ElasticSearch;

namespace BOI.Core.Search.Services
{
    public interface IIndexingService
    {
        void CheckAndCreateIndex(IEnumerable<WebContent> content);

        void DeleteItemsFromIndex(params int[] indexItems);

        WebContent DocBuilder(IPublishedContent content);
        string GetIndexName(string indexAlias);
        bool IndexExists(string? indexAlias = null);
        void IndexMediaViewLog(params MediaRequestLog[] mediaRequestLogs);
        void IndexSolicitors(IEnumerable<Solicitor> solicitors);
        void IndexWebContent(params WebContent[] indexItem);
        void ReIndexWebContent(IEnumerable<WebContent> content);
    }

    public class IndexingService : IIndexingService
    {
        private readonly ILogger<IndexingService> logger;
        //private readonly IDatabaseFactory databaseFactory;
        private readonly IPublishedValueFallback publishedValueFallback;
        private readonly IPublishedUrlProvider publishedUrlProvider;
        private readonly IGetAllMediaLogs getAllMediaLogs;
        private readonly IConfiguration config;

        private readonly IElasticClient client;

        /// <summary>
        /// Creates a new instance of the indexing service
        /// </summary>
        /// <param name="logger">An instance of a logger to log progress</param>
        /// <param name="client">A NEST Elasticsearch client</param>
        public IndexingService(IConfiguration config, IElasticClient client, ILogger<IndexingService> logger, /*IDatabaseFactory databaseFactory,*/
            IPublishedValueFallback publishedValueFallback, IPublishedUrlProvider publishedUrlProvider, IGetAllMediaLogs getAllMediaLogs)
        {
            this.config = config;
            this.client = client;
            this.logger = logger;
            //this.databaseFactory = databaseFactory;
            this.publishedValueFallback = publishedValueFallback;
            this.publishedUrlProvider = publishedUrlProvider;
            this.getAllMediaLogs = getAllMediaLogs;
        }
        private ElasticSettings EsIndexes => new(config);

        /// <summary>
        /// Creates a new index suffixed with todays date. Uses the bulk all feature of Elasticsearch to index batches of Solicitors. 
        /// It then swaps the Solicitor alias used in queries to point the new index.
        /// </summary>
        /// <param name="solicitors"> A collection of Solicitors</param>
        /// 
        public void IndexSolicitors(IEnumerable<Solicitor> solicitors)
        {
            var indexName = EsIndexes.SolicitorEsIndexAlias;
            var indexAlias = string.Format("{0}{1}", EsIndexes.SolicitorEsIndexAlias, "_index");

            var indexExists = client.Indices.Exists(indexAlias).Exists;

            if (indexExists)
            {
                client.Indices.Delete(indexName);
            }

            var createIndexResponse = client.Indices.Create(indexName, c => c.Map<Solicitor>(m => m
                    .AutoMap()
                )
            );

            var bulkAllObservable = client.BulkAll(solicitors.RemoveNulls(), b => b
                    .Index(indexName)
                    .BackOffTime("30s")
                    .BackOffRetries(2)
                    .RefreshOnCompleted(true)
                    .MaxDegreeOfParallelism(Environment.ProcessorCount)
                    .Size(10000)
                )
                .Wait(TimeSpan.FromMinutes(15), next =>
                {
                    // do something e.g. write number of pages to console
                });


            client.Indices.BulkAlias(aliases =>
            {
                if (indexExists)
                {
                    aliases.Remove(a => a.Alias(indexAlias).Index("*"));
                }
                return aliases.Add(a => a.Alias(indexAlias).Index(indexName));
            });
        }

        /// <summary>
        /// Gets an index name (alias suffixed with date). 
        /// </summary>
        /// <param name="indexAlias">An index alias. If empty will default to the webcontent index alias</param>
        /// <returns>Index name</returns>
        public string GetIndexName(string indexAlias = null)
        {
            logger.LogInformation("GetIndexName");

            if (!indexAlias.HasValue())
            {
                indexAlias = EsIndexes.WebContentEsIndexAlias;
            }

            logger.LogInformation(string.Format("GetIndexName - {0}-{1}:yyyy.MM.dd", indexAlias, DateTime.UtcNow));

            return $"{indexAlias}-{DateTime.UtcNow:yyyy.MM.dd}";
        }


        public void CheckAndCreateIndex(IEnumerable<WebContent> content)
        {
            var indexAlias = EsIndexes.WebContentEsIndexAlias;
            var indexExists = client.Indices.Exists(indexAlias).Exists;

            logger.LogInformation("Check and create index");


            if (!indexExists)
            {
                logger.LogInformation("Check and create index - index doesn't exists");

                var indexName = GetIndexName(indexAlias);

                var createIndexResponse = client.Indices.Create(indexName, c => c
                .Settings(s => s
                .Analysis(an => an
                    .Analyzers(n => n
                        .Custom("ignore_html_tags", cn => cn
                            .Tokenizer("standard")
                            .Filters("lowercase")
                            .CharFilters(new[] { "ignore_html_tags" })
                        )
                    )
                    .CharFilters(cf => cf
                        .HtmlStrip("ignore_html_tags")
                     )
                ))
                .Map<WebContent>(m => m
                        .AutoMap()
                        .Properties(ps => ps
                            .Nested<Tags>(f => f.Name(p => p.Tags))
                            .Text(f => f
                                 .Name(p => p.Content)
                                 .Analyzer("ignore_html_tags")
                                 .Fields(fi => fi
                                     .Keyword(kw => kw
                                         .Name("keyword")
                                         .IgnoreAbove(256)
                                     )
                                 )
                            )
                        )

                    ));

                logger.LogInformation(createIndexResponse.IsValid.ToString());


                if (createIndexResponse.IsValid)
                {
                    client.Indices.BulkAlias(aliases =>
                    {
                        return aliases.Add(a => a.Alias(indexAlias).Index(indexName));
                    });
                }

                try
                {
                    ReIndexWebContent(content);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "media request log index not rebuilt");
                }
            }

            var mediLogIndexAlias = EsIndexes.MediaEsIndexAlias;
            if (mediLogIndexAlias.HasValue())
            {
                var mediLogIndexExists = client.Indices.Exists(mediLogIndexAlias).Exists;

                if (!mediLogIndexExists)
                {

                    logger.LogInformation("Check and create index - media log index doesn't exists");

                    var indexName = GetIndexName(mediLogIndexAlias);

                    var createIndexResponse = client.Indices.Create(indexName, c => c.Map<MediaRequestLog>(m => m
                            .AutoMap()
                            .Properties(ps => ps
                                .Keyword(t => t.Name(n => n.MediaItemId))
                                .Keyword(t => t.Name(n => n.MediaUrl))
                                .Date(t => t.Name(n => n.DateViewed))
                            )
                        )
                    );

                    logger.LogInformation(createIndexResponse.IsValid.ToString());


                    if (createIndexResponse.IsValid)
                    {
                        client.Indices.BulkAlias(aliases =>
                        {

                            return aliases.Add(a => a.Alias(mediLogIndexAlias).Index(indexName));
                        });
                    }

                    try
                    {
                        ReIndexMediaViewLogs(mediLogIndexAlias);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "media request log index not rebuilt");
                    }
                }

            }

        }

        public void IndexMediaViewLog(params MediaRequestLog[] mediaRequestLogs)
        {

            //MediaRequestLogIndexAlias
            foreach (var item in mediaRequestLogs)
            {
                if (item == null)
                {

                    continue;
                }

                var index = client.Index(item, i => i.Index(EsIndexes.MediaEsIndexAlias));
                if (!index.IsValid)
                {
                    logger.LogWarning("media request log not indexed");
                    logger.LogWarning(index.DebugInformation.HasValue() ? index.DebugInformation : "");
                    logger.LogError("Elastic search error", index.OriginalException);
                }
            }
        }

        //TODO:reindex all media log DB entries
        private async Task ReIndexMediaViewLogs(string mediaLogIndexAlias)
        {
            const int pageSize = 10000;
            var pageIndex = 1;

            var currentPageResultSize = 0;

            do
            {
                var pagedSet = await getAllMediaLogs.Execute(pageIndex);
                currentPageResultSize = pagedSet.Items.Count();
                IndexMediaViewLog(pagedSet.Items.ToArray());

                pageIndex++;
            } while (currentPageResultSize == pageSize);
        }

        public void IndexWebContent(params WebContent[] indexItems)
        {
            foreach (var item in indexItems)
            {
                if (item == null)
                {
                    continue;
                }
                var docCheck = client.DocumentExists<WebContent>(item.Id, d => d.Index(EsIndexes.WebContentEsIndexAlias));



                if (!docCheck.Exists)
                {
                    var index = client.Index(item, i => i.Index(EsIndexes.WebContentEsIndexAlias));
                    if (!index.IsValid)
                    {

                        logger.LogWarning("Content not indexed");
                        logger.LogWarning(index.DebugInformation.HasValue() ? index.DebugInformation : "");
                        logger.LogError("Elastic search error", index.OriginalException);
                    }
                }
                else
                {
                    var update = client.Update<WebContent>(item.Id, u => u.Doc(item).Index(EsIndexes.WebContentEsIndexAlias));
                    if (!update.IsValid)
                    {
                        logger.LogWarning("Content not indexed");
                        logger.LogWarning(update.DebugInformation);

                        logger.LogError("Elastic search error", update.OriginalException);
                    }
                }
            }


        }

        /// <summary>
        /// Used to index web content from the site
        /// </summary>
        public void ReIndexWebContent(IEnumerable<WebContent> content)
        {
            logger.LogInformation("ReIndexWebContent");

            var indexAlias = EsIndexes.WebContentEsIndexAlias;
            var indexName = GetIndexName();

            logger.LogInformation("ReIndexWebContent - index content");

            var bulkAllObservable = client.BulkAll(content.RemoveNulls(), b => b
                .Index(indexName)
                .BackOffTime("30s")
                .BackOffRetries(2)
                .RefreshOnCompleted(true)
                .MaxDegreeOfParallelism(Environment.ProcessorCount)
                .Size(10000)
            )
            .Wait(TimeSpan.FromMinutes(15), next => { });

            var indexExistsWithAlias = client.Indices.Exists(indexAlias).Exists;

            client.Indices.BulkAlias(aliases =>
            {
                if (indexExistsWithAlias)
                {
                    aliases.Remove(a => a.Alias(indexAlias).Index("*"));
                }
                return aliases.Add(a => a.Alias(indexAlias).Index(indexName));
            });
        }

        public void DeleteItemsFromIndex(params int[] indexItemIds)
        {
            foreach (var id in indexItemIds)
            {
                var docCheck = client.DocumentExists<WebContent>(id, d => d.Index(EsIndexes.WebContentEsIndexAlias));

                if (!docCheck.Exists)
                { continue; }

                var delete = client.Delete<WebContent>(id, d => d.Index(EsIndexes.WebContentEsIndexAlias));

            }


        }

        public WebContent DocBuilder(IPublishedContent content)
        {
            //todo: T11360 - returning null won't update the index as it's got nothing to update to say it's false
            //if (!content.HasProperty("excludeFromSearch") || content.Value<bool>("excludeFromSearch"))
            //{
            //    return null;
            //}

            var combinedContent = new StringBuilder();
            //TODO: extract this to a method or an extension method on something appropriate
            var blockListContent = content.HasValue("content") ? content.Value<BlockListModel>(publishedValueFallback, "content") : null;
            if (blockListContent != null && blockListContent.Any())
            {
                foreach (var block in blockListContent)
                {
                    var blockContent = block.Content;
                    switch (block.Content.ContentType.Alias)
                    {
                        case "richTextBlock":
                            combinedContent.AppendLine(block.Content.HasValue("richText") ? block.Content.Value<string>(publishedValueFallback, "richText") : null);
                            break;
                        case "calculatorBlock":
                            break;
                        case "childPageBlock":
                            combinedContent.AppendLine(block.Content.HasValue("title") ? block.Content.Value<string>(publishedValueFallback, "title") : null);
                            var childPages = block.Content.HasValue("childPages") ? block.Content.Value<BlockListModel>(publishedValueFallback, "childPages") : null;
                            foreach (var childPage in childPages)
                            {
                                combinedContent.AppendLine(childPage.Content.HasValue("childTitle") ? childPage.Content.Value<string>(publishedValueFallback, "childTitle") : null);
                                combinedContent.AppendLine(childPage.Content.HasValue("childSubText") ? childPage.Content.Value<string>(publishedValueFallback, "childSubText") : null);
                            }
                            break;
                        case "contactDetailBlock":
                            var contactDetailList = block.Content.HasValue("contactDetailList") ? block.Content.Value<BlockListModel>(publishedValueFallback, "contactDetailList") : null;
                            foreach (var contactDetailItem in contactDetailList)
                            {
                                combinedContent.AppendLine(contactDetailItem.Content.HasValue("contactDetailCopy") ? contactDetailItem.Content.Value<string>(publishedValueFallback, "contactDetailCopy") : null);
                            }
                            break;
                        case "documentDownloadAndCTALinkBlock":
                            combinedContent.AppendLine(block.Content.HasValue("title") ? block.Content.Value<string>(publishedValueFallback, "title") : null);
                            var documentsLinks = block.Content.HasValue("documentsLinks") ? block.Content.Value<BlockListModel>(publishedValueFallback, "documentsLinks") : null;
                            foreach (var documentsLink in documentsLinks)
                            {
                                var documentsLinkblockContent = documentsLink.Content;
                                switch (documentsLink.Content.ContentType.Alias)
                                {
                                    case "documentUploadBlock":
                                        var documentsLinkBlockDocuments = documentsLink.Content.HasValue("documents") ? documentsLink.Content.Value<BlockListModel>(publishedValueFallback, "documents") : null;
                                        foreach (var documentsLinkBlockDocument in documentsLinkBlockDocuments)
                                        {
                                            combinedContent.AppendLine(documentsLinkBlockDocument.Content.HasValue("title") ? documentsLinkBlockDocument.Content.Value<string>(publishedValueFallback, "title") : null);
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        case "documentUploadBlock":
                            var documents = block.Content.HasValue("documents") ? block.Content.Value<BlockListModel>(publishedValueFallback, "documents") : null;
                            if (documents.NotNullAndAny())
                            {
                                foreach (var document in documents)
                                {
                                    combinedContent.AppendLine(document.Content.HasValue("title") ? document.Content.Value<string>(publishedValueFallback, "title") : null);
                                }
                            }
                            break;
                        case "fAQBlock":
                            combinedContent.AppendLine(block.Content.HasValue("title") ? block.Content.Value<string>(publishedValueFallback, "title") : null);
                            var fAQs = block.Content.HasValue("fAQs") ? block.Content.Value<BlockListModel>(publishedValueFallback, "fAQs") : null;
                            if (fAQs.NotNullAndAny())
                            {
                                foreach (var faq in fAQs)
                                {
                                    combinedContent.AppendLine(faq.Content.HasValue("question") ? faq.Content.Value<string>(publishedValueFallback, "question") : null);
                                    combinedContent.AppendLine(faq.Content.HasValue("answer") ? faq.Content.Value<string>(publishedValueFallback, "answer") : null);
                                }
                            }
                            break;
                        case "gridBlock":
                            break;
                        case "howItWorksBlock":
                            combinedContent.AppendLine(block.Content.HasValue("title") ? block.Content.Value<string>(publishedValueFallback, "title") : null);
                            var steps = block.Content.HasValue("steps") ? block.Content.Value<BlockListModel>(publishedValueFallback, "steps") : null;
                            if (steps.NotNullAndAny())
                            {
                                foreach (var step in steps)
                                {
                                    combinedContent.AppendLine(step.Content.HasValue("stepTitle") ? step.Content.Value<string>(publishedValueFallback, "stepTitle") : null);
                                    combinedContent.AppendLine(step.Content.HasValue("subSteps") ? step.Content.Value<string>(publishedValueFallback, "subSteps") : null);
                                }
                            }
                            break;
                        case "imageBlock":
                            combinedContent.AppendLine(block.Content.HasValue("caption") ? block.Content.Value<string>(publishedValueFallback, "caption") : null);
                            break;
                        case "imageGalleryBlock":
                            combinedContent.AppendLine(block.Content.HasValue("title") ? block.Content.Value<string>(publishedValueFallback, "title") : null);
                            var gallery = block.Content.HasValue("gallery") ? block.Content.Value<BlockListModel>(publishedValueFallback, "gallery") : null;
                            if (gallery.NotNullAndAny())
                            {
                                foreach (var imageBlock in gallery)
                                {
                                    combinedContent.AppendLine(imageBlock.Content.HasValue("caption") ? imageBlock.Content.Value<string>(publishedValueFallback, "caption") : null);
                                }
                            }
                            break;
                        case "videoBlock":
                            combinedContent.AppendLine(block.Content.HasValue("title") ? block.Content.Value<string>(publishedValueFallback, "title") : null);
                            combinedContent.AppendLine(block.Content.HasValue("caption") ? block.Content.Value<string>(publishedValueFallback, "caption") : null);
                            break;
                        default:
                            break;
                    }
                }
            }

            var searchTitle = content.HasValue("searchTitle") ? content.Value<string>(publishedValueFallback, "searchTitle") : content.Name;
            var searchDescription = content.HasValue("searchDescription") ? content.Value<string>(publishedValueFallback, "searchDescription") : "";
            var searchKeywords = content.HasValue("searchKeywords") ? content.Value<string>(publishedValueFallback, "searchKeywords") : "";
            var searchImage = content.HasValue("searchImage") ? content.Value<IPublishedContent>(publishedValueFallback, "searchImage")?.Url(publishedUrlProvider) : "";
            var searchExclude = content.HasProperty("excludeFromSearch") ? content.Value<bool>(publishedValueFallback, "excludeFromSearch") : true;

            var metaTitle = content.HasValue("metaTitle") ? content.Value<string>(publishedValueFallback, "metaTitle") : "";
            var metaDescription = content.HasValue("metaDescription") ? content.Value<IPublishedContent>(publishedValueFallback, "metaDescription")?.Url(publishedUrlProvider) : "";
            var canonicalURL = content.HasProperty("canonicalURL") ? content.Value<bool>(publishedValueFallback, "canonicalURL") : true;

            var listingSummary = content.HasValue("listingSummary") ? content.Value<string>(publishedValueFallback, "listingSummary") : "";
            var listingImage = content.HasValue("listingImage") ? content.Value<IPublishedContent>(publishedValueFallback, "listingImage")?.Url(publishedUrlProvider) : "";

            var parentNodeId = content.Parent.Id;
            var nodeTypeAlias = content.ContentType.Alias;
            var sortOrder = content.SortOrder;

            bool BuyToLetProduct = false, ResidentialProduct = false, BespokeProduct = false;
            string criteriaName = "";
            string criteriaCategory = "";

            if (string.Equals(content.ContentType.Alias, "criteria", StringComparison.InvariantCultureIgnoreCase))
            {
                if (content.HasValue("criteriaName") && !string.IsNullOrEmpty(content.Value<string>(publishedValueFallback, "criteriaName")))
                {
                    criteriaName = content.Value<string>(publishedValueFallback, "criteriaName");
                }
                else
                {
                    criteriaName = content.Name;
                }
                criteriaCategory = content.HasValue("criteriaCategory") ? content.Value<IPublishedContent>(publishedValueFallback, "criteriaCategory")?.Name : "";
                var criteriaProductType = content.HasValue("criteriaProductType") ? content.Value<IEnumerable<string>>(publishedValueFallback, "criteriaProductType") : Enumerable.Empty<string>();
                if (criteriaProductType != null && criteriaProductType.Any())
                {
                    foreach (var criteriaproductType in criteriaProductType)
                    {
                        switch (criteriaproductType)
                        {
                            case "Buy to Let Criteria":
                                BuyToLetProduct = string.Equals(criteriaproductType, "Buy to Let Criteria", StringComparison.InvariantCultureIgnoreCase);
                                break;
                            case "Residential Criteria":
                                ResidentialProduct = string.Equals(criteriaproductType, "Residential Criteria", StringComparison.InvariantCultureIgnoreCase);
                                break;
                            case "Bespoke Criteria":
                                BespokeProduct = string.Equals(criteriaproductType, "Bespoke Criteria", StringComparison.InvariantCultureIgnoreCase);
                                break;
                            default:
                                BuyToLetProduct = false;
                                ResidentialProduct = false;
                                break;
                        }
                    }
                }
            }

            string bodyText = null, criteriaUpdateDate = null, criteriaTabUpdateDate = null;

            if (string.Equals(content.ContentType.Alias, "criteria", StringComparison.InvariantCultureIgnoreCase) || string.Equals(content.ContentType.Alias, "criteriaTab", StringComparison.InvariantCultureIgnoreCase))
            {
                bodyText = content.HasValue("bodyText") ? content.Value<string>(publishedValueFallback, "bodyText") : "";
                criteriaUpdateDate = content.HasValue("criteriaUpdatedDate") ? content.Value<DateTime>(publishedValueFallback, "criteriaUpdatedDate").ToString("yyyy-MM-ddTHH:mm:sszzz") : string.Empty;
                criteriaTabUpdateDate = content.HasValue("criteriaTabUpdatedDate") ? content.Value<DateTime>(publishedValueFallback, "criteriaTabUpdatedDate").ToString("yyyy-MM-ddTHH:mm:sszzz") : string.Empty;
            }

            string productType = null, category = null, lTVTitle = null, lTVFilterText = null, term = null, rate = null, description = null, overallCost = null, productFees = null, features = null, earlyRepaymentCharges = null, code = null, productVariant = null, withdrawalDateTime = null, aIPDeadlineDateTime = null, launchDateTime = null;
            bool interestOnly = false, isNew = false, isFixedRate = false;

            if (string.Equals(content.ContentType.Alias, "product", StringComparison.InvariantCultureIgnoreCase))
            {
                productType = content.Value<IPublishedContent>(publishedValueFallback, "productType")?.Name ?? "";
                category = content.Value<IPublishedContent>(publishedValueFallback, "category")?.Name ?? "";
                lTVTitle = content.Value<IPublishedContent>(publishedValueFallback, "lTVTitle")?.Name ?? "";
                lTVFilterText = content.Value<IPublishedContent>(publishedValueFallback, "lTVTitle")?.Value<string>(publishedValueFallback, "lTVFilterText") ?? "";
                term = content.Value<IPublishedContent>(publishedValueFallback, "term")?.Name ?? "";
                rate = content.HasValue("rate") ? content.Value<string>(publishedValueFallback, "rate") : "";
                description = content.HasValue("description") ? content.Value<string>(publishedValueFallback, "description") : "";
                overallCost = content.HasValue("overallCost") ? content.Value<string>(publishedValueFallback, "overallCost") : "";
                productFees = content.HasValue("productFees") ? content.Value<string>(publishedValueFallback, "productFees") : "";
                features = content.HasValue("features") ? content.Value<string>(publishedValueFallback, "features") : "";
                earlyRepaymentCharges = content.HasValue("earlyRepaymentCharges") ? content.Value<string>(publishedValueFallback, "earlyRepaymentCharges") : "";
                code = content.HasValue("code") ? content.Value<string>(publishedValueFallback, "code") : "";
                interestOnly = content.HasValue("interestOnly") ? content.Value<bool>(publishedValueFallback, "interestOnly") : false;
                launchDateTime = content.HasValue("launchDateTime") ? content.Value<DateTime>(publishedValueFallback, "launchDateTime").ToString("yyyy-MM-dd") : string.Empty;
                isNew = content.HasValue("isNew") ? content.Value<bool>(publishedValueFallback, "isNew") : false;
                isFixedRate = content.HasValue("isFixedRate") ? content.Value<bool>(publishedValueFallback, "isFixedRate") : false;
                withdrawalDateTime = content.HasValue("withdrawalProductDateTime") ? content.Value<DateTime>(publishedValueFallback, "withdrawalProductDateTime").ToString("yyyy-MM-ddTHH:mm:sszzz") : "";
                aIPDeadlineDateTime = content.HasValue("aIPDeadlineDateTime") ? content.Value<DateTime>(publishedValueFallback, "aIPDeadlineDateTime").ToString("yyyy-MM-ddTHH:mm:sszzz") : "";
                productVariant = content.HasValue("productVariant") ? content.Value<string>(publishedValueFallback, "productVariant") : "";
            }

            if (string.Equals(content.ContentType.Alias, "productsLanding", StringComparison.InvariantCultureIgnoreCase) || string.Equals(content.ContentType.Alias, "withdrawalProductsLanding", StringComparison.InvariantCultureIgnoreCase))
            {
                productVariant = content.HasValue("productVariant") ? content.Value<string>(publishedValueFallback, "productVariant") : "";
            }

            var tags = content.HasProperty("tags") && content.HasValue("tags") ? content.Value<IEnumerable<string>>(publishedValueFallback, "tags") : null;
            var tagsList = new List<Tags>();

            if (tags != null)
            {
                tagsList.AddRange(tags.Select(tag => new Tags() { Tag = tag }));
            }


            string regionsList = null, bio = null, bdmId = null, fCANumberList = null, postcodeOutcodes = null, bdmType = null;
            bool active = true, requireFcaAndpc = false;
            if (string.Equals(content.ContentType.Alias, "bDMContact", StringComparison.InvariantCultureIgnoreCase))
            {
                var rawPostCodes = content.Value(publishedValueFallback, "regions", fallback: Fallback.ToDefaultValue, defaultValue: "").Replace(" ", "").ToUpperInvariant();
                postcodeOutcodes = string.Join(" ", rawPostCodes.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.PostcodeOutCode()) ?? Enumerable.Empty<string>());

                regionsList = rawPostCodes.Replace(",", " ");


                logger.LogInformation(content.Name);


                fCANumberList = content.Value(publishedValueFallback, "fCANumber", fallback: Fallback.ToDefaultValue, defaultValue: "").Replace(",", " ");

                bdmType = content.Value<string>(publishedValueFallback, BdmContactConstants.BDMType);
                bio = content.Value<string>(publishedValueFallback, BdmContactConstants.Bio);
                active = content.Value<bool>(publishedValueFallback, BdmContactConstants.Active);
                bdmId = content.Value<string>(publishedValueFallback, "email");
                requireFcaAndpc = content.Value<bool>(publishedValueFallback, BdmContactConstants.RequireFCAAndPostcodeMatch, fallback: Fallback.ToDefaultValue, defaultValue: false);
            }


            var doc = new WebContent()
            {
                Id = content.Id,
                Name = content.Name,
                //Template = content.GetTemplateAlias(),
                Url = content.Url(publishedUrlProvider),
                ParentNodeId = parentNodeId,
                NodeTypeAlias = nodeTypeAlias,
                SortOrder = sortOrder,

                SearchTitle = searchTitle,
                SearchDescription = searchDescription,
                SearchImage = searchImage,
                SearchExclude = searchExclude,
                SearchKeywords = searchKeywords,

                MetaTitle = metaTitle,
                MetaDescription = metaDescription,
                CanonicalURL = canonicalURL,

                ListingImage = listingImage,
                ListingSummary = listingSummary,

                CriteriaName = criteriaName,
                CriteriaCategory = criteriaCategory,
                BuyToLetProduct = BuyToLetProduct,
                ResidentialProduct = ResidentialProduct,
                BespokeProduct = BespokeProduct,
                BodyText = bodyText,
                CriteriaUpdateDate = criteriaUpdateDate,
                CriteriaTabUpdateDate = criteriaTabUpdateDate,

                Content = !string.IsNullOrWhiteSpace(combinedContent.ToString().Trim()) ? combinedContent.ToString().Trim() : !string.IsNullOrWhiteSpace(bodyText) ? bodyText : null,
                Tags = tagsList,
                Regions = regionsList,
                PostCodeOutCodes = postcodeOutcodes,
                FCANumber = fCANumberList,
                Bio = bio,
                Published = true,
                BDMIdentifier = bdmId,
                RequireFCAAndPostcodeMatch = requireFcaAndpc ? "1" : "0",
                BDMType = bdmType,

                ProductType = productType,
                Category = category,
                LTVTitle = lTVTitle,
                LTVFilterText = lTVFilterText,
                InterestOnly = interestOnly,
                LaunchDateTime = launchDateTime,
                IsNew = isNew,
                Term = term,
                Rate = rate,
                IsFixedRate = isFixedRate,
                Description = description,
                OverallCost = overallCost,
                ProductFees = productFees,
                Features = features,
                EarlyRepaymentCharges = earlyRepaymentCharges,
                Code = code,
                ProductVariant = productVariant,
                WithdrawalDateTime = withdrawalDateTime,
                AIPDeadlineDateTime = aIPDeadlineDateTime,

                CreateDate = content.CreateDate,
                UpdateDate = content.UpdateDate,
            };

            return doc;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexAlias">If empty will default to the webcontent index alias</param>
        /// <returns></returns>
        public bool IndexExists(string indexAlias = null)
        {
            if (!indexAlias.HasValue())
            {
                indexAlias = EsIndexes.WebContentEsIndexAlias;
            }

            return client.Indices.Exists(indexAlias).Exists; ;
        }
    }

}
