using Our.Umbraco.Meganav.Models;

namespace BOI.Core.Web.Extensions
{
    public static class MegaNavExtensions
    {
        public static string GetTitleOrName(this IMeganavItem item)
        {
            if (item.Title.HasValue())
            {
                return item.Title;
            }
            if (item.Content != null)
            {
                return item.Content.Name;
            }

            return item.Url;
        }
    }
}
