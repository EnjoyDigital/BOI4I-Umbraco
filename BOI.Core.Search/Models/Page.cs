namespace BOI.Core.Search.Models
{
    public class Page<T>
    {
        public Page()
        {
            Items = Enumerable.Empty<T>();
            Links = Enumerable.Empty<PaginationLink>();
        }

        public Page(IEnumerable<T> resultItems, long totalItems, int currentPage, int pagesize, string activeClass)
        {
            Limit = 5;
            Items = resultItems;
            TotalItems = totalItems;
            CurrentPage = currentPage;
            ItemsPerPage = pagesize;
            Links = Enumerable.Empty<PaginationLink>();
            ActiveClass = activeClass;
        }

        public Page(IEnumerable<T> resultItems, long totalItems, string activeClass)
        {
            Limit = 5;
            Items = resultItems;
            TotalItems = totalItems;
            Links = Enumerable.Empty<PaginationLink>();
            ActiveClass = activeClass;
        }

        public string ActiveClass { get; private set; }

        public int Limit { get; private set; }

        public long TotalItems { get; private set; }

        public int ItemsPerPage { get; private set; }

        public int CurrentPage { get; private set; }

        public IEnumerable<T> Items { get; private set; }

        public int PageCount
        {
            get { return (int)Math.Ceiling(TotalItems / (decimal)ItemsPerPage); }
        }

        public bool HasPages
        {
            get { return PageCount > 1; }
        }

        public bool IsFirstPage
        {
            get { return CurrentPage <= 1; }
        }

        public bool IsLastPage
        {
            get { return CurrentPage >= PageCount; }
        }

        public IEnumerable<PaginationLink> Links { get; private set; }

        public PaginationLink FirstPageUrl { get; set; }
        public PaginationLink PrevPageUrl { get; set; }
        public PaginationLink NextPageUrl { get; set; }
        public PaginationLink LastPageUrl { get; set; }
    }
    public class PaginationLink
    {
        public string Class { get; set; }
        public string Href { get; set; }
        public string Text { get; set; }
        public int Page { get; set; }
    }
}
