using Microsoft.Extensions.Configuration;

namespace BOI.Core.Search.Models.ElasticSearch
{
    public class ElasticSettings : ElasticIndexes
    {

        private readonly IConfiguration configuration;

        public ElasticSettings(IConfiguration configuration) : base(configuration)
        {
            this.configuration = configuration;
        }

        public string EsSearchUri => configuration["ElasticSettings:EsSearchUri"];

        public string EsUsername => configuration["ElasticSettings:EsUsername"];

        public string EsPassword => configuration["ElasticSettings:EsPassword"];

        public bool EsEnableDebugMode => Convert.ToBoolean(configuration["ElasticSettings:EsEnableDebugMode"]);
    }
}
