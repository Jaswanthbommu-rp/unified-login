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
using System.Security.Claims;
using System.Web.Http;
using ActivityDetailMessage = RP.Enterprise.Foundation.Activity.Service.Logging.Shared.Models.ActivityDetailMessage;


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
        public HttpResponseMessage ExportActivityLog(ActivityLogFilterCriteria filterCriteria)
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

                ;

                ReaderRepository readerRepository = new ReaderRepository();
                var results = readerRepository.ListActivityLogDetails(filterCriteria);
                IList<ActivityDetailMessage> listActivityDetailMessage = results.Records;
                
                if (listActivityDetailMessage != null)
                {
                    errorStatus = SetAsposeLicense();
                    if (errorStatus.Success)
                    {
                        saveFormat = filterCriteria.DataFormat;
                        plainBytes = ExportActivityData(listActivityDetailMessage, saveFormat);
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
                Log.Error(ex, "Exception in Activity Logging");

                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError);
            }

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Returns additional details (key-value) data for particular activity.", Type = typeof(List<Shared.Models.AdditionalParameters>))]
        [Route("api/additionalparams")]
        [HttpGet]
        public HttpResponseMessage ListActivityAdditionalParams(long activityId)
        {
            try
            {
                if (activityId == 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "0 activity Id received.");

                var readerRepository = new ReaderRepository();
                var result = readerRepository.ListActivityAdditionalParams(activityId);

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
                Log.Information($"Deleting Activity Log for company {organizationPartyId}");

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
        public HttpResponseMessage ListActivityLogDetails(ActivityLogFilterCriteria filterCriteria)
        {
            var result = new ListResponse<ActivityDetailMessage>();
            try
            {
                Log.Information($"Getting Activity Log Detail");
                var isArchived = Request.GetQueryNameValuePairs().Any(x=>x.Key == "isArchived" && x.Value == "true");
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
                result = readerRepository.ListActivityLogDetails(filterCriteria, isArchived);

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
        private byte[] ExportActivityData(IList<ActivityDetailMessage> listActivityDetailMessage, SaveFormat dataFormat = SaveFormat.CSV)
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

            // Get the pagesetup object
            PageSetup pageSetup = worksheet.PageSetup;

            // Set bottom,left,right and top page margins
            pageSetup.BottomMarginInch = 0.5;
            pageSetup.LeftMarginInch = 0.25;
            pageSetup.RightMarginInch = 0.25;
            pageSetup.TopMarginInch = 0.5;

            worksheet.Cells.ImportCustomObjects(
                (System.Collections.ICollection) listActivityDetailMessage,
                propertyNames,
                false, //Don't show the field names
                1, //Start at second row
                0,
                listActivityDetailMessage.Count,
                true,
                "",
                false
            );

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

            //FileStream file = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Export." + dataFormat.ToString()), FileMode.Create, FileAccess.Write);
            //memorystream.WriteTo(file);
            //file.Close();

            //Get bytes
            byte[] bytes = memorystream.ToArray();

            memorystream.Dispose();
            memorystream.Close();

            return bytes;
        }

        #endregion
    }
}