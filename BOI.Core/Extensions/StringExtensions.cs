using Microsoft.AspNetCore.Html;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace BOI.Core.Extensions
{
    public static class StringExtensions
    {
        public static string ToUrlSafe(this string value)
            => WebUtility.UrlEncode(value.Replace(" ", "-"));

        public static bool HasValue(this string value)
            => !string.IsNullOrEmpty(value) || !string.IsNullOrWhiteSpace(value);

       

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

            if (!input.HasValue()) return string.Empty;

            var lowerCase = input.ToLower();
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lowerCase);

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
            
		public static HtmlString SurroundWith(this string value, string insertBefore = "", string insertAfter = "", string fallbackText = "")
		{
			if (!value.HasValue() && !fallbackText.HasValue())
			{
				return new HtmlString("");
			}
			if (!value.HasValue())
			{
				value = fallbackText;
			}
			return new HtmlString(string.Concat(insertBefore, value, insertAfter));
		}
	}
}
