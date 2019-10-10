using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Text.RegularExpressions;


namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	public class UserDeviceDetail
	{
		public static UserDeviceDetails ParseUserDeviceDetails(HttpRequest request)
		{
			var browser = request.Browser;
			var browserName = string.Empty;
			var browserVersion = string.Empty;
			var browserType = string.Empty;
			var platform = string.Empty;

			if (request.UserAgent.ToLower().IndexOf("edge") > -1)
			{
				browserVersion = Regex.Match(request.UserAgent + " ", @"Edge/([^\s]*)\s").Groups[1].ToString();
				browserName = "Edge";				
				browserType = "Edge" + Math.Floor(double.Parse(browserVersion));
			}
			else
			{
				browserName = browser.Browser;
				browserVersion = browser.Version;
				browserType = browser.Type;
			}

			platform = browser.Platform;

			if (platform == "Unknown")
			{
				if (request.UserAgent.IndexOf("Android") > -1)
				{
					platform = "Android";
				}
				else if (request.UserAgent.IndexOf("Mac OS X") > -1)
				{
					platform = "Mac OS X";
				}
				else if (request.UserAgent.IndexOf("Windows") > -1)
				{
					platform = "Windows";
				}
			}

			return ParseUserDeviceDetails(
					request.UserHostAddress,
					browserType,
					browserName,
					browserVersion,
					platform,
					browser.IsMobileDevice,
					getDeviceType(request.UserAgent)
				);
		}

		public static UserDeviceDetails ParseUserDeviceDetails(string ipAddress, string browserType, string browserName, string version, string platform, bool isMobile, DeviceType deviceType)
		{
			UserDeviceDetails deviceDetails = null;
			try
			{
				deviceDetails = new UserDeviceDetails
				{
					IpAddress = ipAddress,
					BrowserType = browserType,
					BrowserName = browserName,
					Version = version,
					Platform = platform,
					IsMobile = isMobile,
					DeviceType = deviceType.ToString()
				};
			}
			catch (Exception ex)
			{
				// add log            
			}

			return deviceDetails;
		}

		static DeviceType getDeviceType(string userAgent)
		{
			if (Regex.IsMatch(userAgent, "Windows Phone") || Regex.IsMatch(userAgent, "WPDesktop"))
			{
				return DeviceType.WindowsPhone;
			}
			else if (Regex.IsMatch(userAgent, "iPhone"))
			{
				return DeviceType.Iphone;
			}
			else if (Regex.IsMatch(userAgent, "iPad"))
			{
				return DeviceType.Ipad;
			}
			else if (Regex.IsMatch(userAgent, "Android"))
			{
				if (Regex.IsMatch(userAgent, "Mobile"))
				{
					return DeviceType.AndroidPhone;
				}
				else
				{
					return DeviceType.AndroidTablet;
				}
			}
			else
			{
				return DeviceType.DesktopOrLaptop;
			}
		}
	}
}
