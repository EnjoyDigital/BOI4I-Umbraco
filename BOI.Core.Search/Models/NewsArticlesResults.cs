namespace BOI.Core.Search.Models
{
    public class NewsArticlesResults
    {
        public int Page { get; set; }

        public int Size { get; set; }

        public int Total { get; set; }

        public List<NewsArticleResult> QueryResults { get; set; }
    }

    public class NewsArticleResult
    {
        public string ArticleName { get; set; }
        public string ArticleListingSummary { get; set; }
        public string ArticleUrl { get; set; }
    }
}
