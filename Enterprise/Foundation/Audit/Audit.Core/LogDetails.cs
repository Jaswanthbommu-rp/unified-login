using System;
using System.Collections.Generic;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;

namespace RP.Enterprise.Foundation.Audit.Core.Component
{
    public abstract class BaseLogDetails
    {
        public BaseLogDetails()
        {
            Timestamp = DateTime.Now.ToUniversalTime();
            LocalTime = DateTime.Now;
            LocalTimezone = TimeZone.CurrentTimeZone.StandardName;
            ProductName = ConfigReader.LogProductName;
            ServerName = System.Environment.MachineName;
        }

        public BaseLogDetails(string serverName) : this()
        {
            ServerName = serverName;
        }

        public string LocalTimezone { get; }
        public DateTime LocalTime { get; }
        public LogLevel LogLevel { get; set; }
        public string ProductName { get; set; }
        public string ProductLocation { get; set; }
        public string ProductModule { get; set; }
        public string ProductWorkflow { get; set; }
        public string ProductStep { get; set; }
        public string ServerName { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; private set; }
        public string CorrelationId { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public string PmcId { get; set; }
        public string PmcName { get; set; }
        public string SiteId { get; set; }
        public string SiteName { get; set; }
        public string Environment { get; set; } // local, dev, sat etc
        public Dictionary<string, object> AdditionalInfo { get; set; }
    }
    public class LogDetails : BaseLogDetails
    {
        public Exception Exception { get; set; }
    }

    internal class PerformanceLogDetails : BaseLogDetails
    {

    }

    internal class UtilizationLogDetails : BaseLogDetails
    {

    }

    internal class InformationLogDetails : BaseLogDetails
    {

    }
}
