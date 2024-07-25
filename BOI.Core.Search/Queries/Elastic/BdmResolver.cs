using BOI.Core.Search.Extensions;
using BOI.Core.Search.Models;
using Microsoft.Extensions.Configuration;
using Nest;
using BOI.Core.Extensions;

namespace BOI.Core.Search.Queries.Elastic
{
    public class BdmResolverQuery
    {
        public string BDMQueryString { get; set; }
    }

    public class BdmResolver : IBdmResolver
    {
        private readonly IConfiguration configuration;

        private readonly IElasticClient esClient;

        private IConfigurationRoot configuration1;

        public BdmResolver(IConfiguration configuration, IElasticClient esClient)
        {
            this.configuration = configuration;
            this.esClient = esClient;
        }

        public BdmResolver(IConfigurationRoot configuration1)
        {
            this.configuration1 = configuration1;
        }

        /// <summary>
        /// Conditional Search for Cemeteries
        /// </summary>
        public BdmResult Execute(BdmResolverQuery query)
        {
            //if (model.Size == 0)
            //{
            //    model.Size = 10;
            //}

            //if (model.Page < 1)
            //{
            //    model.Page = 1;
            //}
            if (!query.BDMQueryString.HasValue())
            {
                return null;
            }

            var search = esClient
                .Search<WebContent>(s => s
                    .Index(configuration["WebContentEsIndexAlias"])
                    .TrackTotalHits()
                    .Size(1)
                    .Query(q => q
                        .MatchPhrase(b => b
                            .Field(
                                 a => a.BDMIdentifier)
                            .Query(query.BDMQueryString))
                          )                    
                    )
                .EnsureSuccess();

            // var queryDebug = search.DebugInformation;

            // var resultsHits = search.Hits;
            var result = search.Documents.FirstOrDefault();
            if (result == null)
            {
                return null;
            }
            return new BdmResult() { ItemId = result.Id };

        }
    }
}
