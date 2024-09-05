
using Microsoft.Extensions.Configuration;
using BOI.Core.Web.Factories;
using BankOfIreland.Intermediaries.Core.Web.Commands;
using Umbraco.Cms.Core.Services;
using Microsoft.Extensions.Logging;
using BOI.Core.Search.Services;
using BOI.Core.Extensions;
using BOI.Core.Web.Models.Dtos;

namespace BOI.Core.Web.Commands
{
    public class LogMediaRequestViewToDatabase : ILogMediaRequestView
    {
        private readonly IDatabaseFactory Idbfactory;
        private readonly IMediaService mediaService;
        private readonly ILogger logger;
        private readonly IIndexingService indexingService;
        private readonly IConfiguration config;

        public LogMediaRequestViewToDatabase(IDatabaseFactory Idbfactory, IMediaService mediaService,
            ILogger<LogMediaRequestViewToDatabase> logger, IIndexingService indexingService, IConfiguration config)
        {
            this.Idbfactory = Idbfactory;
            this.mediaService = mediaService;
            this.logger = logger;
            this.indexingService = indexingService;
            this.config = config;
        }

        public void LogMediaViewed(MediaRequestLog mediaRequestLog)
        {

            if (!indexingService.MediaRequestLogIndexAlias.HasValue())
            { return; }
            var mediaItem = mediaService.GetMediaByPath(mediaRequestLog.MediaUrl);
            if (mediaItem != null)
            {
                //TODO:remove this saving to umbraco when query for ealstic search is done
                var currentValue = mediaItem.GetValue<int>("downloadCounter");
                mediaItem.SetValue("downloadCounter", currentValue + 1);
                try
                {

                    object dbResponse;
                    using (var db = Idbfactory.GetDatabase())
                    {
                        db.BeginTransaction();
                        mediaRequestLog.MediaItemId = mediaItem.Key;
                        dbResponse = db.Insert<MediaRequestLog>(mediaRequestLog);
                        db.CompleteTransaction();
                    }

                    if (dbResponse != null)
                    {
                        //TODO:push to indexing service
                        var indexItem = new BOI.Core.Search.Models.MediaRequestLog();
                        indexItem.DateViewed = mediaRequestLog.DateViewed;
                        indexItem.Id = mediaRequestLog.Id;
                        indexItem.MediaItemId = mediaRequestLog.MediaItemId;
                        indexItem.MediaUrl = mediaRequestLog.MediaUrl;

                        //TODO:DO this in a fire and forget
                        indexingService.IndexMediaViewLog(indexItem);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "requested url " + mediaRequestLog.MediaUrl);
                }

            }
        }
    }
}
