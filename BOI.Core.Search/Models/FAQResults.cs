namespace BOI.Core.Search.Models
{
    public class FAQResults
    {
        public List<FAQResult> QueryResults { get; set; }
    }

    public class FAQResult
    {
        public int Id { get; set; }
        public string NameId { get; set; }
        public string FAQQuestion { get; set; }
        public string FAQAnswer { get; set; }
        public int SortOrder { get; set; }
        public string FAQCategory { get; set; }
        public IEnumerable<FAQTabResult> FAQTabs { get; set; }
    }

    public class FAQTabResult
    {
        public int Id { get; set; }
        public int SortOrder { get; set; }
        public string FAQTabQuestion { get; set; }
        public string FAQAnswer { get; set; }
    }
}
