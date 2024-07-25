using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Net;

namespace BOI.Core.Extensions
{
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Determines whether the specified HTTP request is an AJAX request.
        /// https://stackoverflow.com/questions/29282190/where-is-request-isajaxrequest-in-asp-net-core-mvc
        /// </summary>
        /// 
        /// <returns>
        /// true if the specified HTTP request is an AJAX request; otherwise, false.
        /// </returns>
        /// <param name="request">The HTTP request.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="request"/> parameter is null (Nothing in Visual Basic).</exception>
        public static bool IsAjaxRequest(this HttpRequest request)
        {

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Headers != null)
                return request.Headers["X-Requested-With"] == "XMLHttpRequest";

            return false;
        }

        public static string GetFullUrlWithQueryString(this HttpRequest httpRequest)
        {
            return $"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.Path}{httpRequest.QueryString}";
        }


        public static Uri GetCurrentUriFromRequest(this HttpRequest httpRequest)
        {

            var currentUrl = httpRequest.GetDisplayUrl();
            Uri currentUri = null;
            UriCreationOptions options = new UriCreationOptions();

            if (Uri.TryCreate(currentUrl, in options, out currentUri))
            {
                return currentUri;
            }

            return null;
        }

        public static string GetCurrentDomainFromRequest(this HttpRequest httpRequest)
        {

            var currentUri = httpRequest.GetCurrentUriFromRequest();


            return currentUri?.GetLeftPart(UriPartial.Authority); ;
        }


        public static string GetDomain(this HttpRequest httpRequest, bool domainSafehost = false)
        {
            string text = httpRequest?.GetEncodedUrl();
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            if (domainSafehost)
            {
                return new Uri(text).DnsSafeHost;
            }

            return new Uri(text).GetLeftPart(UriPartial.Authority);

        }

        public static Uri GetReferer(this HttpRequest httpRequest)
        {
            return httpRequest.GetTypedHeaders()?.Referer;
        }

        public static T GetItemFromContext<T>(this HttpRequest httpRequest, string key)
        {
            var requestItem = httpRequest.HttpContext.Items[key];

            if (requestItem is T)
            {
                return (T)requestItem;
            }

            return default;
        }
    }
}
