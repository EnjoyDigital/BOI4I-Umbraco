using BOI.Core.Constants;
using BOI.Core.Search.Constants;
using BOI.Core.Search.Extensions;
using BOI.Core.Extensions;
using BOI.Core.Search.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BOI.Core.Search.Queries.Elastic
{
    public class BdmFinderSearch
    {
        public int Page { get; set; }

        public int Size { get; set; }

        public string Postcode { get; set; }
        public string FCANumber { get; set; }
    }

    public class BdmFinderSearcher : IBdmFinderSearcher
    {
        private readonly IConfiguration configuration;

        private readonly IElasticClient esClient;
        private readonly ILogger<BdmFinderSearcher> logger;

        public BdmFinderSearcher(IConfiguration configuration, IElasticClient esClient, ILogger<BdmFinderSearcher> logger)
        {
            this.configuration = configuration;
            this.esClient = esClient;
            this.logger = logger;
        }



        /// <summary>
        /// BDM search
        /// </summary>
        public BdmResults Execute(BdmFinderSearch model)
        {
            var results = new BdmResults();

            //initial seach will match against registered BDM, TBDM and 'hitachi' (IEL) registered FCA codes
            var search = esClient
                .Search<Bdm>(s => s
                    .Index(configuration[ConfigurationConstants.WebcontentIndexAliasKey])
                    .TrackTotalHits()
                    .Size(model.Size)
                    .Query(q => q
                        .Bool(b => b
                            .Must(a => a.Match(t => t.Field(BdmContactConstants.FCANumber).Query(model.FCANumber)))
                        )
                    )
                    );
            try
            {
                search = search.EnsureSuccess();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "bdm search");
            }

            //This is incase fcacode hasnt been registered with BOI, so match by 'postcode', region(first letter characters) currently 16/04/2023
            if (search.Documents.Count < 1)
            {
                var regioncode = model.Postcode.PostcodeOutCode();
                search = esClient
                .Search<Bdm>(s => s
                    .Index(configuration[ConfigurationConstants.WebcontentIndexAliasKey])
                    .TrackTotalHits()
                    .Size(model.Size)
                    .Query(q => q
                        .Bool(b => b
                            .Must(a => a.Match(t => t.Field(BdmContactConstants.Regions).Query(regioncode)))
                        )
                    )
                    );
            }

            if (search == null)
            {
                return results;
            }

            var query = search.DebugInformation;

            var resultsHits = search.Hits;
            List<BdmResult> queryResults = search.Documents.Select(bdm => new BdmResult(bdm)).ToList();




            results = new BdmResults
            {
                QueryResults = queryResults,
                Total = Convert.ToInt32(search.Total),
                Page = model.Page,
                Size = model.Size,
            };

            return results;
        }
    }
}
