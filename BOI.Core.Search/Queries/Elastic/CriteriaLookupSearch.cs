using BOI.Core.Constants;
using BOI.Core.Extensions;
using BOI.Core.Search.Extensions;
using BOI.Core.Search.Models;
using Microsoft.Extensions.Configuration;
using Nest;
using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace BOI.Core.Search.Queries.Elastic
{
    public class CriteriaLookupSearch
    {
        [StringLength(200)]
        public string CriteriaName { get; set; }
        public string CriteriaCategoryFilter { get; set; }
        public string CriteriaCategory { get; set; }
        public string BuyToLetCriteriaName { get; set; }
        public string BuyToLetCriteriaCategoryFilter { get; set; }
        public string BuyToLetCriteriaCategory { get; set; }
        public bool IsBuyToLet { get; set; }
        public string BespokeCriteriaName { get; set; }
        public string BespokeCriteriaCategoryFilter { get; set; }
        public string BespokeCriteriaCategory { get; set; }
        public bool IsBespoke { get; set; }
    }

    public class CriteriaLookupSearcher
    {
        private readonly IConfiguration configuration;

        private readonly IElasticClient esClient;
        private readonly IShortStringHelper shortStringHelper;
        private const string HightLightPre = "<mark class=\"criteria-highlight\">";
        private const string HightLightPost = "</mark>";
        public CriteriaLookupSearcher(IConfiguration configuration, IElasticClient esClient, IShortStringHelper shortStringHelper)
        {
            this.configuration = configuration;
            this.esClient = esClient;
            this.shortStringHelper = shortStringHelper;
        }

        public AggregateDictionary CriteriaLookupFormValues()
        {
            var results = esClient.Search<WebContent>(s => s
                    .Index(configuration[ConfigurationConstants.WebcontentIndexAliasKey])
                    .Size(0)
                    .Query(q => q
                        .Bool(b => b
                            .Must(
                                a =>
                                {
                                    return a.Term(t => t.Field(tf => tf.ResidentialProduct).Value(true));
                                }
                            )
                        )
                    )
                    .Aggregations(agg => agg
                        .Terms("CriteriaCategory", t => t.Field(f => f.CriteriaCategory.Suffix("keyword")).Size(10000))
                    )
                )
                .EnsureSuccess();

            return results.Aggregations;
        }
        public AggregateDictionary BuyToLetCriteriaLookupFormValues()
        {
            var results = esClient.Search<WebContent>(s => s
                    .Index(configuration[ConfigurationConstants.WebcontentIndexAliasKey])
                    .Size(0)
                    .Query(q => q
                        .Bool(b => b
                            .Must(
                                a =>
                                {
                                    return a.Term(t => t.Field(tf => tf.BuyToLetProduct).Value(true));
                                }
                            )
                        )
                    )
                    .Aggregations(agg => agg
                        .Terms("BuyToLetCriteriaCategory", t => t.Field(f => f.CriteriaCategory.Suffix("keyword")).Size(10000))
                    )
                )
                .EnsureSuccess();

            return results.Aggregations;
        }
        public AggregateDictionary BespokeCriteriaLookupFormValues()
        {
            var results = esClient.Search<WebContent>(s => s
                    .Index(configuration[ConfigurationConstants.WebcontentIndexAliasKey])
                    .Size(0)
                    .Query(q => q
                        .Bool(b => b
                            .Must(
                                a =>
                                {
                                    return a.Term(t => t.Field(tf => tf.BespokeProduct).Value(true));
                                }
                            )
                        )
                    )
                    .Aggregations(agg => agg
                        .Terms("BespokeCriteriaCategory", t => t.Field(f => f.CriteriaCategory.Suffix("keyword")).Size(10000))
                    )
                )
                .EnsureSuccess();

            return results.Aggregations;
        }

        public QueryContainer BuildQueryContainer(CriteriaLookupSearch model)
        {
            var query = new QueryContainerDescriptor<WebContent>();

            var queryContainer =
                (query.Bool(b => b.Must(m => m.Match(t => t.Field(f => f.Name).Query(model.CriteriaName).Boost(20))))
                 ||
                 query.Bool(b => b.Must(m => m.Match(t => t.Field(f => f.Content).Query(model.CriteriaName).Operator(Operator.And).Analyzer("ignore_html_tags")))
                 ))
                &&
                query.Bool(b => b.Must(m => m.Terms(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Terms("criteria", "criteriaTab"))));

            return queryContainer;
        }

        public QueryContainer BuildBuyToLetQueryContainer(CriteriaLookupSearch model)
        {
            var query = new QueryContainerDescriptor<WebContent>();

            var queryContainer =
                (query.Bool(b => b.Must(m => m.Match(t => t.Field(f => f.Name).Query(model.BuyToLetCriteriaName).Boost(20))))
                 ||
                 query.Bool(b => b.Must(m => m.Match(t => t.Field(f => f.Content).Query(model.BuyToLetCriteriaName).Operator(Operator.And).Analyzer("ignore_html_tags"))))
                 )
                &&
                query.Bool(b => b.Must(m => m.Terms(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Terms("criteria", "criteriaTab"))));

            return queryContainer;
        }

        public QueryContainer BuildBespokeQueryContainer(CriteriaLookupSearch model)
        {
            var query = new QueryContainerDescriptor<WebContent>();

            var queryContainer =
                (query.Bool(b => b.Must(m => m.Match(t => t.Field(f => f.Name).Query(model.BespokeCriteriaName).Boost(20))))
                 ||
                 query.Bool(b => b.Must(m => m.Match(t => t.Field(f => f.Content).Query(model.BespokeCriteriaName).Operator(Operator.And).Analyzer("ignore_html_tags"))))
                 )
                &&
                query.Bool(b => b.Must(m => m.Terms(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Terms("criteria", "criteriaTab"))));

            return queryContainer;
        }

        public CriteriaLookupsResults Execute(CriteriaLookupSearch model)
        {
            var search = esClient
                .Search<WebContent>(s => s
                    .Index(configuration[ConfigurationConstants.WebcontentIndexAliasKey])
                    .TrackTotalHits()
                    .Size(10000)
                    .Query(q => BuildQueryContainer(model))
                    .Aggregations(agg => agg
                        .Terms("criteriaCategoryFilter", t => t.Field(f => f.CriteriaCategory.Suffix("keyword")).Size(1000))
                    )
                    .PostFilter(pf =>
                    {
                        QueryContainer criteriaCategoryFilters = null;

                        if (!string.IsNullOrEmpty(model.CriteriaCategoryFilter) && model.CriteriaCategoryFilter != "null")
                        {
                            criteriaCategoryFilters &= pf.Term(t => t.Field(f => f.CriteriaCategory.Suffix("keyword")).Value(model.CriteriaCategoryFilter));
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(model.CriteriaCategory) && model.CriteriaCategory != "null")
                            {
                                criteriaCategoryFilters &= pf.Term(t => t.Field(f => f.CriteriaCategory.Suffix("keyword")).Value(model.CriteriaCategory));
                            }
                        }

                        return criteriaCategoryFilters;
                    }
                    )
                    .Highlight(x => x.Fields(y => y.Field(f => f.Content).NumberOfFragments(1).Type(HighlighterType.Plain).FragmentSize(10000).PreTags(HightLightPre).PostTags(HightLightPost)))
                    .Sort(so => so.Descending(f => f.SortOrder))
                )
                .EnsureSuccess();
            ParseHighLights(search, "content");

            var queryResults = GetCriteriaResutls(search);

            var refinefilters = search.Aggregations;

            var categoryFilters = refinefilters.Terms("criteriaCategoryFilter").Buckets.Where(x => x.Key.HasValue()).Select(c => new RefineFilter
            {
                Label = c.Key,
                Value = c.Key,
                IsChecked = c.Key == model.CriteriaCategoryFilter ? true : false
            }).ToList();

            if (categoryFilters.Count > 0)
            {
                categoryFilters.Sort((x, y) => string.Compare(x.Value, y.Value));
            }

            var results = new CriteriaLookupsResults
            {
                QueryResults = queryResults
            };

            return results;
        }

        public CriteriaLookupsResults BuyToLetExecute(CriteriaLookupSearch model)
        {
            var search = esClient
                .Search<WebContent>(s => s
                    .Index(configuration[ConfigurationConstants.WebcontentIndexAliasKey])
                    .TrackTotalHits()
                    .Size(10000)
                    .Query(q => BuildBuyToLetQueryContainer(model))
                    .Aggregations(agg => agg
                        .Terms("buyToLetCriteriaCategoryFilter", t => t.Field(f => f.CriteriaCategory.Suffix("keyword")).Size(1000))
                    )
                    .PostFilter(pf =>
                    {
                        QueryContainer criteriaCategoryFilters = null;

                        if (!string.IsNullOrEmpty(model.BuyToLetCriteriaCategoryFilter) && model.BuyToLetCriteriaCategoryFilter != "null")
                        {
                            criteriaCategoryFilters &= pf.Term(t => t.Field(f => f.CriteriaCategory.Suffix("keyword")).Value(model.BuyToLetCriteriaCategoryFilter));
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(model.BuyToLetCriteriaCategory) && model.BuyToLetCriteriaCategory != "null")
                            {
                                criteriaCategoryFilters &= pf.Term(t => t.Field(f => f.CriteriaCategory.Suffix("keyword")).Value(model.BuyToLetCriteriaCategory));
                            }
                        }

                        return criteriaCategoryFilters;
                    }
                    )
                    .Highlight(x => x.Fields(y => y.Field(f => f.Content).NumberOfFragments(1).Type(HighlighterType.Plain).FragmentSize(10000).PreTags(HightLightPre).PostTags(HightLightPost)))
                    .Sort(so => so.Descending(f => f.SortOrder))
                )
                .EnsureSuccess();
            ParseHighLights(search, "content");

            var queryResults = GetCriteriaResutls(search);

            var refinefilters = search.Aggregations;

            var categoryFilters = refinefilters.Terms("buyToLetCriteriaCategoryFilter").Buckets.Where(x => x.Key.HasValue()).Select(c => new RefineFilter
            {
                Label = c.Key,
                Value = c.Key,
                IsChecked = c.Key == model.BuyToLetCriteriaCategoryFilter ? true : false
            }).ToList();

            if (categoryFilters.Count > 0)
            {
                categoryFilters.Sort((x, y) => string.Compare(x.Value, y.Value));
            }

            var results = new CriteriaLookupsResults
            {
                QueryResults = queryResults,
            };

            return results;

        }

        public CriteriaLookupsResults BespokeExecute(CriteriaLookupSearch model)
        {
            var search = esClient
                .Search<WebContent>(s => s
                    .Index(configuration[ConfigurationConstants.WebcontentIndexAliasKey])
                    .TrackTotalHits()
                    .Size(10000)
                    .Query(q => BuildBespokeQueryContainer(model))
                    .Aggregations(agg => agg
                        .Terms("bespokeCriteriaCategoryFilter", t => t.Field(f => f.CriteriaCategory.Suffix("keyword")).Size(1000))
                    )
                    .PostFilter(pf =>
                    {
                        QueryContainer criteriaCategoryFilters = null;

                        if (!string.IsNullOrEmpty(model.BespokeCriteriaCategoryFilter) && model.BespokeCriteriaCategoryFilter != "null")
                        {
                            criteriaCategoryFilters &= pf.Term(t => t.Field(f => f.CriteriaCategory.Suffix("keyword")).Value(model.BespokeCriteriaCategoryFilter));
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(model.BespokeCriteriaCategory) && model.BespokeCriteriaCategory != "null")
                            {
                                criteriaCategoryFilters &= pf.Term(t => t.Field(f => f.CriteriaCategory.Suffix("keyword")).Value(model.BespokeCriteriaCategory));
                            }
                        }

                        return criteriaCategoryFilters;
                    }
                    )
                    .Highlight(x => x.Fields(y => y.Field(f => f.Content).NumberOfFragments(1).Type(HighlighterType.Plain).FragmentSize(10000).PreTags(HightLightPre).PostTags(HightLightPost)))
                    .Sort(so => so.Descending(f => f.SortOrder))
                )
                .EnsureSuccess();
            ParseHighLights(search, "content");

            var queryResults = GetCriteriaResutls(search);

            var refinefilters = search.Aggregations;

            var categoryFilters = refinefilters.Terms("bespokeCriteriaCategoryFilter").Buckets.Where(x => x.Key.HasValue()).Select(c => new RefineFilter
            {
                Label = c.Key,
                Value = c.Key,
                IsChecked = c.Key == model.BespokeCriteriaCategoryFilter ? true : false
            }).ToList();

            if (categoryFilters.Count > 0)
            {
                categoryFilters.Sort((x, y) => string.Compare(x.Value, y.Value));
            }

            var results = new CriteriaLookupsResults
            {
                QueryResults = queryResults,
            };

            return results;

        }

        private List<CriteriaLookupResult> GetCriteriaResutls(ISearchResponse<WebContent> search)
        {
            var parentCriteriaList = new List<CriteriaLookupResult>();
            if (search.Documents.Any(x => x.NodeTypeAlias.Equals("criteriaTab", StringComparison.InvariantCultureIgnoreCase)))
            {
                var criteriaTabDocs = search.Documents.Where(x => x.NodeTypeAlias.Equals("criteriaTab", StringComparison.InvariantCultureIgnoreCase)).GroupBy(y => y.ParentNodeId);

                foreach (var criteriaTabDoc in criteriaTabDocs)
                {
                    var criteriaName = criteriaTabDoc.Key;
                    var parentCriteria = esClient.Get<WebContent>(criteriaName, i => i.Index(configuration[ConfigurationConstants.WebcontentIndexAliasKey]));

                    var criteriaTabs = search.Documents.Where(x => x.ParentNodeId == int.Parse(parentCriteria.Id)).Select(r => new CriteriaTabResult()
                    {
                        Id = r.Id,
                        CriteriaTabName = r.Name,
                        SortOrder = r.SortOrder,
                        BodyText = r.BodyText,
                        CriteriaTabUpdatedDate = r.CriteriaTabUpdateDate
                    }).ToList();

                    parentCriteriaList.Add(new CriteriaLookupResult()
                    {
                        Id = parentCriteria.Source.Id,
                        NameId = parentCriteria.Source.Name.ToCleanString(shortStringHelper,CleanStringType.UrlSegment),
                        CriteriaName = parentCriteria.Source.CriteriaName,
                        SortOrder = parentCriteria.Source.SortOrder,
                        CriteriaCategory = parentCriteria.Source.CriteriaCategory,
                        BodyText = parentCriteria.Source.BodyText,
                        IsBuyToLetProduct = parentCriteria.Source.BuyToLetProduct,
                        IsResidentialProduct = parentCriteria.Source.ResidentialProduct,
                        IsBespokeProduct = parentCriteria.Source.BespokeProduct,
                        CriteriaTabs = criteriaTabs,
                        CriteriaUpdatedDate = parentCriteria.Source.CriteriaUpdateDate
                    });
                }
            }

            var queryResults = new List<CriteriaLookupResult>();
            var nonCriteriaTabs = search.Documents.Where(x => !x.NodeTypeAlias.Equals("criteriaTab", StringComparison.InvariantCulture));
            foreach (var nonCriteriaTab in nonCriteriaTabs)
            {
                var criteriaTabs = search.Documents.Where(x => x.ParentNodeId == nonCriteriaTab.Id).Select(r => new CriteriaTabResult()
                {
                    Id = r.Id,
                    CriteriaTabName = r.Name,
                    SortOrder = r.SortOrder,
                    BodyText = r.BodyText,
                    CriteriaTabUpdatedDate = r.CriteriaTabUpdateDate
                }).ToList();

                queryResults.Add(new CriteriaLookupResult()
                {
                    Id = nonCriteriaTab.Id,
                    NameId = nonCriteriaTab.Name.ToCleanString(shortStringHelper, CleanStringType.UrlSegment),
                    CriteriaName = nonCriteriaTab.CriteriaName,
                    SortOrder = nonCriteriaTab.SortOrder,
                    CriteriaCategory = nonCriteriaTab.CriteriaCategory,
                    BodyText = nonCriteriaTab.BodyText,
                    IsBuyToLetProduct = nonCriteriaTab.BuyToLetProduct,
                    IsResidentialProduct = nonCriteriaTab.ResidentialProduct,
                    IsBespokeProduct = nonCriteriaTab.BespokeProduct,
                    CriteriaTabs = criteriaTabs,
                    CriteriaUpdatedDate = nonCriteriaTab.CriteriaUpdateDate
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
                                content.BodyText = string.Join("", search);
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<CriteriaTabResult> SearchCriteriaTabs(int parentNodeId)
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
                                                return a.Term(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Value("criteriaTab"));
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


            var queryResults = search.Documents.Select(r => new CriteriaTabResult()
            {
                Id = r.Id,
                CriteriaTabName = r.Name,
                SortOrder = r.SortOrder,
                BodyText = r.BodyText,
                CriteriaTabUpdatedDate = r.CriteriaTabUpdateDate
            }).ToList();

            return queryResults;
        }
    }
}
