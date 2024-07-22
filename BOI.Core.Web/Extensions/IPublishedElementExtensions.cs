using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace BOI.Core.Web.Extensions
{
    public static class IPublishedElementExtensions
    {
        public static IPublishedElement GetElement(this IEnumerable<IPublishedElement> items, string doctypeAlias)
        {
            var element = items.FirstOrDefault(x => x.ContentType.Alias == doctypeAlias);

            return element;
        }

        public static T GetElement<T>(this IEnumerable<IPublishedElement> items)
        {
            var element = items.FirstOrDefault(x => x.ContentType.Alias == typeof(T).GetField("ModelTypeAlias").GetValue(x).ToString());

            return (T)element;
        }

        public static T GetElementValue<T>(this IEnumerable<IPublishedElement> items, string doctypeAlias, string propertyAlias)
        {
            var element = items.FirstOrDefault(x => x.ContentType.Alias == doctypeAlias);

            if (element != null)
            {
                var value = element.Value<T>(propertyAlias);

                return value;
            }

            return default;
        }

        public static object Value(this IEnumerable<IPublishedElement> items, string doctypeAlias, string propertyAlias)
        {
            var element = items.FirstOrDefault(x => x.ContentType.Alias == doctypeAlias);

            if (element != null)
            {
                var value = element.Value(propertyAlias);

                return value;
            }

            return default;
        }
    }

}
