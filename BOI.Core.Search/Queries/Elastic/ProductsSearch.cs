using BOI.Core.Constants;
using BOI.Core.Search.Constants;
using BOI.Core.Search.Models;
using BOI.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Nest;
using BOI.Core.Search.Extensions;

namespace BOI.Core.Search.Queries.Elastic
{
    public class ProductsSearch
    {
        public IEnumerable<SearchFilter> Filters
        {
            get
            {
                var filters = new List<SearchFilter>();

                if (!string.IsNullOrWhiteSpace(ProductType) && ProductType != "null")
                {
                    filters.Add(new SearchFilter
                    {
                        Name = "ProductType",
                        Label = "ProductType",
                        Value = ProductType,
                    });
                }

                if (!string.IsNullOrWhiteSpace(ProductTerm) && ProductTerm != "null")
                {
                    filters.Add(new SearchFilter
                    {
                        Name = "ProductTerm",
                        Label = "ProductTerm",
                        Value = ProductTerm,
                    });
                }

                if (!string.IsNullOrWhiteSpace(ProductCategory) && ProductCategory != "null")
                {
                    filters.Add(new SearchFilter
                    {
                        Name = "ProductCategory",
                        Label = "ProductCategory",
                        Value = ProductCategory,
                    });
                }

                if (!string.IsNullOrWhiteSpace(ProductLTV) && ProductLTV != "null")
                {
                    filters.Add(new SearchFilter
                    {
                        Name = "ProductLTV",
                        Label = "ProductLTV",
                        Value = ProductLTV,
                    });
                }

                filters.Add(new SearchFilter
                {
                    Name = "InterestOnly",
                    Label = "InterestOnly",
                    Value = InterestOnly.ToString(),
                });

                return filters;
            }
        }

        public string ProductCategoriesFilter { get; set; }
        public string ProductCategory { get; set; }
        public string ProductTermsFilter { get; set; }
        public string ProductTerm { get; set; }
        public string ProductTypesFilter { get; set; }
        public string ProductType { get; set; }
        public string ProductLTVsFilter { get; set; }
        public string ProductLTV { get; set; }
        public bool InterestOnly { get; set; }
        public string ProductVariant { get; set; }
    }

    public class ProductSearcher
    {
        private readonly IConfiguration configuration;

        private readonly IElasticClient esClient;

        public ProductSearcher(IConfiguration configuration, IElasticClient esClient)
        {
            this.configuration = configuration;
            this.esClient = esClient;
        }

        public ProductsResults Execute(ProductsSearch model)
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
                                    return a.Term(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Value(DocTypeConstants.Product));
                                },
                                a =>
                                {
                                    if (model.InterestOnly)
                                    {
                                        return a.Term(t => t.Field(tf => tf.InterestOnly).Value(model.InterestOnly));
                                    }
                                    return null;
                                },
                                a =>
                                {
                                    return a.Term(t => t.Field(tf => tf.ProductVariant.Suffix("keyword")).Value(model.ProductVariant));
                                },
                                a =>
                                {
                                    return
                                    a.Term(t => t.Field(tf => tf.WithdrawalDateTime.Suffix("keyword")).Value("").Verbatim())
                                    ||
                                    a.DateRange(t => t.Field(tf => tf.WithdrawalDateTime.Suffix("keyword")).GreaterThan(DateTime.Now).Format("yyyy-MM-dd HH:mm:ss"));
                                }
                            )
                        )
                    )
                .Aggregations(agg => agg
                    .Terms("productTypesFilter", t => t.Field(f => f.ProductType.Suffix("keyword")).Size(1000))
                    .Terms("productTermsFilter", t => t.Field(f => f.Term.Suffix("keyword")).Size(1000))
                    .Terms("productCategoriesFilter", t => t.Field(f => f.Category.Suffix("keyword")).Size(1000))
                    .Terms("productLtvFilter", t => t.Field(f => f.LTVFilterText.Suffix("keyword")).Size(1000))

                )
                .PostFilter(pf =>
                {
                    QueryContainer productsFilters = null;

                    if (!string.IsNullOrEmpty(model.ProductTypesFilter) && model.ProductTypesFilter != "null")
                    {
                        productsFilters &= pf.Term(t => t.Field(f => f.ProductType.Suffix("keyword")).Value(model.ProductTypesFilter));
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(model.ProductType) && model.ProductType != "null")
                        {
                            productsFilters &= pf.Term(t => t.Field(f => f.ProductType.Suffix("keyword")).Value(model.ProductType));
                        }
                    }

                    if (!string.IsNullOrEmpty(model.ProductTermsFilter) && model.ProductTermsFilter != "null")
                    {
                        productsFilters &= pf.Term(t => t.Field(f => f.Term.Suffix("keyword")).Value(model.ProductTermsFilter));
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(model.ProductTerm) && model.ProductTerm != "null")
                        {
                            productsFilters &= pf.Term(t => t.Field(f => f.Term.Suffix("keyword")).Value(model.ProductTerm));
                        }
                    }

                    if (!string.IsNullOrEmpty(model.ProductCategoriesFilter) && model.ProductCategoriesFilter != "null")
                    {
                        productsFilters &= pf.Term(t => t.Field(f => f.Category.Suffix("keyword")).Value(model.ProductCategoriesFilter));
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(model.ProductCategory) && model.ProductCategory != "null")
                        {
                            productsFilters &= pf.Term(t => t.Field(f => f.Category.Suffix("keyword")).Value(model.ProductCategory));
                        }
                    }

                    if (!string.IsNullOrEmpty(model.ProductLTVsFilter) && model.ProductLTVsFilter != "null")
                    {
                        productsFilters &= pf.Term(t => t.Field(f => f.LTVFilterText.Suffix("keyword")).Value(model.ProductLTVsFilter));
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(model.ProductLTV) && model.ProductLTV != "null")
                        {
                            productsFilters &= pf.Term(t => t.Field(f => f.LTVFilterText.Suffix("keyword")).Value(model.ProductLTV));
                        }
                    }

                    return productsFilters;
                })
                )
            .EnsureSuccess();

            int term;
            var queryResults = search.Documents.Select(r => new ProductResult()
            {
                ProductType = r.ProductType,
                Category = r.Category,
                LTVFilterText = r.LTVFilterText,
                InterestOnly = r.InterestOnly,
                LTVTitle = r.LTVTitle,
                LaunchDateTime = r.LaunchDateTime,
                IsNew = r.IsNew,
                Term = string.Concat(r.Term, int.TryParse(r.Term, out term) ? r.Term == "1" ? " year" : " years" : " rate"),
                Rate = r.Rate.TryParseDecimal().GetValueOrDefault(0),
                IsFixedRate = r.IsFixedRate,
                Description = r.Description,
                OverallCost = r.OverallCost.TryParseDecimal().GetValueOrDefault(0),
                ProductFees = r.ProductFees.TryParseDecimal().GetValueOrDefault(0),
                Features = r.Features,
                EarlyRepaymentCharges = r.EarlyRepaymentCharges,
                Code = r.Code,
                ProductVariant = r.ProductVariant,
                WithdrawalDateTime = r.WithdrawalDateTime,
                AIPDeadlineDateTime = r.AIPDeadlineDateTime,
                LTVSortOrder = FindLTVSortOrder(r.LTVTitle)
            }).Reverse().ToList();

            var refinefilters = search.Aggregations;

            var typesFilters = refinefilters.Terms("productTypesFilter").Buckets.Where(x => x.Key.HasValue()).Select(c => new RefineFilter
            {
                Label = c.Key,
                Value = c.Key,
                IsChecked = c.Key == model.ProductTypesFilter ? true : false
            }).ToList();

            if (typesFilters.Count > 0)
            {
                typesFilters.Sort((x, y) => string.Compare(x.Value, y.Value));
            }

            var termsFilters = refinefilters.Terms("productTermsFilter").Buckets.Where(x => x.Key.HasValue()).Select(c => new RefineFilter
            {
                Label = c.Key,
                Value = c.Key,
                IsChecked = c.Key == model.ProductTermsFilter ? true : false
            }).ToList();

            if (termsFilters.Count > 0)
            {
                termsFilters.Sort((x, y) => string.Compare(x.Value, y.Value));
            }

            var categoriesFilters = refinefilters.Terms("productCategoriesFilter").Buckets.Where(x => x.Key.HasValue()).Select(c => new RefineFilter
            {
                Label = c.Key,
                Value = c.Key,
                IsChecked = c.Key == model.ProductCategoriesFilter ? true : false
            }).ToList();

            if (categoriesFilters.Count > 0)
            {
                categoriesFilters.Sort((x, y) => string.Compare(x.Value, y.Value));
            }

            var ltvsFilters = refinefilters.Terms("productLtvFilter").Buckets.Where(x => x.Key.HasValue()).Select(c => new RefineFilter
            {
                Label = c.Key,
                Value = c.Key,
                IsChecked = c.Key == model.ProductLTVsFilter ? true : false
            }).ToList();

            if (ltvsFilters.Count > 0)
            {
                ltvsFilters.Sort((x, y) => string.Compare(x.Value, y.Value));
            }

            var results = new ProductsResults
            {
                QueryResults = queryResults,
                Filters = model.Filters,
                RefineFilters = new ProductRefineFilters()
                {
                    ProductTypeList = typesFilters,
                    ProductTermList = termsFilters,
                    ProductCategoryList = categoriesFilters,
                    ProductLTVList = ltvsFilters
                }
            };

            return results;
        }

        public ProductsResults ExecuteWithdrwalProducts(ProductsSearch model)
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
                                    return a.Term(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Value("product"));
                                },
                                a =>
                                {
                                    return a.Term(t => t.Field(tf => tf.ProductVariant.Suffix("keyword")).Value(model.ProductVariant));
                                },
                                a =>
                                {
                                    return a.DateRange(t => t.Field(tf => tf.WithdrawalDateTime.Suffix("keyword")).GreaterThan(DateTime.Now).Format("yyyy-MM-dd HH:mm:ss"));
                                }
                            )
                        )
                    )
                )
            .EnsureSuccess();

            int term;
            var queryResults = search.Documents.Select(r => new ProductResult()
            {
                ProductType = r.ProductType,
                Category = r.Category,
                LTVFilterText = r.LTVFilterText,
                InterestOnly = r.InterestOnly,
                LTVTitle = r.LTVTitle,
                LaunchDateTime = r.LaunchDateTime,
                IsNew = r.IsNew,
                Term = string.Concat(r.Term, int.TryParse(r.Term, out term) ? r.Term == "1" ? " year" : " years" : " rate"),
                Rate = r.Rate.TryParseDecimal().GetValueOrDefault(0),
                IsFixedRate = r.IsFixedRate,
                Description = r.Description,
                OverallCost = r.OverallCost.TryParseDecimal().GetValueOrDefault(0),
                ProductFees = r.ProductFees.TryParseDecimal().GetValueOrDefault(0),
                Features = r.Features,
                EarlyRepaymentCharges = r.EarlyRepaymentCharges,
                Code = r.Code,
                ProductVariant = r.ProductVariant,
                WithdrawalDateTime = r.WithdrawalDateTime,
                AIPDeadlineDateTime = r.AIPDeadlineDateTime,
                LTVSortOrder = FindLTVSortOrder(r.LTVTitle)
            }).Reverse().ToList();

            var results = new ProductsResults
            {
                QueryResults = queryResults
            };

            return results;
        }
        public int FindLTVSortOrder(string ltvName)
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
                                    return a.Term(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Value("productLTV"));
                                },
                                a =>
                                {
                                    return a.Term(t => t.Field(tf => tf.Name.Suffix("keyword")).Value(ltvName));
                                }
                            )
                        )
                    )
                )
            .EnsureSuccess();

            if (search.Documents != null && search.Documents.Any())
                return search.Documents.Select(x => x.SortOrder).FirstOrDefault();
            return 0;
        }
    }
}
