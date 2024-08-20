using BOI.Core.Web.Commands;
using BOI.Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Extensions;
using BOI.Umbraco.Models;
using BOI.Core.Search.Constants;
using BOI.Core.Web.Services;
using NUglify;
using static BOI.Core.Web.Services.FileUploadService;

namespace BankOfIreland.Intermediaries.Core.Web.Controllers.Backoffice
{
    [PluginController("BdmUpload")]
    [UmbracoUserTimeoutFilter]
    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
    [IsBackOffice]
    public class BdmUploadApiController : UmbracoAuthorizedApiController
    {
        
        //private static string FileName { get; set; }
        private  readonly string FileUploadPath = "";
      
        private readonly IContentImporter bdmImporter;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IUmbracoHelperAccessor umbracoHelperAccessor;
        private readonly IFileUploadService fileUploadService;
        private readonly IIOHelper iOHelper;

        public BdmUploadApiController(IContentImporter bdmImporter, IWebHostEnvironment hostingEnvironment, 
            IUmbracoHelperAccessor umbracoHelperAccessor, IFileUploadService fileUploadService)
        {
           
            this.bdmImporter = bdmImporter;
            this.hostingEnvironment = hostingEnvironment;
            this.umbracoHelperAccessor = umbracoHelperAccessor;
            this.fileUploadService = fileUploadService;
            FileUploadPath = Path.Combine(hostingEnvironment.ContentRootPath, "~/App_Data/TEMP/CSVUploads/");
        }

        /// <summary>
        /// Saves file to disk
        /// </summary>
        /// <returns>Returns full filename and path of valid uploaded file, or null if none</returns>
        //public async Task<string> SaveFile()
        //{
        //    if (!Directory.Exists(FileUploadPath))
        //    {
        //        Directory.CreateDirectory(FileUploadPath);
        //    }

        //    var fileRequest?.Form.Files
        //    var fileata Request?.Form.Files
        //    var provider = new MultipartFormDataStreamProvider(FileUploadPath);

        //    var result = await Request.Content.ReadAsMultipartAsync(provider);

        //    foreach (var file in result.FileData)
        //    {
        //        var fileName = file.Headers.ContentDisposition.FileName.Trim('\"');
        //        var ext =  Path.GetExtension(fileName).ToLower();
        //        if (!AllowedExtensions.Contains(ext))
        //        {
        //            Logger.Info<BdmUploadApiController>($"{fileName} extension not allowed");
        //            continue;
        //        }

        //        var saveFilename = string.Concat(FileUploadPath, Path.GetFileName(fileName), DateTime.Now.ToString("ddMMMyyyyhhmmss"), ext);
        //        System.IO.File.Copy(file.LocalFileName, saveFilename, true);
        //        System.IO.File.Delete(file.LocalFileName);
        //        return saveFilename;
        //    }
        //    return null;
        //}

       [HttpPost]
        public async Task<HttpResponseMessage> ProcessFile()
        {
            var request = Request;
            var importerResponse = new ImporterResponse();
            var fileSave = await fileUploadService.ValidateAndSaveFile(Request?.Form?.Files[0], FileSaveType.BDM);

            if(fileSave.FilePath.HasValue())
            {
                if (umbracoHelperAccessor.TryGetUmbracoHelper(out var umbracoHelper))
                {

                    var root = umbracoHelper.ContentAtRoot().FirstOrDefault()?.Children?.OfType<SiteRoot>().FirstOrDefault();
                    var bdmParent = root?.Children?.OfType<BDmfinder>().FirstOrDefault();

                    if (bdmParent == null)
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.InternalServerError,
                            Content = new StringContent("Bdm listing/container not found")
                        };
                    }

                    var importRequest = new ImporterRequest();
                    importRequest.FileNamePath = fileSave.FilePath;
                    importRequest.UmbracoParentContainer = bdmParent;
                    importRequest.NodeTypeAlias = BDmcontact.ModelTypeAlias;
                    importRequest.ImportFileColumnMapping = 
                        new Dictionary<int, string>() { { request.Form[BdmContactConstants.FCANumber].FirstOrDefault().TryParseInt32().GetValueOrDefault(2), BdmContactConstants.FCANumber},
                                {  request.Form["postcode"].FirstOrDefault().TryParseInt32().GetValueOrDefault(3), BdmContactConstants.Regions},
                                {request.Form["firstName"].FirstOrDefault().TryParseInt32().GetValueOrDefault(4) , BdmContactConstants.Firstname},
                                {request.Form["lastName"].FirstOrDefault().TryParseInt32().GetValueOrDefault(5),BdmContactConstants.Surname },
                                { request.Form["email"].FirstOrDefault().TryParseInt32().GetValueOrDefault(6), BdmContactConstants.Email},
                                { request.Form["contactNumber"].FirstOrDefault().TryParseInt32().GetValueOrDefault(7), BdmContactConstants.ContactNumber },
                                { request.Form["jobTitle"].FirstOrDefault().TryParseInt32().GetValueOrDefault(9),BdmContactConstants.JobTitle },
                                { request.Form[BdmContactConstants.BDMType].FirstOrDefault().TryParseInt32().GetValueOrDefault(-1),BdmContactConstants.BDMType }

               // ,{ request.Form[BdmContactConstants.RequireFCAAndPostcodeMatch].TryParseInt32().GetValueOrDefault(15),BdmContactConstants.RequireFCAAndPostcodeMatch }
                ,{ request.Form[BdmContactConstants.Bio].FirstOrDefault().TryParseInt32().GetValueOrDefault(8),BdmContactConstants.Bio }};

                    importRequest.IgnoreImportItemsByColumnAlias = new Dictionary<string, IEnumerable<string>>() { { BdmContactConstants.JobTitle, new[] { /*"IEL",*/ string.Empty } } };
                    importRequest.NodeNameProperties = new[] { BdmContactConstants.Firstname, BdmContactConstants.Surname };

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


                    
                
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Bdm listing/container not found")
            };
        }

    }
}