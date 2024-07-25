using BankOfIreland.Intermediaries.Core.Web.Attributes;
using BankOfIreland.Intermediaries.Core.Web.Extensions;
using BankOfIreland.Intermediaries.Feature.Search.Infrastructure.PostCodeLookup;
using BankOfIreland.Intermediaries.Feature.Search.Models;
using BankOfIreland.Intermediaries.Feature.Search.Services;
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
using Umbraco.Core.Logging;
using Umbraco.Core.IO;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using BankOfIreland.Intermediaries.Core.Factories;

namespace BankOfIreland.Intermediaries.Core.Web.Controllers.Backoffice
{
    [PluginController("SolicitorUpload")]
    [UmbracoUserTimeoutFilter]
    [Umbraco.Web.WebApi.UmbracoAuthorize]
    [IsBackOffice]
    [CamelCaseControllerConfig]
    public class SolicitorUploadApiController : UmbracoAuthorizedApiController
    {
        private const string AllowedExtension = ".csv";
        private static string FileName { get; set; }
        private static readonly string FileUploadPath = IOHelper.MapPath("~/App_Data/TEMP/CSVUploads/");
        private static readonly Regex CsvRegex = new Regex(@"(?x)\s*,\s*(?=(?:[^""]*""[^""]*"")*[^""]*$)");

        private readonly IPostcodeGeocoder geocoder;
        private readonly IElasticClient esClient;
        private readonly IConfiguration config;
        private readonly ILogger logger;
        private readonly IDatabaseFactory dbFactory;

        public SolicitorUploadApiController(IElasticClient esClient, IConfiguration config, ILogger logger, IDatabaseFactory dbFactory)
        {
            geocoder = new PostcodeGeocoder();
            this.esClient = esClient;
            this.config = config;
            this.logger = logger;
            this.dbFactory = dbFactory;
        }

        public async Task<bool> SaveFile()
        {
            if (!Directory.Exists(FileUploadPath))
            {
                Directory.CreateDirectory(FileUploadPath);
            }
            var provider = new MultipartFormDataStreamProvider(FileUploadPath);

            var result = await Request.Content.ReadAsMultipartAsync(provider);

            foreach (var file in result.FileData)
            {
                FileName = file.Headers.ContentDisposition.FileName.Trim('\"');
                var ext = FileName.Substring(FileName.LastIndexOf('.')).ToLower();
                if (!AllowedExtension.Equals(ext)) continue;
                File.Copy(file.LocalFileName, FileUploadPath + FileName, true);
                File.Delete(file.LocalFileName);
                return true;
            }
            return false;
        }

        [System.Web.Http.HttpPost]
        public async Task<HttpResponseMessage> ProcessFile()
        {
            if (await SaveFile())
            {
                try
                {
                    var solicitorsData = GetFileData().RemoveNulls().Where(x => x.PostCode != null).ToList();

                    var postCodeLocations = geocoder.PostCodeLookup(solicitorsData.RemoveNulls().Select(x => x.PostCode)).ToList();

                    foreach (var solicitor in solicitorsData)
                    {
                        foreach (var postcode in postCodeLocations.Where(postcode => postcode.Query == solicitor.PostCode && postcode.Result.Lat.HasValue() && postcode.Result.Lon.HasValue()))
                        {
                            solicitor.Lat = float.Parse(postcode.Result.Lat);
                            solicitor.Lon = float.Parse(postcode.Result.Lon);
                        }

                    }

                    var indexingService = new IndexingService(config, esClient, logger, dbFactory);
                    indexingService.IndexSolicitors(solicitorsData.RemoveNulls());

                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message, ex);
                }
            }
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new Exception());
        }

        private IEnumerable<Solicitor> GetFileData()
        {
            var file = IOHelper.MapPath(FileUploadPath + FileName);
            if (File.Exists(file))
            {
                try
                {
                    using (var reader = new StreamReader(file))
                    {
                        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            csv.Configuration.Delimiter = ",";
                            csv.Configuration.IgnoreBlankLines = true;
                            csv.Configuration.HeaderValidated = null;
                            csv.Configuration.HasHeaderRecord = true;
                            var records = csv.GetRecords<Solicitor>().ToList();
                            return records;
                        }
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return Enumerable.Empty<Solicitor>();
        }
    }
}