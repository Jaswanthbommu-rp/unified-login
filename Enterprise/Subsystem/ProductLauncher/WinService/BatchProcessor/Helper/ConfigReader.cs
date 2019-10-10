using System.Configuration;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Helper
{
    public static class ConfigReader
    {
        #region Common Configurations

        public static string GetThreadCount => ConfigurationManager.AppSettings.Get("ThreadCount");
        public static string GetPollingInterval => ConfigurationManager.AppSettings.Get("PollingInterval");
        public static string GetRetryPollingInterval => ConfigurationManager.AppSettings.Get("RetryPollingInterval");
        public static string GetBatchSize => ConfigurationManager.AppSettings.Get("BatchSize");
        public static string GetExceptionWaitInterval => ConfigurationManager.AppSettings.Get("ExceptionWaitInterval");

        #endregion
    }
}