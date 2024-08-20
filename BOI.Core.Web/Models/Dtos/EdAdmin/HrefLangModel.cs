using System;

namespace BOI.Core.Web.Models.Dtos.EdAdmin
{
    public class HrefLangModel
    {
        public string Hreflang { get; set; }
        public Uri Href { get; set; }
        public string NcContentTypeAlias = "hreflangAttributes";

        public string Name
        {
            get { return "Hreflang for " + Hreflang; }
        }
    }
}
