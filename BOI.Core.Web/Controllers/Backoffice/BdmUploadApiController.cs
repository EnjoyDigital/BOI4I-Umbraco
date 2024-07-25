using BankOfIreland.Intermediaries.Core.Extensions;
using BankOfIreland.Intermediaries.Core.Web.Attributes;
using BankOfIreland.Intermediaries.Core.Web.Constants;
using BankOfIreland.Intermediaries.Core.Web.Extensions;
using BankOfIreland.Intermediaries.Core.Web.Models.CmsModels;
using BankOfIreland.Intermediaries.Feature.Search.Infrastructure.PostCodeLookup;
using BankOfIreland.Intermediaries.Feature.Search.Models;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using BankOfIreland.Intermediaries.Core.Web.Commands;
using Newtonsoft.Json;
using System.Text;
using System.Web;

namespace BankOfIreland.Intermediaries.Core.Web.Controllers.Backoffice
{
    [PluginController("BdmUpload")]
    [UmbracoUserTimeoutFilter]
    [Umbraco.Web.WebApi.UmbracoAuthorize]
    [IsBackOffice]
    [CamelCaseControllerConfig]
    public class BdmUploadApiController : UmbracoAuthorizedApiController
    {
        private IEnumerable<string> AllowedExtensions =new [] { ".csv", ".xlsx"};
        //private static string FileName { get; set; }
        private static readonly string FileUploadPath = IOHelper.MapPath("~/App_Data/TEMP/CSVUploads/");
      
        private readonly IContentImporter bdmImporter;

        public BdmUploadApiController(IContentImporter bdmImporter)
        {
           
            this.bdmImporter = bdmImporter;
        }

        /// <summary>
        /// Saves file to disk
        /// </summary>
        /// <returns>Returns full filename and path of valid uploaded file, or null if none</returns>
        public async Task<string> SaveFile()
        {
            if (!Directory.Exists(FileUploadPath))
            {
                Directory.CreateDirectory(FileUploadPath);
            }
            var provider = new MultipartFormDataStreamProvider(FileUploadPath);

            var result = await Request.Content.ReadAsMultipartAsync(provider);

            foreach (var file in result.FileData)
            {
                var fileName = file.Headers.ContentDisposition.FileName.Trim('\"');
                var ext =  Path.GetExtension(fileName).ToLower();
                if (!AllowedExtensions.Contains(ext))
                {
                    Logger.Info<BdmUploadApiController>($"{fileName} extension not allowed");
                    continue;
                }

                var saveFilename = string.Concat(FileUploadPath, Path.GetFileName(fileName), DateTime.Now.ToString("ddMMMyyyyhhmmss"), ext);
                System.IO.File.Copy(file.LocalFileName, saveFilename, true);
                System.IO.File.Delete(file.LocalFileName);
                return saveFilename;
            }
            return null;
        }

        [System.Web.Http.HttpPost]
        public async Task<HttpResponseMessage> ProcessFile()
        {
            var request = HttpContext.Current.Request;
            var importerResponse = new ImporterResponse();
            var savedFile = await SaveFile();
            if(savedFile.HasValue())
            {
                var root = Umbraco.ContentAtRoot().FirstOrDefault()?.Children?.OfType<SiteRoot>().FirstOrDefault();
                var bdmParent = root?.Children?.OfType<BDmfinder>().FirstOrDefault();
                if(bdmParent == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Bdm listing/container not found");
                }

                var importRequest = new ImporterRequest();
                importRequest.FileNamePath = savedFile;
                importRequest.UmbracoParentContainer = bdmParent;
                importRequest.NodeTypeAlias = BDmcontact.ModelTypeAlias;
                importRequest.ImportFileColumnMapping = new Dictionary<int, string>() { { request.Form[BdmContactConstants.FCANumber].TryParseInt32().GetValueOrDefault(2), BdmContactConstants.FCANumber},
                                {  request.Form["postcode"].TryParseInt32().GetValueOrDefault(3), BdmContactConstants.Regions},
                                {request.Form["firstName"].TryParseInt32().GetValueOrDefault(4) , BdmContactConstants.Firstname}, 
                                {request.Form["lastName"].TryParseInt32().GetValueOrDefault(5),BdmContactConstants.Surname }, 
                                { request.Form["email"].TryParseInt32().GetValueOrDefault(6), BdmContactConstants.Email},
                                { request.Form["contactNumber"].TryParseInt32().GetValueOrDefault(7), BdmContactConstants.ContactNumber },
                                { request.Form["jobTitle"].TryParseInt32().GetValueOrDefault(9),BdmContactConstants.JobTitle },
                                { request.Form[BdmContactConstants.BDMType].TryParseInt32().GetValueOrDefault(-1),BdmContactConstants.BDMType }

               // ,{ request.Form[BdmContactConstants.RequireFCAAndPostcodeMatch].TryParseInt32().GetValueOrDefault(15),BdmContactConstants.RequireFCAAndPostcodeMatch }
                ,{ request.Form[BdmContactConstants.Bio].TryParseInt32().GetValueOrDefault(8),BdmContactConstants.Bio }};

                importRequest.IgnoreImportItemsByColumnAlias = new Dictionary<string, IEnumerable<string>>() { { BdmContactConstants.JobTitle, new[] { /*"IEL",*/ string.Empty } } };
                importRequest.NodeNameProperties = new[] { BdmContactConstants.Firstname, BdmContactConstants.Surname};

                importRequest.PropertyAliasToMatchContentItem = BdmContactConstants.Email;
                importRequest.MergeColumns = new[] { BdmContactConstants.Regions,
                    BdmContactConstants.FCANumber };

                importerResponse = bdmImporter.ProcessImportFile(importRequest);

                return new HttpResponseMessage
                {
                    StatusCode = importerResponse.FileValidationMessages.NotNullAndAny() ? HttpStatusCode.InternalServerError : HttpStatusCode.Accepted,
                    Content = new StringContent(JsonConvert.SerializeObject(importerResponse), Encoding.UTF8, "application/json")
                };
            }


            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new Exception());
        }

    }
}