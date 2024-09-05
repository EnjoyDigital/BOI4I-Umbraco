using BOI.Core.Extensions;
using BOI.Core.Web.Commands;
using BOI.Core.Web.Models.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOI.Core.Web.Middleware
{
    public class MediaRequestHandler
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration config;
        private readonly ILogMediaRequestView logMediaRequestView;

        public MediaRequestHandler(RequestDelegate next, IConfiguration config, ILogMediaRequestView logMediaRequestView)
        {
            _next = next;
            this.config = config;
            this.logMediaRequestView = logMediaRequestView;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            switch (context.Request.Method)
            {
                //First make sure this is a get request
                case "GET":
                    {
                        //If its not media, move on...
                        if (!context.Request.Path.StartsWithSegments("/media"))
                        {
                            await _next(context);
                            return;
                        }
                        //This will give you absolute path
                        var absolutePath = context.Request.GetCurrentUriFromRequest().AbsolutePath;
                        var referrer = context.Request.GetReferer()?.AbsolutePath ?? "";
                        var validTrackingExtensions = (config.GetValue<string>("MediaTrackingExtensions") ?? "").Split(new[] { "," }, StringSplitOptions.None);

                        //TODO add in extension filtering
                        if (absolutePath.StartsWith(@"/media") && !referrer.StartsWith(@"/umbraco"))
                        {
                            var mediarequestLog = new MediaRequestLog();
                            mediarequestLog.DateViewed = DateTime.Now;
                            mediarequestLog.MediaUrl = absolutePath;
                            logMediaRequestView.LogMediaViewed(mediarequestLog);
                        }


                        break;
                    }
            }


            await _next(context);

            return;
        }
    }
}
