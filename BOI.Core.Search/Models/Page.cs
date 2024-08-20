using BOI.Core.Search.Constants;

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
            //GenerateLinks();
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

        //public static implicit operator Page<T>(Page<MainSearchResult> v)
        //{
        //    return v;
        //}

        //public void GenerateLinks()
        //{
        //    var links = new List<PaginationLink>();
        //    var wrapper = new HttpRequestWrapper(HttpContext.Current.Request);

        //    FirstPageUrl = new PaginationLink()
        //    {
        //        Class = IsFirstPage ? "disabled" : "",
        //        Href = (CurrentPage > 1) ? wrapper.SetQuerystringKey(BaseQueryAliases.Page, 0, true) : "",
        //    };

        //    PrevPageUrl = new PaginationLink()
        //    {
        //        Class = IsFirstPage ? "disabled" : "",
        //        Href = (CurrentPage > 1) ? wrapper.SetQuerystringKey(BaseQueryAliases.Page, CurrentPage - 1, true) : "",
        //    };

        //    if (HasPages)
        //    {
        //        if (PageCount > Limit)
        //        {
        //            if (CurrentPage >= Limit && CurrentPage < (PageCount - Limit))
        //            {
        //                links.Add(new PaginationLink { Page = 1, Href = wrapper.RemoveQuerystringKey(BaseQueryAliases.Page), Text = "1" });
        //                links.Add(new PaginationLink { Page = 0, Href = string.Empty });
        //                for (int i = CurrentPage - 1; i < CurrentPage + 2; i++)
        //                {
        //                    var activeClass = (CurrentPage == i) ? ActiveClass : "";
        //                    var pathAndQuery = (i == 1) ? wrapper.RemoveQuerystringKey(BaseQueryAliases.Page) : wrapper.SetQuerystringKey(BaseQueryAliases.Page, i, true);
        //                    if (i != 1 && i != PageCount)
        //                    {
        //                        links.Add(new PaginationLink { Page = i, Href = pathAndQuery, Class = activeClass, Text = i.ToString() });
        //                    }
        //                }
        //                links.Add(new PaginationLink { Page = 0, Href = string.Empty });
        //                links.Add(new PaginationLink { Page = PageCount, Href = wrapper.SetQuerystringKey(BaseQueryAliases.Page, PageCount, true), Text = PageCount.ToString() });
        //            }
        //            else if (CurrentPage < Limit)
        //            {
        //                for (int i = 1; i <= Limit; i++)
        //                {
        //                    var activeClass = (CurrentPage == i) ? ActiveClass : "";
        //                    var pathAndQuery = (i == 1) ? wrapper.RemoveQuerystringKey(BaseQueryAliases.Page) : wrapper.SetQuerystringKey(BaseQueryAliases.Page, i, true);

        //                    if (i != PageCount)
        //                    {
        //                        links.Add(new PaginationLink { Page = i, Href = pathAndQuery, Class = activeClass, Text = i.ToString() });
        //                    }
        //                }
        //                links.Add(new PaginationLink { Page = 0, Href = string.Empty });
        //                links.Add(new PaginationLink { Page = PageCount, Href = wrapper.SetQuerystringKey(BaseQueryAliases.Page, PageCount, true), Text = PageCount.ToString() });
        //            }
        //            else if (CurrentPage >= (PageCount - Limit))
        //            {
        //                links.Add(new PaginationLink { Page = 1, Href = wrapper.RemoveQuerystringKey(BaseQueryAliases.Page), Text = "1" });
        //                links.Add(new PaginationLink { Page = 0, Href = string.Empty });

        //                for (int i = PageCount - Limit; i <= PageCount; i++)
        //                {
        //                    var activeClass = (CurrentPage == i) ? ActiveClass : "";
        //                    var pathAndQuery = (i == 1) ? wrapper.RemoveQuerystringKey(BaseQueryAliases.Page) : wrapper.SetQuerystringKey(BaseQueryAliases.Page, i, true);

        //                    if (i != 1)
        //                    {
        //                        links.Add(new PaginationLink { Page = i, Href = pathAndQuery, Class = activeClass, Text = i.ToString() });
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            for (int i = 1; i <= PageCount; i++)
        //            {
        //                var activeClass = (CurrentPage == i) ? ActiveClass : "";
        //                var pathAndQuery = (i == 1) ? wrapper.RemoveQuerystringKey(BaseQueryAliases.Page) : wrapper.SetQuerystringKey(BaseQueryAliases.Page, i, true);

        //                links.Add(new PaginationLink { Page = i, Href = pathAndQuery, Class = activeClass, Text = i.ToString() });
        //            }
        //        }
        //    }

        //    NextPageUrl = new PaginationLink()
        //    {
        //        Class = IsLastPage ? "disabled" : "",
        //        Href = (CurrentPage < PageCount) ? wrapper.SetQuerystringKey(BaseQueryAliases.Page, CurrentPage + 1, true) : "",
        //    };

        //    LastPageUrl = new PaginationLink()
        //    {
        //        Class = IsLastPage ? "disabled" : "",
        //        Href = (CurrentPage < PageCount) ? wrapper.SetQuerystringKey(BaseQueryAliases.Page, PageCount, true) : "",
        //    };

        //    Links = links;
        //}
    }
    public class PaginationLink
    {
        public string Class { get; set; }
        public string Href { get; set; }
        public string Text { get; set; }
        public int Page { get; set; }
    }
}
