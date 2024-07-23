using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.Caching.Memory;

namespace BOI.Core.Services
{
    public interface ICacheTagHelperService
    {
        void ClearCache();
    }

    public class CacheTagHelperService : ICacheTagHelperService
    {
        private readonly CacheTagHelperMemoryCacheFactory cacheTagHelperMemoryCache;

        public CacheTagHelperService(CacheTagHelperMemoryCacheFactory cacheTagHelperMemoryCache)
        {
            this.cacheTagHelperMemoryCache = cacheTagHelperMemoryCache;
        }

        public void ClearCache()
        {
            if (cacheTagHelperMemoryCache.Cache as MemoryCache is { } cache)
            {
                cache.Clear();
            }
        }
    }
}
