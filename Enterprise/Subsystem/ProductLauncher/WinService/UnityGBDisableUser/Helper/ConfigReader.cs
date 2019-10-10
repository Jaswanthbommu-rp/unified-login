using System.Configuration;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityGBDisableUser.Helper
{
	public static class ConfigReader
	{
		#region Common Configurations
		public static string GetThreadCount => ConfigurationManager.AppSettings.Get("ThreadCount");
		public static string GetCallDuration => ConfigurationManager.AppSettings.Get("callIntervel");
		public static string GetScheduledTime => ConfigurationManager.AppSettings.Get("ScheduledTime");
		public static string GetLandingApiUri => ConfigurationManager.AppSettings.Get("LandingAPIUri");

		#endregion
	}
}
