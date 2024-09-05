using BOI.Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOI.Core.Middleware
{
    public class CustomMediaAuthentication
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration config;

        public CustomMediaAuthentication(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            this.config = config;
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
                            var logMedia = ResolveService<ILogMediaRequestView>();
                            logMedia.LogMediaViewed(mediarequestLog);
                        }


                        break;
                    }
            }
            

            //Authenticate user
            if (context.User.Identities.Any(IdentityExtensions => IdentityExtensions.IsAuthenticated))
            {
                await _next(context);
                return;
            }

            // Stop processing the request and return a 401 response.
            context.Response.StatusCode = 401;
            await Task.FromResult(0);
            return;
        }
    }
}
