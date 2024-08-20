using System;
using System.Collections.Generic;

namespace BOI.Core.Search.Models
{
    public class MainSearchResults 
    {
        public int Page { get; set; }
        public int Size { get; set; }
        public int Total { get; set; }
        public List<MainSearchResult> QueryResults { get; set; }
        public IEnumerable<SearchFilter> Filters { get; set; }
    }

    public class MainSearchResult : IPagedResult
    {
        public string SearchTitle { get; set; }
        public string SearchDescription { get; set; }
        public string SearchLink { get; set; }
    }
}
