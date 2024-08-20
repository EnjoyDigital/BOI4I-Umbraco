using BOI.Core.Search.Models;
using BOI.Umbraco.Models;

namespace BOI.Core.Web.Models.ViewModels
{
    public class SearchResultsViewModel
    {
       
        public SearchResult Content { get; set; }
        public MainSearchResults Results { get; set; }
        public Page<IPagedResult> Paging { get; set; }
        public string SearchTerm { get; set; }
    }
}
