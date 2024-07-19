using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace BOI.Core.Services
{
    public interface IOutputCacheService
    {
        bool CacheEnabled();
        TimeSpan CacheTimeSpan(int time = 20);
        bool BlockListCacheDisableCheck(string alias);
        bool WidgetCacheDisableCheck(string alias);
        string GetCurrentCulture();
    }

    public class OutputCacheService : IOutputCacheService
    {

        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;

        public OutputCacheService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
        }

        public TimeSpan CacheTimeSpan(int time = 20)
        {
            return TimeSpan.FromMinutes(time);
        }

        public bool CacheEnabled()
        {
            if (httpContextAccessor.HttpContext != null)
            {
                if (!string.IsNullOrWhiteSpace(httpContextAccessor.HttpContext.Request.Cookies["UMB_PREVIEW"]) ||
                    httpContextAccessor.HttpContext.Request.Cookies["UMB_PREVIEW"] == "preview")
                {
                    return false;
                }
            }

            return !string.IsNullOrWhiteSpace(configuration["Website:SiteCacheEnabled"]) &&
                   Convert.ToBoolean(configuration["Website:SiteCacheEnabled"]);
        }

        public bool BlockListCacheDisableCheck(string alias)
        {
            if (CacheEnabled())
            {
                return alias switch
                {
                    //WidgetPicker.ModelTypeAlias => false,
                    //FormBlock.ModelTypeAlias => false,
                    _ => true
                };
            }

            return false;

        }

        public bool WidgetCacheDisableCheck(string alias)
        {
            if (CacheEnabled())
            {
                return alias switch
                {
                    //Newsletter.ModelTypeAlias => false,
                    _ => true
                };
            }

            return false;
        }

        public string GetCurrentCulture()
        {
            if (httpContextAccessor.HttpContext != null)
            {
                var rqf = httpContextAccessor.HttpContext.Features.Get<IRequestCultureFeature>();
                // Culture contains the information of the requested culture
                return rqf.RequestCulture.Culture.Name;
            }

            return string.Empty;

        }
    }
}
