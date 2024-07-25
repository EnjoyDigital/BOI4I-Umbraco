using BankOfIreland.Intermediaries.Core.Models.Attributes;
using BankOfIreland.Intermediaries.Core.Web.Services;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

namespace BankOfIreland.Intermediaries.Core.Web.Controllers.Backoffice.Api
{
    [PluginController("EdAdmin")]
    [UmbracoUserTimeoutFilter]
    [Umbraco.Web.WebApi.UmbracoAuthorize]
    [IsBackOffice]
    [CamelCaseControllerConfig]
    public class EdAdminApiController : UmbracoAuthorizedApiController
    {
        private IEdAdminService _edAdminService;

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

        public EdAdminApiController(IEdAdminService edAdminService)
        {
            _edAdminService = edAdminService;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> ImportHrefLang()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return xmlSupportMediaType;
            }
            var file = await _edAdminService.SaveFile(Request);
            var path = file.LocalFileName;
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
        public HttpResponseMessage ExportMeta()
        {            
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(_edAdminService.ExportMetaData())
            };
            var fileName = string.Format("MetaData-{0:dd-MMM-yy}.csv", DateTime.Now);
            response.Headers.Add("x-filename", fileName);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };
            return response;
        }
        
        [HttpPost]
        public async Task<HttpResponseMessage> ImportMeta()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return csvSupportMediaType;
            }
            var file = await _edAdminService.SaveFile(Request);
            var path = file.LocalFileName;
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
        public HttpResponseMessage ExportMembers()
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(_edAdminService.ExportMembers())
            };
            var fileName = string.Format("MembersExport-{0:dd-MMM-yy}.csv", DateTime.Now);
            response.Headers.Add("x-filename", fileName);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };
            return response;
        }
    }
}