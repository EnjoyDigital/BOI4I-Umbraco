using BOI.Umbraco.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using static BOI.Core.Web.Constants.SiteAliases;

namespace BOI.Core.Web.Models.ViewModels.Layout
{
    public class BodyViewModel
    {
        public BodyViewModel(IPublishedContent currentPage)
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
        public bool IsUpper { get; set; }

        public bool HideNavigation => (Content?.Value<bool>(FieldAlias.HideMainNavigation)).GetValueOrDefault();

        public string SearcjAjaxUrl { get; set; }

    }
}
