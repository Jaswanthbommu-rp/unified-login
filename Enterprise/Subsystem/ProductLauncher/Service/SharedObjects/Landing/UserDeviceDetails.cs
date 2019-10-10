using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	public class AuthUserDetails
	{
		public string EnterpriseUserName { get; set; }
		public string Password { get; set; }
		public UserDeviceDetails UserDeviceDetails { get; set; }
	}

	public class UserDeviceDetails
	{
		public string IpAddress { get; set; }
		public string BrowserType { get; set; }
		public string BrowserName { get; set; }
		public string Version { get; set; }
		public string Platform { get; set; }
		public bool IsMobile { get; set; }
		public string DeviceType { get; set; }
		public string Timezone { get; set; }

		public static UserDeviceDetails ParseUserDeviceDetails(HttpRequest request)
		{
			if (string.IsNullOrEmpty(request.UserAgent))
				return null;

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
				else if (request.UserAgent.IndexOf("Mac OS") > -1 && (request.UserAgent.IndexOf("iPhone") > -1 || request.UserAgent.IndexOf("iPad") > -1))
				{
					platform = "iOS";
				}
				else if (request.UserAgent.IndexOf("Mac OS") > -1)
				{
					platform = "Mac OS";
				}
				else if (request.UserAgent.IndexOf("Windows") > -1)
				{
					platform = "Windows";
				}
			}

			UserDeviceDetails deviceDetails = null;

			try
			{
				deviceDetails = new UserDeviceDetails
				{
					BrowserType = browserType,
					BrowserName = browserName,
					Version = browserVersion,
					Platform = platform,
					IsMobile = browser.IsMobileDevice,
					DeviceType = GetDeviceType(request.UserAgent).ToString(),
					Timezone = ClientTimezone.GetTimezoneDisplayName(request)
				};

				if (HttpContext.Current.Request.Headers["X-Forwarded-For"] != null)
				{
					deviceDetails.IpAddress = HttpContext.Current.Request.Headers["X-Forwarded-For"].Split(new char[] { ',' })
						.FirstOrDefault();
				}
				else
				{
					deviceDetails.IpAddress = "non-xf" + request.UserHostAddress; // to check if X-Forwarded-For not working
				}
			}
			catch (Exception ex)
			{
				// add log   
				Log.Write(Foundation.Audit.Core.Component.Enums.LogType.Error, new LogDetails
				{
					Message = ex.Message,
					ProductModule = "UserDeviceDetails.ParseUserDeviceDetails",
					Exception = ex,
				});
			}

			return deviceDetails;
		}

		static DeviceType GetDeviceType(string userAgent)
		{
			if (userAgent.IndexOf("Windows Phone", StringComparison.OrdinalIgnoreCase) > -1 || userAgent.IndexOf("WPDesktop", StringComparison.OrdinalIgnoreCase) > -1)
			{
				return Enum.DeviceType.WindowsPhone;
			}
			else if (userAgent.IndexOf("iPhone", StringComparison.OrdinalIgnoreCase) > -1)
			{
				return Enum.DeviceType.Iphone;
			}
			else if (userAgent.IndexOf("iPad", StringComparison.OrdinalIgnoreCase) > -1)
			{
				return Enum.DeviceType.Ipad;
			}
			else if (userAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) > -1)
			{
				if (userAgent.IndexOf("Mobile", StringComparison.OrdinalIgnoreCase) > -1)
				{
					return Enum.DeviceType.AndroidPhone;
				}
				else
				{
					return Enum.DeviceType.AndroidTablet;
				}
			}
			else
			{
				return Enum.DeviceType.DesktopOrLaptop;
			}
		}

	}
}