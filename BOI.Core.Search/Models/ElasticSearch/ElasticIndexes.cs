using BOI.Core.Constants;
using Microsoft.Extensions.Configuration;

namespace BOI.Core.Search.Models.ElasticSearch
{
    public class ElasticIndexes
    {
        private readonly IConfiguration configuration;

        public ElasticIndexes(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string WebContentEsIndexAlias => configuration[ConfigurationConstants.WebcontentIndexAliasKey];
        public string SolicitorEsIndexAlias => configuration[ConfigurationConstants.SolicitorIndexAliasKey];
        public string MediaEsIndexAlias => configuration[ConfigurationConstants.MediaLogIndexAlias];

        
    }

}
