using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Umbraco.Extensions;

namespace BOI.Core.Web.Extensions
{
    public static class StringExtensions
    {
        public static string ToUrlSafe(this string value)
            => WebUtility.UrlEncode(value.Replace(" ", "-"));

        public static bool HasValue(this string value)
            => !string.IsNullOrEmpty(value) || !string.IsNullOrWhiteSpace(value);


        public static string MakeValidFileName(this string value)
        {
            string invalidChars = Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return Regex.Replace(value, invalidRegStr, "_");
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

        
        public static int? TryParseInt32(this string x)
        {
            return int.TryParse(x, out var value) ? value : (int?)null;
        }


        public static string GetPostCodeOutCode(this string value)
        {
            if (!value.HasValue())
            {
                return "";
            }
            string postCode = value.Replace(" ", "");
            return postCode.Substring(0, postCode.Length - 3);
        }

        public static string ToSentenceCase(this string input)
        {

            if (input.IsNullOrWhiteSpace()) return string.Empty;

            var lowerCase = input.ToLower();
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lowerCase);

        }

        public static string GetDaySuffix(this int day)
        {
            switch (day)
            {
                case 1:
                case 21:
                case 31:
                    return "st";
                case 2:
                case 22:
                    return "nd";
                case 3:
                case 23:
                    return "rd";
                default:
                    return "th";
            }
        }

        public static string StripHTML(this string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        public static string StripHTMLPTags(this string input)
        {
            input = Regex.Replace(input, "^\\s*<p([^>]*)>", string.Empty);
            return input.Replace( "<\\/p>\\s*$", string.Empty);
        }

        public static string ReplaceAllButFirst(this string originalStr, string search, string replace)
        {
            var stringPosition = originalStr.IndexOf(search, StringComparison.InvariantCultureIgnoreCase);
            if (stringPosition >= 0)
            {
                string str = originalStr.Substring(0, originalStr.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) + search.Length);
                return str + originalStr.Substring(str.Length).Replace(search, replace);
            }
            return originalStr;
        }
    }
}
