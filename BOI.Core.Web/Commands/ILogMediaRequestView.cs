using BOI.Core.Web.Models.Dtos;

namespace BOI.Core.Web.Commands
{
    public interface ILogMediaRequestView
    {
        void LogMediaViewed(MediaRequestLog mediaRequestLog);
    }
}