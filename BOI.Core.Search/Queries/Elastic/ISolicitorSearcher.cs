using BOI.Core.Search.Models;

namespace BOI.Core.Search.Queries.Elastic
{
    public interface ISolicitorSearcher
    {
        SolicitorsResults Execute(SolicitorSearch model);
    }
}