
using Nest;

using BOI.Core.Web.Commands;
using BOI.Core.Web.Factories;
using Umbraco.Cms.Core.Services;
using Microsoft.Extensions.Logging;
using BOI.Core.Web.Models.Dtos;

namespace BankOfIreland.Intermediaries.Core.Web.Commands
{
    public class LogMediaRequestView : ILogMediaRequestView
    {
       private readonly IMediaService mediaService;
        private readonly ILogger logger;

        public LogMediaRequestView(IDatabaseFactory Idbfactory, IElasticClient elasticClient, IMediaService mediaService, ILogger<LogMediaRequestView> logger)
        {
            this.mediaService = mediaService;
            this.logger = logger;
        }

        public void LogMediaViewed(MediaRequestLog mediaRequestLog)
        {
            var mediaItem = mediaService.GetMediaByPath(mediaRequestLog.MediaUrl);
            if(mediaItem != null)
            {
                var currentValue = mediaItem.GetValue<int>("downloadCounter");
                mediaItem.SetValue("downloadCounter", currentValue + 1);
                try
                {
                    mediaService.Save(mediaItem);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "requested url " +mediaRequestLog.MediaUrl);
                }
            }
        }
    }
}
