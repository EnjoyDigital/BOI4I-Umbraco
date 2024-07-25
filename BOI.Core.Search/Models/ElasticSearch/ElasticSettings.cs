using BOI.Core.Constants;
using Microsoft.Extensions.Configuration;

namespace BOI.Core.Search.Models.ElasticSearch
{
    public class ElasticSettings
    {

        private readonly IConfiguration configuration;

        public ElasticSettings(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string EsSearchUri => configuration[ConfigurationConstants.EsSearchUri];
        public string EsUsername => configuration[ConfigurationConstants.EsUsername];
        public string EsPassword => configuration[ConfigurationConstants.EsPassword];
        public bool EsEnableDebugMode => Convert.ToBoolean(configuration[ConfigurationConstants.EsEnableDebugMode]);
        public string WebContentEsIndexAlias => configuration[ConfigurationConstants.WebcontentIndexAliasKey];
        public string SolicitorEsIndexAlias => configuration[ConfigurationConstants.SolicitorIndexAliasKey];
        public string MediaEsIndexAlias => configuration[ConfigurationConstants.MediaLogIndexAlias];
    }
}
