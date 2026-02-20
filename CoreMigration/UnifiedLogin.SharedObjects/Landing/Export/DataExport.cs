using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Landing.Export
{
    public class DataExport
    {
        #region private methods
        /// <summary>
        /// Set Aspose License
        /// </summary>
        /// <returns>Error Status object</returns>
        public static Status<IErrorData> SetAsposeLicense()
        {
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            try
            {
                Aspose.Cells.License asposeCellsLicense = new Aspose.Cells.License();
                //Gets the base directory that the assembly resolver uses to probe for assemblies + Aspose license file location
                asposeCellsLicense.SetLicense(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "license", "Aspose.Total.NET.lic"));
               // asposeCellsLicense.SetLicense(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ThirdParty\Aspose.Total.Lic"));
            }
            catch (Exception ex)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "SetAsposeLicense.1";
                errorStatus.ErrorMsg = "Set Aspose License: " + ex.Message;
            }

            return errorStatus;
        }

        /// <summary>
        /// Create Excel WorkSheet
        /// </summary>
        /// <param name="workbook">Aspose Cells Workbook</param>
        /// <param name="worksheet">Aspose Cells WorkSheet</param>
        public static void CreateExcelWorkSheet(out Workbook workbook, out Worksheet worksheet)
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
        /// <param name="exportConfigurations">exportConfigurations</param>
        /// <param name="DataList">List of Records to export</param>
        /// <param name="dataFormat">Retrun data in this format (default = CSV)</param>
        /// <returns>Array of bytes</returns>
        public static byte[] ExportDataToFile<T>(List<ExportDataFileConfiguration> exportConfigurations, List<T> DataList, SaveFormat dataFormat = SaveFormat.CSV)
        {
            byte[] bytes;
            Workbook workbook;
            Worksheet worksheet;
            MemoryStream memorystream = new MemoryStream();

            IList<string> propertyNamesList = exportConfigurations.Select(p => p.MappedField).ToList<string>();

            CreateExcelWorkSheet(out workbook, out worksheet);

            //Manually add the row titles
            int col = 0;
            Cells cells = worksheet.Cells;

            Cell cell = cells[0, col++];
            //cell.PutValue("Company");
            cell.PutValue(exportConfigurations[0].Header);

            for (int header = 1; header < exportConfigurations.Count; header++)
            {
                worksheet.Cells[0, col++].PutValue(exportConfigurations[header].Header);
            }
            int totalColumns = col;

            // Get the pagesetup object
            PageSetup pageSetup = worksheet.PageSetup;

            // Set bottom,left,right and top page margins
            pageSetup.BottomMarginInch = 0.5;
            pageSetup.LeftMarginInch = 0.25;
            pageSetup.RightMarginInch = 0.25;
            pageSetup.TopMarginInch = 0.5;

            string[] propertyNames = propertyNamesList.ToArray();
            worksheet.Cells.ImportCustomObjects(
                (System.Collections.ICollection)DataList,
                propertyNames,
                false, //Don't show the field names
                1, //Start at second row
                0,
                DataList.Count,
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
                    foreach (var result in exportConfigurations.OrderBy(p => p.Preference))
                    {
                        if(result.PDFColumnWidth != "")
                        {
                            worksheet.Cells.SetColumnWidthInch(col++, Convert.ToDouble(result.PDFColumnWidth));
                        }
                    }

                    //Create a StyleFlag object.
                    StyleFlag styleFlag = new StyleFlag
                    {
                        //Make the corresponding attributes ON.
                        Font = true,
                        VerticalAlignment = true,
                        CellShading = true
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
                    cell = worksheet.Cells.LastCell;
                    range = worksheet.Cells.CreateRange(1, 0, cell.Row, totalColumns);
                    style.IsTextWrapped = true;
                    style.VerticalAlignment = TextAlignmentType.Top;
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
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.Save(memorystream, dataFormat);

                //Get bytes
                bytes = memorystream.ToArray();
            }

            return bytes;
        }
        #endregion
    }
}
