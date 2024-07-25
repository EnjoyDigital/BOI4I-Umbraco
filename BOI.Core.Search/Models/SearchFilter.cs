namespace BOI.Core.Search.Models
{

    public class SearchFilter
    {
        public string Name { get; set; }

        public string Label { get; set; }

        public string Value { get; set; }

        public string FriendlyValue { get; set; }

        public string QueryString { get; set; }

        public SearchFilter UpdateQueryString(string query)
        {
            QueryString = query;

            return this;
        }
    }
    public class RefineFilter : SearchFilter
    {
        public bool IsChecked { get; set; }
    }
}