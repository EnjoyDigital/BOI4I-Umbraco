using BOI.Umbraco.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace BOI.Core.Web.Models.ViewModels.Layout
{
    public class HeadViewModel
    {
        public HeadViewModel(IPublishedContent currentPage)
        {
            Content = currentPage;
        }

        public IPublishedContent Content { get; set; }
        public SiteRoot SiteRoot { get; set; }

        public IPageSettingsMixin PageSettings => Content.SafeCast<IPageSettingsMixin>();
        public ElementSeoSettings SeoElement { get; set; }
        public ElementShareSettings ShareElement { get; set; }
        public GlobalScripts SiteScripts { get; set; }
        public ElementScriptSettings PageScripts { get; set; }
    }
}
