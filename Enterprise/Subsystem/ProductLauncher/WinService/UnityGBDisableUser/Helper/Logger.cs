using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityGBDisableUser.Helper
{
	public static class Logger
	{
		public static void ConsoleOut(string message)
		{
#if DEBUG
			Console.WriteLine(message);
#endif
		}
	}
}
