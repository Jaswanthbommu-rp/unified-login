using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Identity.Logging
{
    /// <summary>
    /// This class copied from Audit.MvcWeb
    /// </summary>
    public static class AuditLogHelper
    {
        public static void LogWebError(Exception ex)
        {
            string userId, userName, location, pmcId, pmcName;
            var webInfo = GetWebLoggingData(out userId, out userName, out location, out pmcId, out pmcName);

            //Log.ForContext("AdditionalInfo", webInfo).Write( LogEventLevel.Error, ex, ex.Message);
            var logger = Log.Logger;
			if (webInfo?.Keys != null)
			{
				logger = logger.ForContext($"AdditionalInfo", webInfo, true);
			}
            logger.Write(LogEventLevel.Error, ex, ex.Message );
        }

        public static void GetHttpStatus(Exception ex, out int httpStatus)
        {
            httpStatus = 500;  // default is server error
            if (ex is HttpException)
            {
                var httpEx = ex as HttpException;
                httpStatus = httpEx.GetHttpCode();
            }
        }

        public static Dictionary<string, object> GetWebLoggingData(out string userId, out string userName, out string location, out string pmcId, out string pmcName)
        {
            var data = new Dictionary<string, object>();

            GetSessionData(data);
            GetRequestData(data, out location);
            GetUserData(data, out userId, out userName, out pmcId, out pmcName);

            return data;
        }

        private static void GetUserData(Dictionary<string, object> data, out string userId, out string userName, out string pmcId, out string pmcName)
        {
            userId = string.Empty;
            userName = string.Empty;
            pmcId = string.Empty;
            pmcName = string.Empty;

            var user = ClaimsPrincipal.Current;
            if (user != null)
            {
                var i = 1;
                foreach (var claim in user.Claims)
                {
                    if (claim.Type == "loginName")
                    {
                        userName = claim.Value;
                    }
                    else if (claim.Type == "orgPartyId")
                    {
                        pmcId = claim.Value;
                    }
                    else if (claim.Type == "orgName")
                    {
                        pmcName = claim.Value;
                    }
                    else if (claim.Type == "sub")
                    {
                        userId = claim.Value;
                    }
                    else
                    {
                        data.Add($"UserClaim-{i++}-{claim.Type}", claim.Value);
                    }
                }
            }
        }

        private static void GetRequestData(Dictionary<string, object> data, out string location)
        {
            location = "";
            var request = HttpContext.Current.Request;
            if (request != null)
            {
                location = request.Path;

                data.Add("Browser", request.Browser.Type + " (v " + request.Browser.MajorVersion + "." + request.Browser.MinorVersion + ")");
                data.Add("UserHostAddress", request.UserHostAddress);
                data.Add("UserAgent", request.UserAgent);
                data.Add("Languages", request.UserLanguages);
                foreach (var qsKey in request.QueryString.Keys)
                {
                    if (qsKey != null)
                    {
                        data.Add($"QueryString-{qsKey}", request.QueryString[qsKey.ToString()]);
                    }
                }
            }
        }

        private static void GetSessionData(Dictionary<string, object> data)
        {
            if (HttpContext.Current.Session != null)
            {
                foreach (var key in HttpContext.Current.Session.Keys)
                {
                    if (HttpContext.Current.Session[key.ToString()] != null)
                    {
                        data.Add($"Session-{key.ToString()}", HttpContext.Current.Session[key.ToString()].ToString());
                    }
                }

                data.Add("SessionId", HttpContext.Current.Session.SessionID);
            }
        }
    }
}