using UnifiedLogin.SharedObjects.Enum;
using System;
using System.Linq;
using System.Text.RegularExpressions;
// using System.Web; // removed for .NET 8
using UnifiedLogin.SharedObjects.Helper;
using Serilog;
using Microsoft.AspNetCore.Http; // for cast only

namespace UnifiedLogin.SharedObjects.Landing
{
	// Minimal placeholders for HttpContext and related header storage
	public class HttpContext
	{
		public static HttpContext Current { get; } = new HttpContext();
		public HttpRequest Request { get; set; } = new HttpRequest();
	}
	public class HttpRequest
	{
		public HttpBrowserCapabilities Browser { get; set; } = new HttpBrowserCapabilities();
		public string UserAgent { get; set; } = string.Empty;
		public string UserHostAddress { get; set; } = string.Empty;
		public System.Collections.Generic.Dictionary<string, string> Headers { get; set; } = new System.Collections.Generic.Dictionary<string, string>();
	}
	public class HttpBrowserCapabilities
	{
		public string Browser { get; set; } = string.Empty;
		public string Version { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;
		public string Platform { get; set; } = "Unknown";
		public bool IsMobileDevice { get; set; } = false;
	}
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
					Timezone = ClientTimezone.GetTimezoneDisplayName((Microsoft.AspNetCore.Http.HttpRequest)(object)request)
				};

				if (HttpContext.Current.Request.Headers.ContainsKey("X-Forwarded-For") && HttpContext.Current.Request.Headers["X-Forwarded-For"] != null)
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
				Log.Error(ex, ex.Message, "UserDeviceDetails.ParseUserDeviceDetails");
			}

			return deviceDetails;
		}

		static UnifiedLogin.SharedObjects.Enum.DeviceType GetDeviceType(string userAgent)
		{
			if (userAgent.IndexOf("Windows Phone", StringComparison.OrdinalIgnoreCase) > -1 || userAgent.IndexOf("WPDesktop", StringComparison.OrdinalIgnoreCase) > -1)
			{
				return UnifiedLogin.SharedObjects.Enum.DeviceType.WindowsPhone;
			}
			else if (userAgent.IndexOf("iPhone", StringComparison.OrdinalIgnoreCase) > -1)
			{
				return UnifiedLogin.SharedObjects.Enum.DeviceType.Iphone;
			}
			else if (userAgent.IndexOf("iPad", StringComparison.OrdinalIgnoreCase) > -1)
			{
				return UnifiedLogin.SharedObjects.Enum.DeviceType.Ipad;
			}
			else if (userAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) > -1)
			{
				if (userAgent.IndexOf("Mobile", StringComparison.OrdinalIgnoreCase) > -1)
				{
					return UnifiedLogin.SharedObjects.Enum.DeviceType.AndroidPhone;
				}
				else
				{
					return UnifiedLogin.SharedObjects.Enum.DeviceType.AndroidTablet;
				}
			}
			else
			{
				return UnifiedLogin.SharedObjects.Enum.DeviceType.DesktopOrLaptop;
			}
		}

	}
}