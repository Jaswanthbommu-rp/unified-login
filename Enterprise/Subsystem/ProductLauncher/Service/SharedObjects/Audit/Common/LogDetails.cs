using System;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common
{
    /// <summary>
    ///     Refactored class comes from Audit.Core
    /// </summary>
    public class LogDetails
    {
        public string LocalTimezone { get; }
        public DateTime LocalTime { get; }
        //public LogLevel LogLevel { get; set; }
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
        public Exception Exception { get; set; }
    }
}
