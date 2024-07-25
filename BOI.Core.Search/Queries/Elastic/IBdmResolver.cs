using BOI.Core.Search.Models;

namespace BOI.Core.Search.Queries.Elastic   
{
    public interface IBdmResolver
    {
        BdmResult Execute(BdmResolverQuery query);
    }
}