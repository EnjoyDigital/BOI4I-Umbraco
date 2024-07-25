using BOI.Core.Web.Extensions;

namespace BOI.Umbraco.Models
{
    public partial class Article
    {
        public string FormattedDate
        {
            get
            {
                if (ArticleDate != null)
                {
                    var dayOfWeek = ArticleDate.DayOfWeek.ToString();
                    var day = ArticleDate.Day + ArticleDate.Day.GetDaySuffix();
                    var month = ArticleDate.ToString("MMMM");
                    var year = ArticleDate.Year;

                    return $"{dayOfWeek} {day} {month} {year}";
                }

                return null;
            }
        }
    }

}
