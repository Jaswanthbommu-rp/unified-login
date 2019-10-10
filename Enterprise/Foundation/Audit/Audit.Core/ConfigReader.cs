using System;
using System.Configuration;

namespace RP.Enterprise.Foundation.Audit.Core.Component
{
    /// <summary>
    /// Helper class to get configuration from web.config
    /// </summary>
    internal static class ConfigReader
    {
        public static string LogPath { get; } = ConfigurationManager.AppSettings.Get("LogPath");
        public static string ElasticSearchUri { get; } = ConfigurationManager.AppSettings["ElasticSearchUri"];
        public static string LogProductName { get; } = ConfigurationManager.AppSettings["LogProductName"];
        public static string ElasticSearchIndexTypeName { get; } = ConfigurationManager.AppSettings["ElasticSearchIndexTypeName"];
        public static string Environment { get; } = ConfigurationManager.AppSettings["Environment"];
        public static bool ShouldWriteInFile { get; } = Convert.ToBoolean(ConfigurationManager.AppSettings["ShouldWriteInFile"]);
        public static bool ShouldLogPerformance { get; } = Convert.ToBoolean(ConfigurationManager.AppSettings["ShouldLogPerformance"]);
        public static bool ShouldLogDiagnostic { get; } = Convert.ToBoolean(ConfigurationManager.AppSettings["ShouldLogDiagnostic"]);
        public static string ActivityMQName { get; } = ConfigurationManager.AppSettings["ActivityMQName"];
    }
}
