using BOI.Core.Constants;
using BOI.Core.Extensions;
using BOI.Core.Search.Constants;
using BOI.Core.Search.Extensions;
using BOI.Core.Search.Models;
using BOI.Umbraco.Models;
using Microsoft.Extensions.Configuration;
using Nest;
using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace BOI.Core.Search.Queries.Elastic
{
    public class FAQSearch
    {
        [StringLength(200)]
        public string FAQQuestion { get; set; }
        public string FAQCategoryFilter { get; set; }
        public string FAQCategory { get; set; }
    }

    public class FAQSearcher : IFAQSearcher
    {
        private readonly IConfiguration configuration;

        private readonly IElasticClient esClient;
        private readonly IShortStringHelper shortStringHelper;
        private const string HightLightPre = "<mark class=\"criteria-highlight\">";
        private const string HightLightPost = "</mark>";
        public FAQSearcher(IConfiguration configuration, IElasticClient esClient, IShortStringHelper shortStringHelper)
        {
            this.configuration = configuration;
            this.esClient = esClient;
            this.shortStringHelper = shortStringHelper;
        }

        public AggregateDictionary FAQFormValues()
        {
            string faqField = string.Empty;
            var results = esClient.Search<WebContent>(s => s
                    .Index(configuration[ConfigurationConstants.WebcontentIndexAliasKey])
                    .Size(0)
                    .Query(q => q
                        .Bool(b => b
                            .Must(
                                a =>
                                {
                                    return a.Terms(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Terms(FAQ.ModelTypeAlias));
                                }
                            )
                        )
                    )
                    .Aggregations(agg => agg.Terms("FaqCategory", t => t.Field(f => f.FaqCategory.Suffix("keyword")).Size(10000))
                    ))
                .EnsureSuccess();

            return results.Aggregations;
        }

        public QueryContainer BuildQueryContainer(string faqQuestion)
        {
            var query = new QueryContainerDescriptor<WebContent>();
            QueryContainer queryContainer = null;

            queryContainer = (query.Bool(b => b.Must(m => m.Match(t => t.Field(f => f.Name).Query(faqQuestion).Boost(20))))
                     || query.Bool(b => b.Must(m => m.Match(t => t.Field(f => f.Content).Query(faqQuestion).Operator(Operator.And).Analyzer("ignore_html_tags"))))
                     || query.Bool(b => b.Must(m => m.Match(t => t.Field(f => f.FaqKeywords).Query(faqQuestion).Operator(Operator.And).Analyzer("ignore_html_tags")))))
                     && query.Bool(b => b.Must(m => m.Terms(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Terms(FAQ.ModelTypeAlias, FAqtab.ModelTypeAlias))));

            return queryContainer;
        }

        public IAggregationContainer BuildAggregationContainer(string faqFilterName)
        {
            var query = new AggregationContainerDescriptor<WebContent>();

            var queryContainer =
                query.Terms(faqFilterName, t => t.Field(f => f.FaqCategory.Suffix("keyword")).Size(1000));

            return queryContainer;
        }

        public QueryContainer BuildPostFilterContainer(string faqCategoryFilter, string faqCategory)
        {
            var query = new QueryContainerDescriptor<WebContent>();

            QueryContainer faqCategoryFilters = null;

            if (!string.IsNullOrEmpty(faqCategoryFilter) && faqCategoryFilter != "null")
            {
                faqCategoryFilters &= query.Term(t => t.Field(f => f.FaqCategory.Suffix("keyword")).Value(faqCategoryFilter));
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(faqCategory) && faqCategory != "null")
                {
                    faqCategoryFilters &= query.Term(t => t.Field(f => f.FaqCategory.Suffix("keyword")).Value(faqCategory));
                }
            }

            return faqCategoryFilters;
        }

        public FAQResults ExecuteFAQ(FAQSearch model)
        {
            string faqQuestion = string.Empty, faqCategoryFilterName = string.Empty, faqCategoryFilter = string.Empty, faqCategory = string.Empty;

            faqQuestion = model.FAQQuestion;
            faqCategoryFilterName = "faqCategoryFilter";
            faqCategoryFilter = model.FAQCategoryFilter;
            faqCategory = model.FAQCategory;

            var esQuery = new SearchDescriptor<WebContent>().Index(configuration[ConfigurationConstants.WebcontentIndexAliasKey])
                    .TrackTotalHits()
                    .Size(10000)
                    .Query(q => BuildQueryContainer(faqQuestion));

            var stream = new MemoryStream();
            esClient.RequestResponseSerializer.Serialize(esQuery, stream);
            var jsonQuery = System.Text.Encoding.UTF8.GetString(stream.ToArray());

            var search = esClient
                .Search<WebContent>(s => s
                    .Index(configuration[ConfigurationConstants.WebcontentIndexAliasKey])
                    .TrackTotalHits()
                    .Size(10000)
                    .Query(q => BuildQueryContainer(faqQuestion))
                    .Aggregations(agg => BuildAggregationContainer(faqCategoryFilterName))
                    .PostFilter(pf => BuildPostFilterContainer(faqCategoryFilter, faqCategory))
                    .Highlight(x => x.Fields(y => y.Field(f => f.Content).NumberOfFragments(1).Type(HighlighterType.Plain).FragmentSize(10000).PreTags(HightLightPre).PostTags(HightLightPost)))
                    .Sort(so => so.Descending(f => f.SortOrder))
                )
                .EnsureSuccess();
            ParseHighLights(search, "content");

            var queryResults = GetCriteriaResutls(search);

            var refinefilters = search.Aggregations;

            var categoryFilters = refinefilters.Terms(faqCategoryFilterName).Buckets.Where(x => x.Key.HasValue()).Select(c => new RefineFilter
            {
                Label = c.Key,
                Value = c.Key,
                IsChecked = c.Key == faqCategoryFilter ? true : false
            }).ToList();

            if (categoryFilters.Count > 0)
            {
                categoryFilters.Sort((x, y) => string.Compare(x.Value, y.Value));
            }

            var results = new FAQResults
            {
                QueryResults = queryResults
            };

            return results;
        }

        private List<FAQResult> GetCriteriaResutls(ISearchResponse<WebContent> search)
        {
            var parentCriteriaList = new List<FAQResult>();
            if (search.Documents.Any(x => x.NodeTypeAlias.Equals(FAqtab.ModelTypeAlias, StringComparison.InvariantCultureIgnoreCase)))
            {
                var faqTabDocs = search.Documents.Where(x => x.NodeTypeAlias.Equals(FAqtab.ModelTypeAlias, StringComparison.InvariantCultureIgnoreCase)).GroupBy(y => y.ParentNodeId);

                foreach (var faqTabDoc in faqTabDocs)
                {
                    var faqQuestion = faqTabDoc.Key;
                    var parentCriteria = esClient.Get<WebContent>(faqQuestion, i => i.Index(configuration[ConfigurationConstants.WebcontentIndexAliasKey]));

                    var faqTabs = search.Documents.Where(x => x.ParentNodeId == int.Parse(parentCriteria.Id)).Select(r => new FAQTabResult()
                    {
                        Id = r.Id,
                        FAQTabQuestion = r.Name,
                        SortOrder = r.SortOrder,
                        FAQAnswer = r.FaqAnswer,
                    }).ToList();

                    parentCriteriaList.Add(new FAQResult()
                    {
                        Id = parentCriteria.Source.Id,
                        NameId = parentCriteria.Source.Name.ToCleanString(shortStringHelper, CleanStringType.UrlSegment),
                        FAQQuestion = parentCriteria.Source.FaqQuestion,
                        SortOrder = parentCriteria.Source.SortOrder,
                        FAQCategory = parentCriteria.Source.FaqCategory,
                        FAQAnswer = parentCriteria.Source.FaqAnswer,
                        FAQTabs = faqTabs
                    });
                }
            }

            var queryResults = new List<FAQResult>();
            var nonFaqTabs = search.Documents.Where(x => !x.NodeTypeAlias.Equals(FAqtab.ModelTypeAlias, StringComparison.InvariantCulture));
            foreach (var nonFaqTab in nonFaqTabs)
            {
                var faqTabs = search.Documents.Where(x => x.ParentNodeId == nonFaqTab.Id).Select(r => new FAQTabResult()
                {
                    Id = r.Id,
                    FAQTabQuestion = r.FaqQuestion,
                    SortOrder = r.SortOrder,
                    FAQAnswer = r.FaqAnswer
                }).ToList();

                queryResults.Add(new FAQResult()
                {
                    Id = nonFaqTab.Id,
                    NameId = nonFaqTab.Name.ToCleanString(shortStringHelper, CleanStringType.UrlSegment),
                    FAQQuestion = nonFaqTab.FaqQuestion,
                    SortOrder = nonFaqTab.SortOrder,
                    FAQCategory = nonFaqTab.FaqCategory,
                    FAQAnswer = nonFaqTab.FaqAnswer,
                    FAQTabs = faqTabs
                });
            }

            var uniqueCriteriaParent = parentCriteriaList.Where(x => queryResults.All(y => y.Id != x.Id));
            queryResults.AddRange(uniqueCriteriaParent);

            return queryResults;
        }

        public void ParseHighLights(ISearchResponse<WebContent> response, string key)
        {
            var hits = response.Hits.Select(x => x.Highlight).Where(y => y.Count() > 0);
            foreach (var content in response.Documents)
            {
                if (response.Hits.Select(x => x.Id).Any(x => int.Parse(x) == content.Id))
                {
                    var highlight = response.Hits.Where(x => int.Parse(x.Id) == content.Id).FirstOrDefault().Highlight;
                    foreach (var search in highlight.Values)
                    {
                        foreach (var highlightResponse in highlight.Keys)
                        {
                            if (highlightResponse.Equals(key, StringComparison.OrdinalIgnoreCase))
                            {
                                content.FaqAnswer = string.Join("", search);
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<FAQTabResult> SearchFAQTabs(int parentNodeId)
        {
            var search = esClient
                            .Search<WebContent>(s => s
                                .Index(configuration[ConfigurationConstants.WebcontentIndexAliasKey])
                                .TrackTotalHits()
                                .Size(10000)
                                .Query(q => q
                                    .Bool(b => b
                                        .Must(
                                            a =>
                                            {
                                                return a.Term(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Value(FAqtab.ModelTypeAlias));
                                            },
                                            a =>
                                            {
                                                return a.Match(t => t.Field(f => f.ParentNodeId).Query(parentNodeId.ToString()).Operator(Operator.And));
                                            }
                                        )
                                    )
                                )
                                .Sort(so =>
                                {
                                    return so.Ascending(f => f.SortOrder);
                                }
                                )
                            );

            try
            {
                search = search.EnsureSuccess();
            }
            catch (Exception ex)
            {
                //ex.Log();
            }


            var queryResults = search.Documents.Select(r => new FAQTabResult()
            {
                Id = r.Id,
                FAQTabQuestion = r.Name,
                SortOrder = r.SortOrder,
                FAQAnswer = r.FaqAnswer
            }).ToList();

            return queryResults;
        }
    }
}
