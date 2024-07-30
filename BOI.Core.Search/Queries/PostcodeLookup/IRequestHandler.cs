using Microsoft.Extensions.Logging;

namespace BOI.Core.Search.Queries.PostcodeLookup
{
    public interface IRequestHandler
    {
        ILogger Logger { get; }

        PostcodeLookupQuery.Result Execute(PostcodeLookupQuery.Request request);
    }
}