using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityGBDisableUser.Helper;
using System;
using System.Net;
using System.ServiceProcess;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityGBDisableUser
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var disableUserService = new DisableUserService();

			if (Environment.UserInteractive)
			{
				Log.Write(LogType.Information, new LogDetails { Message = "DisableUserServiceMC Running in console mode" });

				disableUserService.Start();
				Logger.ConsoleOut("Press any key to stop program");
				Console.Read();
				disableUserService.Stop();
			}
			else
			{
				Log.Write(LogType.Information, new LogDetails { Message = "DisableUserServiceMC Running in windows Service mode" });
				ServiceBase.Run(disableUserService);
			}
		}
	}
}
