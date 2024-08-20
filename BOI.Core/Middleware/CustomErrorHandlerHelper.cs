using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace BOI.Core.Middleware
{
	public static class CustomErrorHandlerHelper
	{
		public static void UseCustomErrors(this IApplicationBuilder app, IWebHostEnvironment environment)
		{
			app.Use(WriteProductionResponse);
		}

		private static Task WriteProductionResponse(HttpContext httpContext, Func<Task> next)
			=> WriteResponse(httpContext, includeDetails: false);

		private static async Task WriteResponse(HttpContext httpContext, bool includeDetails)
		{

			var exceptionDetails = httpContext.Features.Get<IExceptionHandlerFeature>();
			var ex = exceptionDetails?.Error;

			if (ex != null)
			{
				httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				httpContext.Response.ContentType = "text/HTML";

				var path = $"{Path.GetFullPath("wwwroot")}/500-{httpContext.Request.Host}.html";

				var file = await File.ReadAllTextAsync(path);

				await httpContext.Response.WriteAsync(file);
			}
		}
	}
}
