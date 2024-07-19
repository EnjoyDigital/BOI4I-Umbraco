using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace BOI.Core.Web.Extensions
{
    public static class BlockListModelExtensions
    {

        public static T GetElementByContentType<T>(this BlockListModel blockListModel, string modelTypeAlias) where T : PublishedElementModel
        {
            return blockListModel?.FirstOrDefault(c => c.Content.ContentType.Alias == modelTypeAlias)?.Content as T;
        }

        public static T GetElementByContentType<T>(this BlockGridModel blockListModel, string modelTypeAlias) where T : PublishedElementModel
        {
            return blockListModel?.FirstOrDefault(c => c.Content.ContentType.Alias == modelTypeAlias)?.Content as T;
        }
    }
}
