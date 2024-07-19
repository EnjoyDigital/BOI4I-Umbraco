namespace BOI.Core.Web.Models.ViewModels
{
    public class SitemapXmlItem
    {
        public string ChangeFreq { get; set; }
        public string Priority { get; set; }
        public string Url { get; set; }
        public string LastModified { get; set; }
        public IEnumerable<HrefLangs> Alternates { get; set; }
    }

    public class HrefLangs
    {
        public string IsoCode { get; set; }
        public string Href { get; set; }
    }
}
