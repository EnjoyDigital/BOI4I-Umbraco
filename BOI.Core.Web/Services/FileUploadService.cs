using BOI.Core.Web.Models.Dtos;
using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Globalization;
using static BOI.Core.Web.Services.FileUploadService;

namespace BOI.Core.Web.Services
{
    public interface IFileUploadService
    {
        Task<SaveResponseDto> SaveFile(IFormFile file);
        Task<SaveResponseDto> ValidateAndSaveFile(IFormFile file, FileSaveType modelType);
        IEnumerable<T> GetFileData<T>(string path);
    }
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ILogger<FileUploadService> logger;
        private static string FileUploadPath = "App_Data\\TEMP\\CSVUploads";
        private static readonly IEnumerable<string> AllowedExtensionProduct = new List<string>() { ".csv", ".json" };
        private static readonly IEnumerable<string> AllowedExtensionSolicitor = new List<string>() { ".csv" };
        private IEnumerable<string> AllowedExtensionsBDM = new[] { ".csv", ".xlsx" };


        public FileUploadService(IWebHostEnvironment webHostEnvironment, ILogger<FileUploadService> logger)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.logger = logger;
        }

        public async Task<SaveResponseDto> SaveFile(IFormFile file)
        {
            try
            {
                var uploadFolder = Path.Combine(webHostEnvironment.ContentRootPath, FileUploadPath);
                var filePath = Path.Combine(uploadFolder, file.FileName);

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return new SaveResponseDto { FilePath = filePath };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving file: {FileName}", file.FileName);
                return new SaveResponseDto { Errors = new List<string> { "Error saving file: " + file.FileName } };
            }
        }


        public enum FileSaveType
        {
            Product=1,Solicitor=2,BDM=3,CSV=4//,XLS=5,XLSX=6,XML=7
        }
        public async Task<SaveResponseDto> ValidateAndSaveFile(IFormFile file, FileSaveType modelType)
        {
            if (file == null)
                return new SaveResponseDto { Errors = new List<string> { "File cannot be null." } };

            switch (modelType)
            {
                case FileSaveType.CSV:
                    if (string.Equals(Path.GetExtension(file.FileName), ".csv", StringComparison.InvariantCultureIgnoreCase))
                        return new SaveResponseDto { Errors = new List<string> { "File must be a valid csv" } };
                    break;
                case FileSaveType.Product:
                    if (!AllowedExtensionProduct.Any(x => x.Equals(Path.GetExtension(file.FileName).ToLower())))
                        return new SaveResponseDto { Errors = new List<string> { "File must be a valid csv or json file" } };
                    break;
                case FileSaveType.Solicitor:
                    if (!AllowedExtensionSolicitor.Any(x => x.Equals(Path.GetExtension(file.FileName).ToLower())))
                        return new SaveResponseDto { Errors = new List<string> { "File must be a valid csv file" } };
                    break;
                case FileSaveType.BDM:
                    
                    if (!AllowedExtensionsBDM.Any(x => x.Equals(Path.GetExtension(file.FileName).ToLower())))
                        return new SaveResponseDto { Errors = new List<string> { "File must be a valid csv file" } };

                    break;
                default:
                    break;

            }

            return await SaveFile(file);
        }

        public IEnumerable<T> GetFileData<T>(string path)
        {
            using var stream = File.OpenText(path);
            switch (Path.GetExtension(path))
            {
                case ".csv":
                    try
                    {
                        var csvConfig =
                       new CsvConfiguration(CultureInfo.InvariantCulture)
                       {
                           Delimiter = ",",
                           IgnoreBlankLines = true,
                           HeaderValidated = null,
                           HasHeaderRecord = true
                       };
                        using var csv = new CsvReader(stream, csvConfig);
                        return csv.GetRecords<T>().ToList();
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                case ".json":
                    try
                    {
                        StreamReader r = new StreamReader(path);
                        string jsonString = r.ReadToEnd();
                        var records = JsonConvert.DeserializeObject<List<T>>(jsonString);
                        return records;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                default:
                    return Enumerable.Empty<T>();
            }
        }

    }
}
