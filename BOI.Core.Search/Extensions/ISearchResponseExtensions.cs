using Nest;

namespace BOI.Core.Search.Extensions
{
    public static class ISearchResponseExtensions
    {
        public static ISearchResponse<T> EnsureSuccess<T>(this ISearchResponse<T> searchResponse) where T : class
        {
            if (searchResponse is null)
            {
                throw new ArgumentNullException(nameof(searchResponse));
            }

            if (!searchResponse.IsValid)
            {
                throw searchResponse.OriginalException.InnerException;
            }

            return searchResponse;
        }
    }
}