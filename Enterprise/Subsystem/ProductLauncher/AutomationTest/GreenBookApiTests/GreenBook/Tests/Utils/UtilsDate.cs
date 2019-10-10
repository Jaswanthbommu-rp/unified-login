using System;
using System.Globalization;
using System.Linq;

namespace GreenBook.Utils
{
    public static class UtilsDate
    {
        static readonly CultureInfo  EnUs = new CultureInfo("en-US");
        static string _result = "Fail";
        static DateTime _oDate;
        static readonly string[] AllowedShortDateFormat = new[] { "MM/dd/yyyy", "MM/d/yyyy", "M/dd/yyyy", "M/d/yyyy" };
        static readonly string[] AllowedLongDateFormat = new string[] { "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss.ff", "yyyy-MM-dd HH:mm:ss.f" };

        /// <summary>
        /// TestShortDateFormat : Validates the given string date is of the short date format "MM/dd/yyyy"
        /// </summary>
        /// <param name="sDate"></param>
        /// <returns>boolean of type string</returns>
        public static string TestShortDateFormat(string sDate)
        {
            if(AllowedShortDateFormat.Any(shortDateFormat =>DateTime.TryParseExact(sDate,shortDateFormat,EnUs,DateTimeStyles.None, out _oDate)))
            { _result = "Pass"; }

            return _result;
        }

        /// <summary>
        /// TestLongDateFormat : Validates the given string date is of the long date format "yyyy-MM-dd HH:mm:ss.fff"
        /// </summary>
        /// <param name="lDate"></param>
        /// <returns>boolean of type string</returns>
        public static string TestLongDateFormat(string lDate)
        {
            if( AllowedLongDateFormat.Any(longDateFormat => DateTime.TryParseExact(lDate, longDateFormat, EnUs, DateTimeStyles.None, out _oDate)))
            {_result = "Pass";}

            return _result;
        }

        /// <summary>
        /// StringToShortDateFormat : Convert the given Date from string to short date format
        /// </summary>
        /// <param name="sDate"></param>
        /// <returns>"DateTime Object"</returns>
        public static DateTime StringToShortDateFormat(string sDate)
        {
            sDate = sDate.TrimStart();
            sDate = sDate.TrimEnd();
        
            if (AllowedShortDateFormat.Any(shortDateFormat => DateTime.TryParseExact(sDate, shortDateFormat, EnUs, DateTimeStyles.None, out _oDate)))
            {return _oDate;}
            
            return _oDate;
        }


        /// <summary>
        /// StringToLongDateFormat : Convert the given Date from string to long date format
        /// </summary>
        /// <param name="lDate"></param>
        /// <returns>"DateTime Object"</returns>
        public static DateTime StringToLongDateFormat(string lDate)
        {
            lDate = lDate.TrimStart();
            lDate = lDate.TrimEnd();
            
            if (AllowedLongDateFormat.Any(longDateFormat =>DateTime.TryParseExact(lDate, longDateFormat, EnUs, DateTimeStyles.None, out _oDate)))
            { return _oDate; }
              
            return _oDate;
        }
    }
}
