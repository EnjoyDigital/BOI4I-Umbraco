
using BOI.Core.Extensions;
using CsvHelper;
using CsvHelper.Configuration;
using FastExcel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace BOI.Core.Web.Commands
{
    public class ContentImporter : IContentImporter
    {
        private readonly IContentService contentService;
        private readonly ILogger logger;
        private readonly IConfiguration configuration;
        private readonly string DataSeparator;


        public ContentImporter(IContentService contentService, ILogger<ContentImporter> logger, IConfiguration configuration)
        {
            this.contentService = contentService;
            this.logger = logger;
            this.configuration = configuration;
            DataSeparator = configuration.GetValue<string>("bdmDataSeparator");
        }

        public ImporterResponse ProcessImportFile(ImporterRequest importRequest)
        {
            ValidateRequest(importRequest);

            var importerResponse = new ImporterResponse();
            var importData = GetFileData(importRequest, importerResponse);

            ValidateFileData(importRequest, importData, importerResponse);

            if (importerResponse.FileValidationMessages.Count() > 0)
            {
                return importerResponse;
            }

            foreach (DataRow bdmRow in importData.Rows)
            {
                var skip = false;
                if (importRequest.IgnoreImportItemsByColumnAlias.NotNullAndAny())
                {
                    foreach (var ignore in importRequest.IgnoreImportItemsByColumnAlias)
                    {
                        foreach (var checkValue in ignore.Value)
                        {
                            if (string.Equals(bdmRow.Field<string>(ignore.Key), checkValue))
                            {
                                skip = true;
                                continue;
                            }
                        }

                    }
                }
                if (skip)
                {
                    continue;
                }
                IContent content;

                string nodeName = string.Join(" ", importRequest.NodeNameProperties.Select(x => bdmRow.Field<string>(x)));
                if (!nodeName.HasValue())
                {
                    logger.LogWarning($"no name from import data forrow index {importData.Rows.IndexOf(bdmRow)}");
                    continue;
                }

                if (importRequest.PropertyAliasToMatchContentItem.HasValue())
                {
                    var matchValue = bdmRow.Field<string>(importRequest.PropertyAliasToMatchContentItem)?.ToString();
                    //TODO: refactor this "query" as a child item may be unpublished for some reason and may cause duplication. 
                    //reused from existing code, would be nice to replace with examine query on the internal index
                    var currentBdm = importRequest.UmbracoParentContainer.Children.FirstOrDefault(x => string.Equals(x.GetProperty(importRequest.PropertyAliasToMatchContentItem)?.GetValue()?.ToString().Trim(), matchValue, StringComparison.InvariantCultureIgnoreCase));
                    if (currentBdm == null)
                    {

                        content = contentService.Create(nodeName, importRequest.UmbracoParentContainer.Key, importRequest.NodeTypeAlias);
                    }
                    else
                    {
                        content = contentService.GetById(currentBdm.Id);
                    }
                }
                else
                {
                    content = contentService.Create(nodeName, importRequest.UmbracoParentContainer.Key, importRequest.NodeTypeAlias);
                }

                if (content == null)
                {
                    logger.LogWarning($"Content not found or created. Row index {importData.Rows.IndexOf(bdmRow)}");
                    continue;
                }


                foreach (var kv in importRequest.ImportFileColumnMapping.Where(x => !importRequest.TagAssigningColumnNames.ContainsKey(x.Value) && x.Key > 0))
                {
                    content.SetValue(kv.Value, bdmRow.Field<string>(kv.Value)?.Trim());
                }

               

                try
                {
                    contentService.SaveAndPublish(content);
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"Content not found or created. Row index {importData.Rows.IndexOf(bdmRow)}");
                }


            }




            return importerResponse;
        }

        private void ValidateFileData(ImporterRequest importRequest, DataTable importData, ImporterResponse importerResponse)
        {
            var fileValidationMessages = new List<string>();

            if (importData.Columns.Count < importRequest.ImportFileColumnMapping.Count())
            {
                fileValidationMessages.Add("Insufficent columns");

                logger.LogInformation($"Datatable columns {importData.Columns.Cast<DataColumn>().Select(x => x.ColumnName)}. Expected");
            }


            foreach (var requestedColumn in importRequest.ImportFileColumnMapping.Where(x => x.Key > 0))
            {
                if (!importData.Columns.Contains(requestedColumn.Value))
                {
                    fileValidationMessages.Add($"{requestedColumn.Value} not retrieved from import");
                }
            }

            importerResponse.FileValidationMessages = fileValidationMessages;
        }

        private void ValidateRequest(ImporterRequest importRequest)
        {

            if (importRequest.NodeNameProperties.NullOrEmpty())
            {
                throw new ArgumentNullException("Node name property required");
            }

            if (importRequest.ImportFileColumnMapping.NullOrEmpty())
            {
                throw new ArgumentNullException("Import File Column Mapping property required");
            }

            if (!importRequest.FileNamePath.HasValue())
            {
                throw new ArgumentNullException("File path required");
            }

            if (!System.IO.File.Exists(importRequest.FileNamePath))
            {
                throw new ArgumentNullException($"File {importRequest.FileNamePath} not found");
            }

            if (!importRequest.NodeTypeAlias.HasValue())
            {
                throw new ArgumentNullException("Import FileColumn Mapping property required");
            }

            if (importRequest.UmbracoParentContainer == null)
            {
                throw new ArgumentNullException("Parent content required");
            }

        }

        //private IContent CreateNewContent(DataRow bdmRow, ImporterRequest importRequest)
        //{


        //}

        /// <summary>
        /// Returns a datatable of the file data, with the proposed mapping of properties
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private DataTable GetFileData(ImporterRequest request, ImporterResponse importerResponse)
        {
            var importData = new DataTable("Import Data");
            var fileProcessingErrors = new List<string>();
            if (System.IO.File.Exists(request.FileNamePath))
            {
                var headings = new Dictionary<int, string>();
                //set the columns up on the Datatable
                //Removing blanked columns fro import
                request.ImportFileColumnMapping = request.ImportFileColumnMapping.Where(col => col.Value.HasValue()).ToDictionary(x => x.Key, y => y.Value);
                foreach (var kv in request.ImportFileColumnMapping.Where(x => x.Key > 0))
                {
                    importData.Columns.Add(kv.Value);

                }

                switch (Path.GetExtension(request.FileNamePath))
                {

                    case ".csv":
                        try
                        {
                            using (var reader = new StreamReader(request.FileNamePath))
                            {
                                var csvConfig =
                       new CsvConfiguration(CultureInfo.InvariantCulture)
                       {
                           Delimiter = ",",
                           IgnoreBlankLines = true,
                           HeaderValidated = null,
                           HasHeaderRecord = true
                       };
                                using (var csv = new CsvReader(reader, csvConfig))
                                {
                                    
                                    csv.Read();
                                    csv.ReadHeader();

                                    while (csv.Read())
                                    {

                                        DataRow importRow = null;
                                        if (request.PropertyAliasToMatchContentItem.HasValue())
                                        {
                                            //get the column index from
                                            var columnIndex = request.ImportFileColumnMapping.FirstOrDefault(x => x.Value == request.PropertyAliasToMatchContentItem);

                                            //If the item already exists, merge the data for specified columns
                                            importRow = importData.Select($"{request.PropertyAliasToMatchContentItem} = '{csv.GetField(columnIndex.Key - 1)}' ").FirstOrDefault();

                                            if (importRow != null && request.MergeColumns.NotNullAndAny())
                                            {

                                                foreach (var mergeCol in request.MergeColumns)
                                                {
                                                    columnIndex = request.ImportFileColumnMapping.FirstOrDefault(x => x.Value == mergeCol);

                                                    importRow[mergeCol] = string.Concat(importRow[mergeCol], DataSeparator, csv.GetField(columnIndex.Key - 1));
                                                }

                                                continue;
                                            }

                                        }

                                        if (importRow == null)
                                        {
                                            importRow = importData.NewRow();
                                        }

                                        foreach (var kv in request.ImportFileColumnMapping)
                                        {

                                            importRow[kv.Value] = csv.GetField(kv.Key - 1);



                                        }

                                        importData.Rows.Add(importRow);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "");
                            
                            throw ex;
                        }


                        break;



                    case ".xlsx":

                        try
                        {
                            // Get the input file path
                            var inputFile = new FileInfo(request.FileNamePath);

                            //Create a worksheet
                            Worksheet worksheet = null;


                            // Create an instance of Fast Excel
                            using (FastExcel.FastExcel fastExcel = new FastExcel.FastExcel(inputFile, true))
                            {

                                // Read the rows using the worksheet index
                                // Worksheet indexes are start at 1 not 0
                                // This method is slightly faster to find the underlying file (so slight you probably wouldn't notice)
                                worksheet = fastExcel.Read(1);



                                //translate index to the current heading

                                var headingRow = worksheet.Rows.FirstOrDefault();
                                foreach (var kv in request.ImportFileColumnMapping)
                                {

                                    headings.Add(kv.Key, headingRow.Cells.ElementAt(kv.Key - 1).ColumnName);
                                }


                                //remove heading row
                                worksheet.Rows = worksheet.Rows.Skip(1);

                                foreach (var row in worksheet.Rows)
                                {

                                    DataRow importRow = null;
                                    if (request.PropertyAliasToMatchContentItem.HasValue())
                                    {
                                        //get the column index from
                                        var columnIndex = request.ImportFileColumnMapping.FirstOrDefault(x => x.Value == request.PropertyAliasToMatchContentItem);
                                        if (row.Cells.Count() >= columnIndex.Key)
                                        {
                                            //If the item already exists, merge the data for specified columns
                                            importRow = importData.Select($"{request.PropertyAliasToMatchContentItem} = '{row.GetCellByColumnName(headings[columnIndex.Key])}' ").FirstOrDefault();

                                            if (importRow != null && request.MergeColumns.NotNullAndAny())
                                            {

                                                foreach (var mergeCol in request.MergeColumns)
                                                {
                                                    columnIndex = request.ImportFileColumnMapping.FirstOrDefault(x => x.Value == mergeCol);

                                                    importRow[mergeCol] = string.Concat(importRow[mergeCol], ",", row.GetCellByColumnName(headings[columnIndex.Key]));
                                                }

                                                continue;
                                            }
                                        }
                                    }

                                    if (importRow == null)
                                    {
                                        importRow = importData.NewRow();
                                    }

                                    foreach (var kv in request.ImportFileColumnMapping)
                                    {
                                        //if(row.Cells.Count() >= kv.Key )
                                        //{
                                        var cellValue = row.GetCellByColumnName(headings[kv.Key])?.ToString();
                                        if (cellValue != null)
                                        {
                                            importRow[kv.Value] = cellValue;
                                        }
                                        else
                                        {
                                            importRow[kv.Value] = string.Empty;
                                        }


                                        //}
                                        //else
                                        //{
                                        //    importRow[kv.Value] = string.Empty;
                                        //}

                                    }

                                    importData.Rows.Add(importRow);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "");
                           
                            throw ex;
                        }


                        break;
                }


            }
            return importData;
        }


    }

    public class ImporterRequest
    {
        public ImporterRequest()
        {
            ImportFileColumnMapping = new Dictionary<int, string>();
            NodeNameProperties = Enumerable.Empty<string>();
            TagAssigningColumnNames = new Dictionary<string, string>();
            IgnoreImportItemsByColumnAlias = new Dictionary<string, IEnumerable<string>>();

            MergeColumns = Enumerable.Empty<string>();
        }
        public string FileNamePath { get; set; }

        /// <summary>
        /// Parent of data being imported
        /// </summary>
        public IPublishedContent UmbracoParentContainer { get; set; }

        /// <summary>
        /// Node type of data being imported
        /// </summary>
        public string NodeTypeAlias { get; set; }


        /// <summary>
        /// Key is files column index, value is the property alias for the content field, 
        /// which is set as the column name on the internal processing datatable
        /// </summary>
        public IDictionary<int, string> ImportFileColumnMapping { get; set; }

        /// <summary>
        /// Key will match the imported column alias. if the given value matches the data valueand ignore if match
        /// </summary>
        public IDictionary<string, IEnumerable<string>> IgnoreImportItemsByColumnAlias { get; set; }

        /// <summary>
        /// Value should match content property alias being set on the content, Key should match the appropriate alias provided 
        /// in ImportFileColumnMapping that is extracted from the file
        /// </summary>
        public IDictionary<string, string> TagAssigningColumnNames { get; set; }

        /// <summary>
        /// Property alias and column name of props to use for name. If more than one specified, will be concatonated with space
        /// </summary>
        public IEnumerable<string> NodeNameProperties { get; set; }

        /// <summary>
        /// Property alias and column name of props to merge on duplicate data, for when data is flattened and causes mutliple entries
        /// for an item i.e one to many relationships
        /// </summary>
        public IEnumerable<string> MergeColumns { get; set; }
        /// <summary>
        /// This is intended to enable updating existing content items. If empty, new content will be created
        /// </summary>
        public string PropertyAliasToMatchContentItem { get; set; }

    }

    public class ImporterResponse
    {
        public IEnumerable<string> FileValidationMessages { get; set; }
    }

}
