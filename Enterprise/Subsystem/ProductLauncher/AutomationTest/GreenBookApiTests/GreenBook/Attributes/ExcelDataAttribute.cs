using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenBook.Attributes
{
    using Microsoft.Office.Interop.Excel;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.OleDb;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Xunit.Sdk;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ExcelDataAttribute : DataAttribute
    {
        public ExcelDataAttribute(string fileName)
        {
            FileName = AppDomain.CurrentDomain.BaseDirectory + "\\data\\" + fileName;
        }
        public string FileName { get; private set; }
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null)
                throw new ArgumentNullException("testMethod");

            //string dataSetPath = AppDomain.CurrentDomain.BaseDirectory + "\\data\\";
            int row = 2;
            var excelApp = new Application();
            var excelWorkbook = excelApp.Workbooks.Open(Filename: FileName, ReadOnly: 1);
            var excelSheets = excelWorkbook.Sheets;
            var excelWorksheet = (Worksheet)excelSheets.Item[1];
            object[] returndatat = new object[4];
            while ((string)((Range)excelWorksheet.Cells[row, 3]).Text != "")
            {
                returndatat[0] = ((string)((Range)excelWorksheet.Cells[row, 2]).Text);
                returndatat[1] = ((string)((Range)excelWorksheet.Cells[row, 3]).Text);
                returndatat[2] = ((string)((Range)excelWorksheet.Cells[row, 4]).Text);
                returndatat[3] = ((string)((Range)excelWorksheet.Cells[row, 5]).Text);
                row++;
                yield return new object[] { returndatat[0], returndatat[1], returndatat[2], returndatat[3] };
                //.SetName(returndatat[0].ToString() + returndatat[1].ToString()); //This is for naming the test case 
                //but using this renders the Test Explorer non usable 
            }
            excelWorkbook.Close();
        }

    }
}
