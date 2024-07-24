using Microsoft.AspNetCore.Html;
using System.Web;
using Umbraco.Extensions;

namespace BOI.Core.Search.Extensions
{
    public static class StringExtensions
    {
        public static bool HasValue(this string value)
            => !string.IsNullOrEmpty(value) || !string.IsNullOrWhiteSpace(value);
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

        public static decimal? TryParseDecimal(this string x)
        {
            if (x is null)
            {
                return null;
            }
            decimal value;
            return decimal.TryParse(x, out value) ? value : (decimal?)null;
        }
        /// <summary>
        /// Will return the first part of the postcode
        /// </summary>
        /// <param name="value"></param>
        /// <param name="justLetters"></param>
        /// <returns></returns>
        public static string PostcodeOutCode(this string value, bool justLetters = true)
        {
            if (!value.HasValue())
            {
                return "";
            }
            value = value.Replace(" ", "");
            var lastDigit = justLetters ? value.IndexOfAny("123456789".ToCharArray()) : value.LastIndexOfAny("123456789".ToCharArray());
            if (lastDigit < 0)
            {
                return value;
            }
            return value.Substring(0, lastDigit);
        }

        public static int? TryParseInt32(this string x)
        {
            return int.TryParse(x, out var value) ? value : (int?)null;
        }
    }
}
