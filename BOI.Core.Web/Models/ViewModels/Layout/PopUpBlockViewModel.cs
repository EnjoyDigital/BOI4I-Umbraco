using BOI.Umbraco.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace BOI.Core.Web.Models.ViewModels.Layout
{
    public class PopUpBlockViewModel
    {
        public PopUpBlockViewModel(IPublishedContent currentPage)
        {
            Content = currentPage;
        }

        public IPublishedContent Content { get; set; }

        public SiteRoot SiteRoot { get; set; }
    }
}
