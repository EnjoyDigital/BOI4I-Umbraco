using Microsoft.AspNetCore.Mvc.Rendering;

namespace BOI.Core.Extensions
{
    public class IndexedType<T>
    {
        public T Item { get; set; }
        public int Index { get; set; }
    }

    public static class IEnumerableExtensions
    {
        public static bool NotNullAndAny<T>(this IEnumerable<T> items)
            => items != null && items.Any();

        public static bool NullOrEmpty<T>(this IEnumerable<T> items)
            => items == null || !items.Any();

        public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T> items)
        {
            if (items == null)
            {
                return default;
            }
            return items.Where(i => i != null);
        }


        public static IEnumerable<SelectListItem> ToSelectList<T>(this IEnumerable<T> items, Func<T, string> text, Func<T, string> value = null, Func<T, bool> selected = null)
            => items.ToSelectListWithDefault(text, value, selected, defaultOption: null);

        public static IEnumerable<SelectListItem> ToSelectListWithDefault<T>(this IEnumerable<T> items, Func<T, string> text, Func<T, string> value = null, Func<T, bool> selected = null, string defaultOptionText = "")
             => items.ToSelectListWithDefault(text, value, selected, new SelectListItem { Text = defaultOptionText, Value = "" });

        public static IEnumerable<SelectListItem> ToSelectListWithDefault<T>(this IEnumerable<T> items, Func<T, string> text, Func<T, string> value = null, Func<T, bool> selected = null, SelectListItem defaultOption = null)
        {
            var list =
                items
                    .Select(p => new SelectListItem
                    {
                        Text = text.Invoke(p),
                        Value = value == null ? text.Invoke(p) : value.Invoke(p),
                        Selected = selected != null && selected.Invoke(p)
                    })
                    .OrderBy(item => item.Text)
                    .ToList();

            if (defaultOption != null)
            {
                defaultOption.Selected = true;
                list.Insert(0, defaultOption);
            }

            return list;
        }

        public static IEnumerable<IndexedType<T>> Index<T>(this IEnumerable<T> source)
        {
            return source.Select((x, i) => new IndexedType<T> { Item = x, Index = i });
        }

        public static IEnumerable<IEnumerable<T>> SplitInto<T>(this IEnumerable<T> source, int groupCount)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / groupCount)
                .Select(x => x.Select(v => v.Value).ToList());
        }
    }
}
