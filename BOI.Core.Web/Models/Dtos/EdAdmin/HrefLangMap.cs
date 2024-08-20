using System;
using System.Collections.Generic;

namespace BOI.Core.Web.Models.Dtos.EdAdmin
{
    public class HrefLangMap
    {
        public HrefLangMap()
        {
            Hrefs = new List<HrefLangModel>();
        }

        public Uri Loc { get; set; }
        public List<HrefLangModel> Hrefs { get; set; }
    }
}
