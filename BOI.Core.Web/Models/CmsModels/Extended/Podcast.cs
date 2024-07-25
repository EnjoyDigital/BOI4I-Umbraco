using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankOfIreland.Intermediaries.Core.Web.Extensions;

namespace BankOfIreland.Intermediaries.Core.Web.Models.CmsModels
{
    public partial class Podcast
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
