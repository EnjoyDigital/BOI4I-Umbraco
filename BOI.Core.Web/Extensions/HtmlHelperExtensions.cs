using BOI.Core.Search.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
namespace BOI.Core.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static string GetPageNumberUrl(this IHtmlHelper htmlHelper, object targetPageNumber)
            => string.Concat("?", string.Concat(BaseQueryAliases.Page, "=", targetPageNumber));

        public static string SetQueryString(this IHtmlHelper htmlHelper, HttpContext httpContext, string targetKey, object value)
        {
            var currentUrl = httpContext.Request.GetFullUrlWithQueryString();


            if (httpContext.Request.Query.ContainsKey(targetKey))
            {
                var uri = new Uri(currentUrl);
                var baseUri = uri.GetLeftPart(UriPartial.Path);
                var query = QueryHelpers.ParseQuery(uri.Query);
                var items = query.SelectMany(x => x.Value, (col, value) => new KeyValuePair<string, string>(col.Key, value)).ToList();
                items.RemoveAll(x => x.Key == targetKey);

                var qb = new QueryBuilder(items);
                qb.Add(targetKey, value.ToString());

                return $"{baseUri}{qb.ToQueryString()}";
            }
            else
            {
                return Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(currentUrl, targetKey, value.ToString());
            }

        }
    }
}
