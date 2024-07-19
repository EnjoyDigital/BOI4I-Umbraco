using BOI.Core.Models;
using Microsoft.Extensions.Configuration;


namespace BOI.Core.Extensions
{
    public static class IConfigurationExtensions
    {
        public static string GetWebsiteSetting(this IConfiguration configuration, string key)
            => configuration["Website:" + key];

        public static bool IsServiceCacheEnabled(this IConfiguration configuration)
            => bool.TryParse(configuration.GetWebsiteSetting("ServiceCache:Enabled"), out bool enabled) ? enabled : false;

        public static IndexProvider GetIndexProvider(this IConfiguration configuration)
        {
            IndexProvider provider = IndexProvider.Examine;
            if (System.Enum.TryParse(configuration.GetWebsiteSetting("IndexingProvider") ?? "", out provider))
            {
                return provider;
            }
            return provider;

        }
    }
}
