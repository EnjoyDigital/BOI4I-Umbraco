using BOI.Core.Search.Models;
using BOI.Core.Search.Models.ElasticSearch;
using Microsoft.Extensions.Configuration;
using Nest;

namespace BOI.Core.Search.Factory
{
    public class EsSearchFactory
    {

        private readonly IConfiguration configuration;

        public EsSearchFactory(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        private ElasticSettings esSettings => new(configuration);

        public IElasticClient CreateClient()
        {
            var searchUri = new Uri(esSettings.EsSearchUri);

            var settings = new ConnectionSettings(searchUri)
                .BasicAuthentication(esSettings.EsUsername, esSettings.EsPassword)
                .DefaultMappingFor<WebContent>(i => i.IndexName(esSettings.WebContentEsIndexAlias))

                //TODO: PB TO REPLACE WITH SOLICITOR
                //.DefaultMappingFor<Casualty>(m => m.IndexName(esSettings.CasualtyEsIndexAlias))
                
               ;

            if (esSettings.EsEnableDebugMode)
            {
                settings.EnableDebugMode()
                    .DisableDirectStreaming()
                    .PrettyJson();
            }

            IElasticClient client = new ElasticClient(settings);

            return client;
        }
    }
}
