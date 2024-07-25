using BOI.Core.Search.Models;

namespace BOI.Core.Search.Queries.Elastic
{
    public interface IBdmFinderSearcher
    {
        BdmResults Execute(BdmFinderSearch model);
    }
}