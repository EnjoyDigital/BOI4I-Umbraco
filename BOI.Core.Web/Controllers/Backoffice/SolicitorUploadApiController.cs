using BOI.Core.Extensions;
using BOI.Core.Search.Infrastructure;
using BOI.Core.Search.Services;
using BOI.Core.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Filters;

namespace BOI.Core.Web.Controllers.Backoffice
{
    [PluginController("SolicitorUpload")]
    [UmbracoUserTimeoutFilter]
    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
    [IsBackOffice]

    public class SolicitorUploadApiController : UmbracoAuthorizedApiController
    {
        private readonly IPostcodeGeocoder geocoder;
        private readonly ILogger<SolicitorUploadApiController> logger;
        private readonly IFileUploadService fileUploadService;
        private readonly IIndexingService indexingService;

        public SolicitorUploadApiController(IConfiguration config, ILogger<SolicitorUploadApiController> logger,
            IFileUploadService fileUploadService, IIndexingService indexingService)
        {
            geocoder = new PostcodeGeocoder(config);
            this.logger = logger;
            this.fileUploadService = fileUploadService;
            this.indexingService = indexingService;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> ProcessFile()
        {
            var response = await fileUploadService.ValidateAndSaveFile(Request?.Form.Files[0], "Solicitor");

            if (response.Errors.NotNullAndAny())
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(string.Join(Environment.NewLine, response.Errors))
                };
            }

            try
            {
                logger.LogInformation("Solicitor upload starts");
                var solicitorsData = fileUploadService.GetFileData<Search.Models.Solicitor>(response.FilePath).RemoveNulls().Where(x => x.PostCode != null).ToList();

                var postCodeLocations = geocoder.PostCodeLookup(solicitorsData.RemoveNulls().Select(x => x.PostCode)).Where(x => x.Result != null).ToList();

                foreach (var solicitor in solicitorsData)
                {
                    foreach (var postcode in postCodeLocations.Where(postcode => postcode.Query == solicitor.PostCode && postcode.Result.Lat.HasValue() && postcode.Result.Lon.HasValue()))
                    {
                        solicitor.Lat = float.Parse(postcode.Result.Lat);
                        solicitor.Lon = float.Parse(postcode.Result.Lon);
                    }

                }

                indexingService.IndexSolicitors(solicitorsData.RemoveNulls());

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                };
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(string.Join(Environment.NewLine, response.Errors, ex.Message))
                };
            }
        }
    }
}