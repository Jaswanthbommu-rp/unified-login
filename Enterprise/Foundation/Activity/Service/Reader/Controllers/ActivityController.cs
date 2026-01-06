using Aspose.Cells;
using RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Helper;
using RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Models;
using RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Repository;
using Serilog;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ActivityDetailMessage = RP.Enterprise.Foundation.Activity.Service.Logging.Shared.Models.ActivityDetailMessage;
using ActivityDetailMessageV2 = RP.Enterprise.Foundation.Activity.Service.Logging.Shared.Models.ActivityDetailMessageV2;


namespace RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Controllers
{
    public class ActivityController : BaseApiController
    {
        #region Public Methods

        /// <summary>
        /// List diffrent search metadata to populate drop-down boxes on UI
        /// </summary>  
        /// <returns>A list of Organization(s) Details for a person</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List different search metadata to populate drop-down boxes on UI", Type = typeof(SearchMetadata))]
        [Route("api/listsearchmetadata")]
        [HttpGet]
        public HttpResponseMessage ListSearchMetadata()
        {
            try
            {
                var readerRepository = new ReaderRepository();
                var result = readerRepository.GetSearchMetadata();

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(exception: ex);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// List activity log based on search criteria
        /// </summary>
        /// <param name="filterCriteria">Activity Log Filter Criteria</param>
        /// <returns>Response message including the status code and data</returns>
        //[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        //[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        //[SwaggerResponse(HttpStatusCode.OK, Description = "List activity by criteria", Type = typeof(ActivityDetailMessage))]
        //[Route("api/listactivitylog")]
        //[HttpPost]
        //public HttpResponseMessage ListActivityLog(ActivityLogFilterCriteria filterCriteria)
        //{
        //    try
        //    {
        //        Log.Information($"Getting Activity Log");

        //        if (filterCriteria == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, "No filterCriteria received.");
        //        }

        //        ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
        //        if (currentClaimPrincipal.Identity.IsAuthenticated)
        //        {
        //            var orgFilter = filterCriteria.ActivitySearchCriteria.Where(x => x.Value.Equals("OrganizationPartyId", StringComparison.OrdinalIgnoreCase));
        //            if (!orgFilter.Any()) // If OrganizationPartyId passed then bypass adding from claim
        //            {
        //                filterCriteria.ActivitySearchCriteria.Add(
        //                    new ActivitySearchCriteria
        //                    {
        //                        Name = "OrganizationPartyId",
        //                        Value = _userClaims.OrganizationPartyId.ToString()
        //                    });
        //            }

        //            ;
        //        }

        //        var readerRepository = new ReaderRepository();
        //        var result = readerRepository.ListActivityLog(filterCriteria);

        //        if (result != null)
        //        {
        //            ObjectListOutput<ActivityDetailMessage, IErrorData> output = new ObjectListOutput<ActivityDetailMessage, IErrorData>() {list = result};
        //            return Request.CreateResponse(HttpStatusCode.OK, output);
        //        }

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, "No data.");
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteToErrorLog(exception: ex);
        //        throw new HttpResponseException(HttpStatusCode.InternalServerError);
        //    }
        //}

        /// <summary>
        /// Export activity log based on search criteria
        /// </summary>
        /// <param name="filterCriteria">Activity Log Filter Criteria</param>
        /// <returns>Response message including the status code and data</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Export activity log by search criteria in excel.", Type = typeof(ActivityDetailMessage))]
        [Route("api/exportactivitylog")]
        [HttpPost]
        public HttpResponseMessage ExportActivityLog(ActivityLogFilterCriteria filterCriteria, bool isArchived = false)
        {
            try
            {
                byte[] plainBytes;
                SaveFormat saveFormat = SaveFormat.CSV;

                ObjectOutput<string, IErrorData> output = new ObjectOutput<string, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();

                if (filterCriteria == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No filterCriteria received.");
                }

                var orgFilter = filterCriteria.ActivitySearchCriteria.Where(x => x.Value.Equals("OrganizationPartyId", StringComparison.OrdinalIgnoreCase));
                if (!orgFilter.Any()) // If OrganizationPartyId passed then bypass adding from claim
                {
                    filterCriteria.ActivitySearchCriteria.Add(
                        new ActivitySearchCriteria
                        {
                            Name = "OrganizationPartyId",
                            Value = _userClaims.OrganizationPartyId.ToString()
                        });
                }

                ReaderRepository readerRepository = new ReaderRepository();
                bool includRPEmployeeActivity = _userClaims.IsImpersonated;
                var results = readerRepository.ListActivityLogDetails(filterCriteria, isArchived, includRPEmployeeActivity);
                IList<ActivityDetailMessage> listActivityDetailMessage = results.Records;

                if (listActivityDetailMessage != null)
                {
                    errorStatus = SetAsposeLicense();
                    if (errorStatus.Success)
                    {
                        string internationalDateFormat, internationalTimeFormat;
                        GetInternationalDateTimeFormat(filterCriteria, out internationalDateFormat, out internationalTimeFormat);
                        string dateFormat = $"{internationalDateFormat} {internationalTimeFormat}";

                        saveFormat = filterCriteria.DataFormat;
                        plainBytes = ExportActivityData(listActivityDetailMessage, saveFormat, dateFormat);
                        output = new ObjectOutput<string, IErrorData>()
                        {
                            obj = Convert.ToBase64String(plainBytes),
                            Status = errorStatus
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, output);
                    }
                    else
                    {
                        output.Status = errorStatus;
                        return Request.CreateResponse(HttpStatusCode.OK, output);
                    }
                }
                else
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "Activity.ExportActivityLog.1";
                    errorStatus.ErrorMsg = "List Activities Export: No data";
                    output.Status = errorStatus;
                    return Request.CreateResponse(HttpStatusCode.OK, output);
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog(exception: ex);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Export activity log based on search criteria V2
        /// </summary>
        /// <param name="filterCriteria">Activity Log Filter Criteria</param>
        /// <returns>Response message including the status code and data</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Export activity log by search criteria in excel.", Type = typeof(ActivityDetailMessageV2))]
        [Route("api/v2/exportactivitylog")]
        [HttpPost]
        public HttpResponseMessage ExportActivityLogV2(ExportActivityLogRequest exportActivityLogRequest, bool isArchived = false)
        {
            try
            {
                byte[] plainBytes;
                SaveFormat saveFormat = SaveFormat.CSV;

                ObjectOutput<string, IErrorData> output = new ObjectOutput<string, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();

                if (exportActivityLogRequest.FilterCriteria == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No filterCriteria received.");
                }

                var orgFilter = exportActivityLogRequest.FilterCriteria.ActivitySearchCriteria.Where(x => x.Value.Equals("OrganizationPartyId", StringComparison.OrdinalIgnoreCase));
                if (!orgFilter.Any()) // If OrganizationPartyId passed then bypass adding from claim
                {
                    exportActivityLogRequest.FilterCriteria.ActivitySearchCriteria.Add(
                        new ActivitySearchCriteria
                        {
                            Name = "OrganizationPartyId",
                            Value = _userClaims.OrganizationPartyId.ToString()
                        });
                }

                ;

                ReaderRepository readerRepository = new ReaderRepository();
                bool includRPEmployeeActivity = _userClaims.IsImpersonated;
                var results = readerRepository.ListActivityLogDetailsV2(exportActivityLogRequest.FilterCriteria, isArchived, includRPEmployeeActivity);
                IList<ActivityDetailMessageV2> listActivityDetailMessage = results.Records;

                if (listActivityDetailMessage != null)
                {
                    errorStatus = SetAsposeLicense();
                    if (errorStatus.Success)
                    {
                        string internationalDateFormat, internationalTimeFormat;
                        GetInternationalDateTimeFormat(exportActivityLogRequest.FilterCriteria, out internationalDateFormat, out internationalTimeFormat);
                        string dateFormat = $"{internationalDateFormat} {internationalTimeFormat}";

                        saveFormat = exportActivityLogRequest.FilterCriteria.DataFormat;
                        plainBytes = ExportActivityDataV2(listActivityDetailMessage, exportActivityLogRequest.ColumnMappings, saveFormat, dateFormat);
                        output = new ObjectOutput<string, IErrorData>()
                        {
                            obj = Convert.ToBase64String(plainBytes),
                            Status = errorStatus
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, output);
                    }
                    else
                    {
                        output.Status = errorStatus;
                        return Request.CreateResponse(HttpStatusCode.OK, output);
                    }
                }
                else
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "Activity.ExportActivityLog.1";
                    errorStatus.ErrorMsg = "List Activities Export: No data";
                    output.Status = errorStatus;
                    return Request.CreateResponse(HttpStatusCode.OK, output);
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog(exception: ex);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Export activity log based on search criteria
        /// </summary>
        /// <param name="filterCriteria">Activity Log Filter Criteria</param>
        /// <returns>Response message including the status code and data</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Export activity log details.", Type = typeof(ActivityDetailMessage))]
        [Route("api/activitylogs/{activityId}/export-details")]
        [HttpPost]
        public HttpResponseMessage ExportActivityLogDetails(ActivityLogDetailExportRequest activityLogDetailExportRequest)
        {
            try
            {
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                ObjectOutput<string, IErrorData> output = new ObjectOutput<string, IErrorData>();

                errorStatus = SetAsposeLicense();

                if (errorStatus.Success)
                {
                    var plainBytes = ExportActivityDetailData(activityLogDetailExportRequest);
                    output = new ObjectOutput<string, IErrorData>()
                    {
                        obj = Convert.ToBase64String(plainBytes),
                        Status = errorStatus
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, output);
                }
                else
                {
                    output.Status = errorStatus;
                    return Request.CreateResponse(HttpStatusCode.OK, output);
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog(exception: ex);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Write activity log
        /// </summary>
        /// <param name="activityDetailMessage">Activity Detail Message</param>
        /// <returns>Response message including the status code and data</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Export activity log by search criteria in excel.", Type = typeof(ActivityDetailMessage))]
        [Route("api/write")]
        [HttpPost]
        public HttpResponseMessage WriteActivityLog(ActivityDetailMessage activityDetailMessage)
        {
            try
            {
                if (activityDetailMessage == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No activityDetailMessage received.");
                }

                if (activityDetailMessage.OrganizationPartyId == 0)
                {
                    activityDetailMessage.OrganizationPartyId = _userClaims.OrganizationPartyId;
                }

                activityDetailMessage.ApplicationTimestamp = DateTime.UtcNow;

                if (string.IsNullOrEmpty(ConfigReader.ActivityMQName))
                {
                    throw new Exception($"ActivityMQName is missing check config file.");
                }

                using (var queue = new MessageQueue(ConfigReader.ActivityMQName))
                {
                    var logMessage = new Message(activityDetailMessage);
                    queue.Send(logMessage);
                }
            }
            catch (Exception ex)
            {
                // log exception in elastic
                Log.Error(exception: ex, messageTemplate: "{ActionName} - {state}", propertyValue0: "WriteToErrorLog", propertyValue1: $"Exception in Activity Logging. Reason: {ex.Message}");

                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError);
            }

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Returns additional details (key-value) data for particular activity.", Type = typeof(List<Shared.Models.AdditionalParameters>))]
        [Route("api/additionalparams")]
        [HttpGet]
        public HttpResponseMessage ListActivityAdditionalParams(long activityId, bool isArchived = false)
        {
            try
            {
                if (activityId == 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "0 activity Id received.");

                var readerRepository = new ReaderRepository();
                var result = readerRepository.ListActivityAdditionalParams(activityId, isArchived);

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(exception: ex);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Used to delete all activity information for the given organization party id
        /// </summary>
        /// <param name="organizationPartyId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Invalid request made")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Log removed")]
        [Route("api/activity/organization/{OrganizationPartyId}")]
        [HttpDelete]
        [AuthorizeScope("companyfunctions")]
        public HttpResponseMessage DeleteActivityLogForCompany(long organizationPartyId)
        {
            try
            {
                if (organizationPartyId == 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid company id.");

                if (ConfigReader.Environment.Equals("PROD", StringComparison.OrdinalIgnoreCase) || ConfigReader.Environment.Equals("PRODUCTION", StringComparison.OrdinalIgnoreCase))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Environment not supported");
                }

                Log.Information(messageTemplate: "{ActionName} - {state}", propertyValue0: "DeleteActivityLogForCompany", propertyValue1: $"Deleting Activity Log for company {organizationPartyId}");

                var readerRepository = new ReaderRepository();
                var result = readerRepository.DeleteOrganizationActivityLog(organizationPartyId);

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(exception: ex);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// List activity log based on search criteria to send data related to Pagination
        /// </summary>
        /// <param name="filterCriteria">Activity Log Filter Criteria</param>
        /// <returns>Response message including the status code and data</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List activity by criteria to send data related to Pagination", Type = typeof(ListResponse<ActivityDetailMessage>))]
        [Route("api/v1/listactivitylog")]
        [HttpPost]
        public HttpResponseMessage ListActivityLogDetails(ActivityLogFilterCriteria filterCriteria, bool isArchived = false)
        {
            var result = new ListResponse<ActivityDetailMessage>();
            try
            {
                Log.Information(messageTemplate: "{ActionName} - {state}", propertyValue0: "ListActivityLogDetails", propertyValue1: "Getting Activity Log Detail");
                if (filterCriteria == null)
                {
                    result.IsError = true;
                    result.ErrorReason = "No filterCriteria received.";
                    return Request.CreateResponse(HttpStatusCode.BadRequest, result);
                }

                var orgFilter = filterCriteria.ActivitySearchCriteria.Where(x => x.Value.Equals("OrganizationPartyId", StringComparison.OrdinalIgnoreCase));
                if (!orgFilter.Any()) // If OrganizationPartyId passed then bypass adding from claim
                {
                    filterCriteria.ActivitySearchCriteria.Add(
                        new ActivitySearchCriteria
                        {
                            Name = "OrganizationPartyId",
                            Value = _userClaims.OrganizationPartyId.ToString()
                        });
                }

                var readerRepository = new ReaderRepository();
                bool includRPEmployeeActivity = _userClaims.IsImpersonated;
                result = readerRepository.ListActivityLogDetails(filterCriteria, isArchived, includRPEmployeeActivity);

                if (result != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }

                return Request.CreateResponse(HttpStatusCode.InternalServerError, "No data.");
            }
            catch (Exception ex)
            {
                WriteToErrorLog(exception: ex);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Set Aspose License
        /// </summary>
        /// <returns>Error Status object</returns>
        private static Status<IErrorData> SetAsposeLicense()
        {
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            try
            {
                Aspose.Cells.License asposeCellsLicense = new Aspose.Cells.License();
                //Gets the base directory that the assembly resolver uses to probe for assemblies + Aspose license file location
                asposeCellsLicense.SetLicense(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ThirdParty\Aspose.Total.Lic"));
            }
            catch (Exception ex)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Person.SetAsposeLicense.1";
                errorStatus.ErrorMsg = "Set Aspose License: " + ex.Message;
            }

            return errorStatus;
        }

        /// <summary>
        /// Create Excel WorkSheet
        /// </summary>
        /// <param name="workbook">Aspose Cells Workbook</param>
        /// <param name="worksheet">Aspose Cells WorkSheet</param>
        private static void CreateExcelWorkSheet(out Workbook workbook, out Worksheet worksheet)
        {
            //Instantiate a new Workbook
            workbook = new Workbook();
            //Clear all the worksheets
            workbook.Worksheets.Clear();
            //Add a new Sheet "Data"
            worksheet = workbook.Worksheets.Add("Data");
        }

        /// <summary>
        /// Export data in a list in a the the specified format
        /// </summary>
        /// <param name="listActivityDetailMessage">List of activities to export</param>
        /// <param name="dataFormat">Retrun data in this format (default = CSV)</param>
        /// <returns>Array of bytes</returns>
        private byte[] ExportActivityData(IList<ActivityDetailMessage> listActivityDetailMessage, SaveFormat dataFormat = SaveFormat.CSV, string dateFormat = "MM/dd/yyyy hh:mm:ss tt")
        {
            Workbook workbook;
            Worksheet worksheet;
            MemoryStream memorystream = new MemoryStream();

            string[] propertyNames = new string[]
            {
                "LogActivityTypeName", "FromUserFirstName", "FromUserLastName", "FromUserLoginName", "Message", "ApplicationTimestampOffset"
            };

            CreateExcelWorkSheet(out workbook, out worksheet);

            //Manually add the row titles
            int col = 0;
            Cells cells = worksheet.Cells;

            Cell cell = cells[0, col++];
            cell.PutValue("Activity type");

            worksheet.Cells[0, col++].PutValue("First name");
            worksheet.Cells[0, col++].PutValue("Last name");
            worksheet.Cells[0, col++].PutValue("Username");
            worksheet.Cells[0, col++].PutValue("Description");
            worksheet.Cells[0, col++].PutValue("Date");
            int totalColumns = col;
            int dateColumnIndex = col - 1; // Index of the Date column

            // Get the pagesetup object
            PageSetup pageSetup = worksheet.PageSetup;

            // Set bottom,left,right and top page margins
            pageSetup.BottomMarginInch = 0.5;
            pageSetup.LeftMarginInch = 0.25;
            pageSetup.RightMarginInch = 0.25;
            pageSetup.TopMarginInch = 0.5;

            worksheet.Cells.ImportCustomObjects(
                (System.Collections.ICollection)listActivityDetailMessage,
                propertyNames,
                false, //Don't show the field names
                1, //Start at second row
                0,
                listActivityDetailMessage.Count,
                true,
                "",
                false
            );

            // Apply custom date format to the Date column while preserving existing styles
            for (int row = 1; row <= listActivityDetailMessage.Count; row++)
            {
                Cell dateCell = worksheet.Cells[row, dateColumnIndex];
                if (dateCell.Value == null || string.IsNullOrEmpty(dateCell.StringValue))
                {
                    continue;
                }

                Style existingStyle = dateCell.GetStyle();
                existingStyle.Custom = dateFormat;
                dateCell.SetStyle(existingStyle);
            }

            switch (dataFormat)
            {
                case SaveFormat.CSV:
                    //Autofits the columns width
                    workbook.Worksheets[0].AutoFitColumns();
                    break;
                case SaveFormat.Pdf:
                    //Set the width columns
                    col = 0;
                    worksheet.Cells.SetColumnWidthInch(col++, 1);
                    worksheet.Cells.SetColumnWidthInch(col++, 1.25);
                    worksheet.Cells.SetColumnWidthInch(col++, 1.25);
                    worksheet.Cells.SetColumnWidthInch(col++, 2);
                    worksheet.Cells.SetColumnWidthInch(col++, 3);
                    worksheet.Cells.SetColumnWidthInch(col++, 2);

                    //Create a StyleFlag object.
                    StyleFlag styleFlag = new StyleFlag
                    {
                        //Make the corresponding attributes ON.
                        Font = true,
                        VerticalAlignment = true
                    };

                    Style style = workbook.CreateStyle();
                    Range range = worksheet.Cells.CreateRange(0, 0, 1, totalColumns);
                    style.Font.IsBold = true;
                    style.VerticalAlignment = TextAlignmentType.Top;
                    range.ApplyStyle(style, styleFlag);

                    styleFlag = new StyleFlag
                    {
                        WrapText = true,
                        VerticalAlignment = true

                    };
                    range = worksheet.Cells.CreateRange(1, 0, 1048575, totalColumns);
                    style.VerticalAlignment = TextAlignmentType.Top;
                    style.IsTextWrapped = true;
                    range.ApplyStyle(style, styleFlag);

                    foreach (Worksheet sheet in workbook.Worksheets)
                    {
                        sheet.PageSetup.Orientation = PageOrientationType.Landscape;
                        sheet.PageSetup.FitToPagesWide = 1;
                        sheet.PageSetup.FitToPagesTall = 0;
                    }

                    break;
                default:
                    break;
            }

            //Autofits all rows in this worksheet
            workbook.Worksheets[0].AutoFitRows(true);

            //Convert to bytes array
            workbook.Save(memorystream, dataFormat);

            //Get bytes
            byte[] bytes = memorystream.ToArray();

            memorystream.Dispose();
            memorystream.Close();

            return bytes;
        }
        /// <summary>
        /// Export data in a list in a the the specified format
        /// </summary>
        /// <param name="listActivityDetailMessage">List of activities to export</param>
        /// <param name="dataFormat">Retrun data in this format (default = CSV)</param>
        /// <returns>Array of bytes</returns>
        private byte[] ExportActivityDataV2(IList<ActivityDetailMessageV2> listActivityDetailMessage,
           IList<ColumnMapping> columnMappings, SaveFormat dataFormat = SaveFormat.CSV, string dateFormat = "MM/dd/yyyy hh:mm:ss tt")
        {
            Workbook workbook;
            Worksheet worksheet;
            MemoryStream memorystream = new MemoryStream();

            CreateExcelWorkSheet(out workbook, out worksheet);

            //Manually add the row titles
            int col = 0;

            var propertyNames = new List<string>();
            foreach (var columnMapping in columnMappings)
            {
                propertyNames.Add(columnMapping.Key);
                worksheet.Cells[0, col++].PutValue(columnMapping.Label);

                if (dataFormat == SaveFormat.Pdf)
                {
                    worksheet.Cells.SetColumnWidthInch(col - 1, columnMapping.Width);
                }
            }

            int totalColumns = col;

            // Get the pagesetup object
            PageSetup pageSetup = worksheet.PageSetup;

            // Set bottom,left,right and top page margins
            pageSetup.BottomMarginInch = 0.5;
            pageSetup.LeftMarginInch = 0.25;
            pageSetup.RightMarginInch = 0.25;
            pageSetup.TopMarginInch = 0.5;

            // Parse the Value property and set OldValue and NewValue
            foreach (var item in listActivityDetailMessage)
            {
                if (!string.IsNullOrWhiteSpace(item.Value))
                {
                    try
                    {
                        var jsonObject = Newtonsoft.Json.Linq.JObject.Parse(item.Value);
                        item.OldValue = jsonObject["old"]?.ToString();
                        item.NewValue = jsonObject["new"]?.ToString();
                    }
                    catch (Exception)
                    {
                        // Handle JSON parsing error if needed
                    }
                }
            }

            worksheet.Cells.ImportCustomObjects(
                (System.Collections.ICollection)listActivityDetailMessage,
                propertyNames.ToArray(),
                false, //Don't show the field names
                1, //Start at second row
                0,
                listActivityDetailMessage.Count,
                true,
                "",
                false
            );

            // Apply custom date format to date columns
            int dateColumnIndex = -1;
            for (int i = 0; i < columnMappings.Count; i++)
            {
                if (columnMappings[i].Label.Equals("Date", StringComparison.OrdinalIgnoreCase))
                {
                    dateColumnIndex = i;
                    break;
                }
            }

            if (dateColumnIndex >= 0)
            {
                for (int row = 1; row <= listActivityDetailMessage.Count; row++)
                {
                    Cell dateCell = worksheet.Cells[row, dateColumnIndex];
                    if (dateCell.Value == null || string.IsNullOrEmpty(dateCell.StringValue))
                    {
                        continue;
                    }

                    Style existingStyle = dateCell.GetStyle();
                    existingStyle.Custom = dateFormat;
                    dateCell.SetStyle(existingStyle);
                }
            }

            switch (dataFormat)
            {
                case SaveFormat.CSV:
                    //Autofits the columns width
                    workbook.Worksheets[0].AutoFitColumns();
                    break;
                case SaveFormat.Pdf:

                    //Create a StyleFlag object.
                    StyleFlag styleFlag = new StyleFlag
                    {
                        //Make the corresponding attributes ON.
                        Font = true,
                        VerticalAlignment = true
                    };

                    Style style = workbook.CreateStyle();
                    Range range = worksheet.Cells.CreateRange(0, 0, 1, totalColumns);
                    style.Font.IsBold = true;
                    style.VerticalAlignment = TextAlignmentType.Top;
                    range.ApplyStyle(style, styleFlag);

                    styleFlag = new StyleFlag
                    {
                        WrapText = true,
                        VerticalAlignment = true

                    };
                    range = worksheet.Cells.CreateRange(1, 0, 1048575, totalColumns);
                    style.VerticalAlignment = TextAlignmentType.Top;
                    style.IsTextWrapped = true;
                    range.ApplyStyle(style, styleFlag);

                    foreach (Worksheet sheet in workbook.Worksheets)
                    {
                        sheet.PageSetup.Orientation = PageOrientationType.Landscape;
                        sheet.PageSetup.FitToPagesWide = 1;
                        sheet.PageSetup.FitToPagesTall = 0;
                    }

                    break;
                default:
                    break;
            }

            //Autofits all rows in this worksheet
            workbook.Worksheets[0].AutoFitRows(true);

            //Convert to bytes array
            workbook.Save(memorystream, dataFormat);

            //FileStream file = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Export." + dataFormat.ToString()), FileMode.Create, FileAccess.Write);
            //memorystream.WriteTo(file);
            //file.Close();

            //Get bytes
            byte[] bytes = memorystream.ToArray();

            memorystream.Dispose();
            memorystream.Close();

            return bytes;
        }

        private byte[] ExportActivityDetailData(ActivityLogDetailExportRequest activityLogDetailExportRequest)
        {
            Workbook workbook;
            Worksheet worksheet;
            MemoryStream memorystream = new MemoryStream();

            string[] propertyNames = activityLogDetailExportRequest.HeaderColumns
                .Select(h => h.Key).ToArray();
            int totalColumns = activityLogDetailExportRequest.HeaderColumns.Count();

            CreateExcelWorkSheet(out workbook, out worksheet);

            for (var i = 0; i < totalColumns; i++)
            {
                var headerColumn = activityLogDetailExportRequest.HeaderColumns.ElementAt(i);
                worksheet.Cells[0, i].PutValue(headerColumn.Header);
            }

            // Get the pagesetup object
            PageSetup pageSetup = worksheet.PageSetup;

            // Set bottom,left,right and top page margins
            pageSetup.BottomMarginInch = 0.5;
            pageSetup.LeftMarginInch = 0.25;
            pageSetup.RightMarginInch = 0.25;
            pageSetup.TopMarginInch = 0.5;

            var rowIndex = 1;
            foreach (var row in activityLogDetailExportRequest.RowData)
            {
                for (var i = 0; i < totalColumns; i++)
                {
                    var headerColumn = activityLogDetailExportRequest.HeaderColumns.ElementAt(i);
                    var rowColumnValue = row.ContainsKey(headerColumn.Key) ? row[headerColumn.Key] : "";
                    worksheet.Cells[rowIndex, i].PutValue(rowColumnValue);
                }
                rowIndex++;
            }

            switch (activityLogDetailExportRequest.DataFormat)
            {
                case SaveFormat.CSV:
                    //Autofits the columns width
                    workbook.Worksheets[0].AutoFitColumns();
                    break;
                case SaveFormat.Pdf:
                    for (var i = 0; i < totalColumns; i++)
                    {
                        var headerColumn = activityLogDetailExportRequest.HeaderColumns.ElementAt(i);
                        worksheet.Cells.SetColumnWidthInch(i, headerColumn.Width);
                    }

                    //Create a StyleFlag object.
                    StyleFlag styleFlag = new StyleFlag
                    {
                        //Make the corresponding attributes ON.
                        Font = true,
                        VerticalAlignment = true
                    };

                    Style style = workbook.CreateStyle();
                    Aspose.Cells.Range range = worksheet.Cells.CreateRange(0, 0, 1, totalColumns);
                    style.Font.IsBold = true;
                    style.VerticalAlignment = TextAlignmentType.Top;
                    range.ApplyStyle(style, styleFlag);

                    styleFlag = new StyleFlag
                    {
                        WrapText = true,
                        VerticalAlignment = true

                    };
                    range = worksheet.Cells.CreateRange(1, 0, 1048575, totalColumns);
                    style.VerticalAlignment = TextAlignmentType.Top;
                    style.IsTextWrapped = true;
                    range.ApplyStyle(style, styleFlag);

                    foreach (Worksheet sheet in workbook.Worksheets)
                    {
                        sheet.PageSetup.Orientation = PageOrientationType.Landscape;
                        sheet.PageSetup.FitToPagesWide = 1;
                        sheet.PageSetup.FitToPagesTall = 0;
                    }
                    break;
                default:
                    break;
            }

            //Autofits all rows in this worksheet
            workbook.Worksheets[0].AutoFitRows(true);

            //Convert to bytes array           
            workbook.Save(memorystream, activityLogDetailExportRequest.DataFormat);

            //Get bytes
            byte[] bytes = memorystream.ToArray();

            memorystream.Dispose();
            memorystream.Close();

            return bytes;
        }

        private static void GetInternationalDateTimeFormat(ActivityLogFilterCriteria filterCriteria, out string internationalDateFormat, out string internationalTimeFormat)
        {
            var dateFormatCriteria = filterCriteria.ActivitySearchCriteria
                .FirstOrDefault(x => x.Name.Equals("InternationalDateFormat", StringComparison.OrdinalIgnoreCase));
            var timeFormatCriteria = filterCriteria.ActivitySearchCriteria
                .FirstOrDefault(x => x.Name.Equals("InternationalTimeFormat", StringComparison.OrdinalIgnoreCase));

            internationalDateFormat = dateFormatCriteria?.Value ?? "MM/dd/yyyy";
            internationalTimeFormat = timeFormatCriteria?.Value ?? "hh:mm tt";

            // Convert time format value to actual format string
            if (internationalTimeFormat.Equals("12Hours", StringComparison.OrdinalIgnoreCase))
            {
                internationalTimeFormat = "hh:mm tt";
            }
            else if (internationalTimeFormat.Equals("24Hours", StringComparison.OrdinalIgnoreCase))
            {
                internationalTimeFormat = "HH:mm";
            }
        }

        #endregion
    }
}