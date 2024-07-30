using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BOI.Core.Constants;
using BOI.Core.Extensions;
using BOI.Core.Search.Extensions;
using BOI.Core.Search.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;

namespace BOI.Core.Search.Queries.Elastic
{
    public class SolicitorSearch
    {

        //This structure isnt really neccesary
        //public IEnumerable<SearchFilter> Filters
        //{
        //    get
        //    {
        //        var filters = new List<SearchFilter>();

        //        if (!string.IsNullOrWhiteSpace(SolicitorName))
        //        {
        //            filters.Add(new SearchFilter
        //            {
        //                Name = "SolicitorName",
        //                Label = "SolicitorName",
        //                Value = SolicitorName,
        //            });
        //        }

        //        if (!string.IsNullOrWhiteSpace(Postcode))
        //        {
        //            filters.Add(new SearchFilter
        //            {
        //                Name = "Postcode",
        //                Label = "Postcode",
        //                Value = Postcode,
        //            });
        //        }

        //        var additionalParams = "";

        //        if (Page > 0)
        //        {
        //            additionalParams = $"&Page={Page}";
        //        }

        //        if (Size > 10)
        //        {
        //            additionalParams += $"&Size={Size}";
        //        }

        //        filters = filters.Select(f => f.UpdateQueryString(string.Join("&", filters.Where(ff => ff.Name != f.Name).Select(fs => $"{fs.Name}={fs.Value}")) + additionalParams)).ToList();

        //        return filters;
        //    }
        //}

        public int Page { get; set; }

        public int Size { get; set; }

        [StringLength(200)]
        public string SolicitorName { get; set; }
        public string Postcode { get; set; }

        public float Lon { get; set; }
        public float Lat { get; set; }
    }

    public class SolicitorSearcher : ISolicitorSearcher
    {
        private readonly IConfiguration configuration;

        private readonly IElasticClient esClient;
        private readonly ILogger<SolicitorSearcher> logger;
        private IConfigurationRoot configuration1;

        public SolicitorSearcher(IConfiguration configuration, IElasticClient esClient, ILogger<SolicitorSearcher> logger)
        {
            this.configuration = configuration;
            this.esClient = esClient;
            this.logger = logger;
        }

        public SolicitorsResults Execute(SolicitorSearch model)
        {
            if (model.Size == 0)
            {
                model.Size = 10;
            }

            if (model.Page < 1)
            {
                model.Page = 1;
            }
            var results = new SolicitorsResults();

            var search = esClient
                .Search<Solicitor>(s => s
                    .Index(configuration[ConfigurationConstants.SolicitorIndexAliasKey])
                    .TrackTotalHits()
                    .From((model.Page - 1) * model.Size)
                    .Size(model.Size)
                    .Query(q => q
                        .Bool(b => b
                            .Must(
                                a =>
                                {
                                    if (!string.IsNullOrWhiteSpace(model.SolicitorName))
                                    {
                                        return a.Match(t => t.Field(f => f.SolicitorName).Query(model.SolicitorName).Operator(Operator.And));
                                    }

                                    return null;

                                },
                                a =>
                                {
                                    if (model.Lon != 0 && model.Lat != 0)
                                    {
                                        return a.GeoDistance(g => g
                                            .Field(f => f.Location)
                                            .DistanceType(GeoDistanceType.Arc)
                                            .Location(model.Lat, model.Lon)
                                            .ValidationMethod(GeoValidationMethod.IgnoreMalformed)
                                            .Distance("15mi")
                                        );
                                    }

                                    return null;
                                }
                            )
                        )
                    )
                    .Sort(so =>
                    {
                        if (model.Lon != 0 && model.Lat != 0)
                        {
                            return so.GeoDistance(g => g
                                .Field(f => f.Location)
                                .Unit(DistanceUnit.Miles)
                                .Points(new GeoLocation(model.Lat, model.Lon))
                                .DistanceType(GeoDistanceType.Arc)
                                .IgnoreUnmapped()
                                .Order(SortOrder.Ascending)
                                .Mode(SortMode.Min)
                            );
                        }

                        return null;
                    })
                );

            try
            {
                search = search.EnsureSuccess();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "bdm search");
            }

            if (search == null)
            {
                return results;
            }

            var resultsHits = search.Hits;
            List<SolicitorResult> queryResults;

            if (model.Postcode.HasValue())
            {
                var hits = resultsHits.Select(solicitor =>
                {
                    var miles = " mile";
                    if (Convert.ToDouble(solicitor.Sorts.FirstOrDefault()).ToString("#.#") != "1")
                    {
                        miles = " miles";
                    }

                    var distance = Convert.ToDouble(solicitor.Sorts.FirstOrDefault()).ToString("0.#") + miles;

                    if (Convert.ToDouble(solicitor.Sorts.FirstOrDefault()) > 300)
                    {
                        distance = string.Empty;
                    }

                    return new SolicitorResult()
                    {
                        SolicitorName = solicitor.Source.SolicitorName,
                        Address1 = solicitor.Source.Address1,
                        Address2 = solicitor.Source.Address2,
                        Address3 = solicitor.Source.Address3,
                        Address4 = solicitor.Source.Address4,
                        Address5 = solicitor.Source.Address5,
                        PostCode = solicitor.Source.PostCode,
                        Telephone = solicitor.Source.Telephone,
                        Distance = distance
                    };
                }).ToList();

                queryResults = hits;
            }
            else
            {
                queryResults = search.Documents.Select(r => new SolicitorResult()
                {
                    SolicitorName = r.SolicitorName,
                    Address1 = r.Address1,
                    Address2 = r.Address2,
                    Address3 = r.Address3,
                    Address4 = r.Address4,
                    Address5 = r.Address5,
                    PostCode = r.PostCode,
                    Telephone = r.Telephone,
                    Distance = r.Distance
                }).ToList();
            }

            results = new SolicitorsResults
            {
                QueryResults = queryResults,
                Total = Convert.ToInt32(search.Total),
                Page = model.Page,
                Size = model.Size,
                // Filters = model.Filters
            };

            return results;
        }
    }
}
