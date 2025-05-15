

using BOI.Core.Extensions;
using BOI.Core.Search.Extensions;
using BOI.Core.Search.Models;
using BOI.Umbraco.Models;
using Microsoft.Extensions.Configuration;
using Nest;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace BankOfIreland.Intermediaries.Feature.Search.Queries.Elastic
{
    public class MainSearchQuery : IMainSearchQuery
    {
        public IEnumerable<SearchFilter> Filters
        {
            get
            {
                var filters = new List<SearchFilter>();

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    filters.Add(new SearchFilter
                    {
                        Name = "SearchTerm",
                        Label = "SearchTerm",
                        Value = SearchTerm,
                    });
                }

                var additionalParams = "";

                if (Page > 0)
                {
                    additionalParams = $"&Page={Page}";
                }

                if (Size > 10)
                {
                    additionalParams += $"&Size={Size}";
                }

                filters = filters.Select(f => f.UpdateQueryString(string.Join("&", filters.Where(ff => ff.Name != f.Name).Select(fs => $"{fs.Name}={fs.Value}")) + additionalParams)).ToList();

                return filters;
            }
        }
        public int Page { get; set; }
        public int Size { get; set; }
        public string SearchTerm { get; set; }
    }

    public class SearchResultSearcher : ISearchResultSearcher
    {
        private readonly IConfiguration configuration;
        private readonly IElasticClient esClient;
        private readonly IShortStringHelper shortStringHelper;
        private IConfigurationRoot configuration1;
        private WebContent criteriaLandingPage = null;
        private WebContent fAQLandingPage = null;

        public SearchResultSearcher(IConfiguration configuration, IElasticClient esClient, IShortStringHelper shortStringHelper)
        {
            this.configuration = configuration;
            this.esClient = esClient;
            this.shortStringHelper = shortStringHelper;
        }

        public SearchResultSearcher(IConfigurationRoot configuration1)
        {
            this.configuration1 = configuration1;
        }

        public QueryContainer BuildQueryContainer(IMainSearchQuery model)
        {
            var query = new QueryContainerDescriptor<WebContent>();

            var queryContainer =
                (query.Bool(b => b.Must(m => m.Match(t => t.Field(f => f.Name).Operator(Operator.And).Query(model.SearchTerm).Boost(20))))
                 ||
                 (query.Bool(b => b.Must(m => m.MatchPhrasePrefix(t => t.Field(f => f.Content).Query(model.SearchTerm)))) ||
                 query.Bool(b => b.Must(m => m.MatchPhrasePrefix(t => t.Field(f => f.SearchKeywords).Query(model.SearchTerm)))) ||
                 query.Bool(b => b.Must(m => m.MatchPhrasePrefix(t => t.Field(f => f.Name).Query(model.SearchTerm))))
                 && query.Bool(b => b.MustNot(m => m.Match(t => t.Field(f => f.Name).Query(string.Empty))))))
                 && query.Bool(b => b.MustNot(m => m.Term(ma => ma.Field(f => f.SearchExclude).Value(true))));

            return queryContainer;
        }

        public MainSearchResults Execute(IMainSearchQuery model)
        {
            if (model.Size == 0)
            {
                model.Size = 6;
            }

            if (model.Page < 1)
            {
                model.Page = 1;
            }

            var search = esClient
                .Search<WebContent>(s => s
                    .Index(configuration["ElasticSettings:WebContentEsIndexAlias"])
                    .TrackTotalHits()
                    .From((model.Page - 1) * model.Size)
                    .Size(model.Size)
                    .Query(q => BuildQueryContainer(model))
                )
                .EnsureSuccess();

            var resultsHits = search.Hits;
            List<MainSearchResult> queryResults;

            queryResults = search.Hits.Select(r => new MainSearchResult()
            {
                SearchTitle = r.Source.SearchTitle,
                SearchDescription = r.Source.SearchDescription,
                SearchLink = GetSearchLink(r.Source),
            }).ToList();

            var results = new MainSearchResults
            {
                QueryResults = queryResults,
                Total = Convert.ToInt32(search.Total),
                Page = model.Page,
                Size = model.Size,
                Filters = model.Filters
            };

            return results;
        }

        private string GetSearchLink(WebContent Source)
        {
            var SearchLinkUrl = "";
            if (Source.NodeTypeAlias.Equals("product", StringComparison.InvariantCultureIgnoreCase))
            {
                SearchLinkUrl = GetProductLandingPage(Source.ProductVariant, Source.WithdrawalDateTime.HasValue(), Source.Code, Source.InterestOnly);
            }
            else if (Source.NodeTypeAlias.Equals("criteria", StringComparison.InvariantCultureIgnoreCase))
            {
                SearchLinkUrl = GetCriteriaLookupLandingPageFromCriteria(Source, Source.ResidentialProduct, Source.BespokeProduct);
            }
            else if (Source.NodeTypeAlias.Equals("criteriaTab", StringComparison.InvariantCultureIgnoreCase))
            {
                SearchLinkUrl = GetCriteriaLookupLandingPageFromCriteriaTab(Source.ParentNodeId);
            }
            else if (Source.NodeTypeAlias.Equals("fAQ", StringComparison.InvariantCultureIgnoreCase))
            {
                SearchLinkUrl = GetFAQLandingPageFromFAQ(Source);
            }
            else if (Source.NodeTypeAlias.Equals("fAQTab", StringComparison.InvariantCultureIgnoreCase))
            {
                SearchLinkUrl = GetFAQLandingPageFromFAQTab(Source.ParentNodeId);
            }
            else
            {
                SearchLinkUrl = !string.IsNullOrWhiteSpace(Source.Url) ? Source.Url : "";
            }
            return SearchLinkUrl;
        }

        private string GetFAQLandingPageFromFAQTab(int parentNodeId)
        {
            var search = esClient
                .Search<WebContent>(s => s
                    .Index(configuration["ElasticSettings:WebContentEsIndexAlias"])
                    .Query(q => q
                        .Bool(b => b
                            .Must(
                                a =>
                                {
                                    return a.Term(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Value(FAQ.ModelTypeAlias));
                                },
                                a =>
                                {
                                    return a.Term(t => t.Field(tf => tf.Id).Value(parentNodeId));
                                }
                            )
                        )
                    )
                )
            .EnsureSuccess();

            return search.Documents.Any() ? GetFAQLandingPageFromFAQ(search?.Documents?.FirstOrDefault()) : "#";
        }

        private string GetFAQLandingPageFromFAQ(WebContent source)
        {
            if (fAQLandingPage == null)
            {
                var search = esClient
                    .Search<WebContent>(s => s
                        .Index(configuration["ElasticSettings:WebContentEsIndexAlias"])
                        .Query(q => q
                            .Bool(b => b
                                .Must(
                                    a =>
                                    {
                                        return a.Term(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Value(FAqlanding.ModelTypeAlias));
                                    }
                                )
                            )
                        )
                    )
                .EnsureSuccess();

                if (search.Documents.Any())
                {
                    fAQLandingPage = search.Documents.FirstOrDefault();
                }
            }

            if (fAQLandingPage != null)
            {
                return string.Concat(fAQLandingPage.Url, "#", source.Name.ToCleanString(shortStringHelper, CleanStringType.UrlSegment).ToLower().Replace(" ", "-"));
            }
            else
            { return "#"; }
        }

        private string GetCriteriaLookupLandingPageFromCriteriaTab(int parentNodeId)
        {
            var search = esClient
                .Search<WebContent>(s => s
                    .Index(configuration["ElasticSettings:WebContentEsIndexAlias"])
                    .Query(q => q
                        .Bool(b => b
                            .Must(
                                a =>
                                {
                                    return a.Term(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Value("criteria"));
                                },
                                a =>
                                {
                                    return a.Term(t => t.Field(tf => tf.Id).Value(parentNodeId));
                                }
                            )
                        )
                    )
                )
            .EnsureSuccess();

            return search.Documents.Any() ? GetCriteriaLookupLandingPageFromCriteria(search.Documents.FirstOrDefault(), search.Documents.FirstOrDefault().ResidentialProduct, search.Documents.FirstOrDefault().BespokeProduct) : "#";
        }

        private string GetCriteriaLookupLandingPageFromCriteria(WebContent source, bool residentialProduct, bool bespokeProduct)
        {
            if (criteriaLandingPage == null)
            {
                var search = esClient
                    .Search<WebContent>(s => s
                        .Index(configuration["ElasticSettings:WebContentEsIndexAlias"])
                        .Query(q => q
                            .Bool(b => b
                                .Must(
                                    a =>
                                    {
                                        return a.Term(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Value("criteriaLookupLanding"));
                                    }
                                )
                            )
                        )
                    )
                .EnsureSuccess();

                if (search.Documents.Any())
                {
                    criteriaLandingPage = search.Documents.FirstOrDefault();
                }
            }

            if (criteriaLandingPage != null)
            {
                return string.Concat(criteriaLandingPage.Url, !residentialProduct ? "?isBuyToLet=true" : "", bespokeProduct ? "?isBespoke=true" : "", "#", source.Name.ToCleanString(shortStringHelper, CleanStringType.UrlSegment).ToLower().Replace(" ", "-"));
            }
            else
            { return "#"; }

        }

        private string GetProductLandingPage(string productVariant, bool isWithdrawalProduct, string productCode, bool interestOnly)
        {
            var search = esClient
                .Search<WebContent>(s => s
                    .Index(configuration["ElasticSettings:WebContentEsIndexAlias"])
                    .TrackTotalHits()
                    .Size(10000)
                    .Query(q => q
                        .Bool(b => b
                            .Must(
                                a =>
                                {
                                    if (isWithdrawalProduct)
                                    {
                                        return a.Term(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Value("withdrawalProductsLanding"));
                                    }
                                    return a.Term(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Value("productsLanding"));
                                },
                                a =>
                                {
                                    return a.Term(t => t.Field(tf => tf.ProductVariant.Suffix("keyword")).Value(productVariant));
                                }
                            )
                        )
                    )
                )
            .EnsureSuccess();

            return search.Documents.Any() ? string.Concat(search.Documents?.FirstOrDefault()?.Url, interestOnly ? "?InterestOnly=true" : "", "#", productCode) : "#";

        }

    }
}
