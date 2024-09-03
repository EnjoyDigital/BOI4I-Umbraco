using BOI.Core.Web.Services;
using BOI.Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Filters;
using static BOI.Core.Web.Services.FileUploadService;

namespace BOI.Core.Web.Controllers.Backoffice.Api
{
    [PluginController("EdAdmin")]
    [UmbracoUserTimeoutFilter]
    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
    [IsBackOffice]
    public class EdAdminApiController : UmbracoAuthorizedApiController
    {
        private IEdAdminService _edAdminService;
        private readonly IFileUploadService fileUploadService;
        private readonly ILogger<EdAdminApiController> logger;
        private readonly HttpResponseMessage xmlSupportMediaType = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.UnsupportedMediaType,
            Content = new StringContent("File must be a valid XML file")
        };

        private readonly HttpResponseMessage csvSupportMediaType = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.UnsupportedMediaType,
            Content = new StringContent("File must be a valid CSV file")
        };

        public EdAdminApiController(IEdAdminService edAdminService, IFileUploadService fileUploadService, ILogger<EdAdminApiController> logger)
        {
            _edAdminService = edAdminService;
            this.fileUploadService = fileUploadService;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> ImportHrefLang()
        {
            
            var fileSave = await fileUploadService.SaveFile(Request?.Form?.Files[0]);
            var path = fileSave.FilePath;
            var ext = path.Substring(path.LastIndexOf('.')).ToLower();
            if (ext != ".xml")
            {
                return xmlSupportMediaType;
            }
            var importResults = _edAdminService.ImportHrefLang(path);
            return new HttpResponseMessage
            {
                StatusCode = string.IsNullOrWhiteSpace(importResults.Errors) ? HttpStatusCode.Accepted : HttpStatusCode.InternalServerError,
                Content = new StringContent(JsonConvert.SerializeObject(importResults), Encoding.UTF8, "application/json")
            };
        }

        [HttpGet]
        public IActionResult ExportMeta()
        {
            try
            {
                var stream = new MemoryStream(_edAdminService.ExportMetaData());

                return new FileStreamResult(stream, "text/csv")
                {
                    FileDownloadName = $"MetaData-{DateTime.Now:dd-MMM-yy}.csv"
                };
            }
            catch (Exception ex)
            {
                logger.LogError($"Error - Exporting Meta: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }


        [HttpPost]
        public async Task<HttpResponseMessage> ImportMeta()
        {
          
            var fileSave = await fileUploadService.ValidateAndSaveFile(Request?.Form?.Files[0], FileSaveType.CSV);
            if (fileSave.Errors.NotNullAndAny())
            {
                return  new HttpResponseMessage
                { 
                    StatusCode = HttpStatusCode.InternalServerError
                };
             }

            var path = fileSave.FilePath;
            var ext = path.Substring(path.LastIndexOf('.')).ToLower();
            if (ext != ".csv")
            {
                return csvSupportMediaType;
            }

            var importResults = _edAdminService.ImportMetaData(path);
            return new HttpResponseMessage
            {
                StatusCode = string.IsNullOrWhiteSpace(importResults.Errors) ? HttpStatusCode.Accepted : HttpStatusCode.InternalServerError,
                Content = new StringContent(JsonConvert.SerializeObject(importResults), Encoding.UTF8, "application/json")
            };
        }

        [HttpGet]
        public IActionResult ExportMembers()
        {
            try
            {
                var stream = new MemoryStream(_edAdminService.ExportMembers());

                return new FileStreamResult(stream, "text/csv")
                {
                    FileDownloadName = $"MembersExport-{DateTime.Now:dd-MMM-yy}.csv"
                };
            }
            catch (Exception ex)
            {
                logger.LogError($"Error - Exporting Members: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}