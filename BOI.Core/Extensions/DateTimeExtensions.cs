using Microsoft.AspNetCore.Html;


namespace BOI.Core.Web.Extensions
{
    public static class DateTimeExtensions
    {
        public static IHtmlContent ToDateOnlyDateTimeAttribute(this DateTime date)
        {
            return new HtmlString(string.Format(@"datetime=""{0:yyyy-MM-dd}""", date));
        }

        public static IHtmlContent ToDateTimeAttributeNoTimeZone(this DateTime date)
        {
            return new HtmlString(string.Format(@"datetime=""{0:yyyy-MM-DD HH:mm}""", date));
        }

        public static IHtmlContent ToFullDateTime(this DateTime date)
        {
            return new HtmlString(string.Format(@"{0:dd MMM yyyy HH:mm tt}", date));
        }

        public static bool IsMorning(this DateTime value)
        {
            return value.Hour < 12;
        }

        public static bool Between(this DateTime input, DateTime date1, DateTime date2)
        {
            return (input > date1 && input < date2);
        }

        public static string GetOrdinalDateString(this DateTime date)
        {
            return string.Format("{0} {1}", date.GetDayNumber(), date.ToString("MMMM yyyy"));
        }

        public static string GetDayNumber(this DateTime date)
        {
            switch (date.Day)
            {
                case 1:
                case 21:
                case 31:
                    return date.Day + "st";
                case 2:
                case 22:
                    return date.Day + "nd";
                case 3:
                case 23:
                    return date.Day + "rd";
                default:
                    return date.Day + "th";
            }
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
    }
}
