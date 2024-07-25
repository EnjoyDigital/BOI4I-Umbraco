using Microsoft.AspNetCore.Html;
using System.Web;
using Umbraco.Extensions;

namespace BOI.Core.Search.Extensions
{
    public static class StringExtensions
    {
        public static string ToDescription(this string str, int length, bool addEllipsis)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            var desc = str.StripHtml();
            if (desc.Length < length) return desc;
            var iNextSpace = desc.LastIndexOf(" ", length, StringComparison.Ordinal);
            return string.Format("{0}{1}", desc.Substring(0, (iNextSpace > 0) ? iNextSpace : length).Trim(), addEllipsis ? '…' : char.MinValue);
        }

        public static HtmlString ToDescription(this IHtmlString str, int length, bool addEllipsis)
        {
            return new HtmlString(str == null ? string.Empty : ToDescription(str.ToString(), length, addEllipsis));
        }

        
    }
}
