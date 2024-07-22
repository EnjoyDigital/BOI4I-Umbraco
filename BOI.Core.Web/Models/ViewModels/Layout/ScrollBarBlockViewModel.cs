using BOI.Umbraco.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace BOI.Core.Web.Models.ViewModels.Layout
{
    public class ScrollBarBlockViewModel
    {
        public ScrollBarBlockViewModel(IPublishedContent currentPage)
        {
            Content = currentPage;
        }

        public IPublishedContent Content { get; set; }
        public SiteRoot SiteRoot { get; set; }

    }
}
