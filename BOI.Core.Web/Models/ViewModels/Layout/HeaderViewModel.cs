using BOI.Umbraco.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace BOI.Core.Web.Models.ViewModels
{
    public class HeaderViewModel
    {
        public HeaderViewModel(IPublishedContent currentPage)
        {
            Content = currentPage;
        }

        public IPublishedContent Content { get; set; }
        public SiteRoot SiteRoot { get; set; }
    }
}
