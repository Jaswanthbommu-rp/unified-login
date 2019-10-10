using System.Configuration;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification.Helper
{
    public static class ConfigReader
    {
        #region Common Configurations
        public static string GetThreadCount => ConfigurationManager.AppSettings.Get("ThreadCount");       
        public static string GetCallDuration => ConfigurationManager.AppSettings.Get("callInterval");
        public static string GetBatchSize => ConfigurationManager.AppSettings.Get("batchSize");
        public static string GetLandingApiUri => ConfigurationManager.AppSettings.Get("LandingAPIUri");
        public static string GetScheduledTime => ConfigurationManager.AppSettings.Get("ScheduledTime");
        #endregion
    }
}
