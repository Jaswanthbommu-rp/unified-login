using System;
using Microsoft.AspNetCore.Http;

namespace UnifiedLogin.SharedObjects.Helper
{
	public class ClientTimezone
	{
		private static double GetUTCOffsetHour(HttpRequest request)
		{
			var cookies = request.Cookies;
			double offset;

			if (cookies["timezone"] == null)
			{
				offset = TimeZoneInfo.Local.BaseUtcOffset.Hours;
			}
			else
			{
				offset = double.Parse(cookies["timezone"]);
			}

			return offset;
		}

		/// <summary>
		/// Get the client timezone display name based on the http request's cookie
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public static string GetTimezoneDisplayName(HttpRequest request)
		{
			var offset = GetUTCOffsetHour(request);
			var sign = offset < 0 ? "-" : "+";
			var timeValue = TimeSpan.FromHours(Math.Abs(offset)).ToString("hh\\:mm");

			foreach (TimeZoneInfo timezoneInfo in TimeZoneInfo.GetSystemTimeZones())
			{
				if (timezoneInfo.DisplayName.Contains(sign + timeValue))
					//return string.Format("UTC{0}{1}", sign, timeValue);
					return timezoneInfo.DisplayName;
			}

			return null;
		}

		/// <summary>
		/// Get the client timezone ID based on the http request's cookie
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public static string GetTimeZoneID(HttpRequest request)
		{
			var offset = GetUTCOffsetHour(request);
			var sign = offset < 0 ? "-" : "+";
			var timeValue = TimeSpan.FromHours(Math.Abs(offset)).ToString("hh\\:mm");

			foreach (TimeZoneInfo timezoneInfo in TimeZoneInfo.GetSystemTimeZones())
			{
				if (timezoneInfo.DisplayName.Contains(sign + timeValue))
					return timezoneInfo.Id;
			}

			return null;
		}

		/// <summary>
		/// Get the client local datetime based on the http request's cookie
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public static DateTime GetClientDatetime(HttpRequest request)
		{
			var clientTimeZoneId = GetTimeZoneID(request);
			return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, clientTimeZoneId);
			//var offset = GetUTCOffsetHour(request);
			//return DateTime.UtcNow.AddHours(offset);
		}


		/// <summary>
		/// Get the client local datetime based on the http request's cookie
		/// </summary>
		/// <param name="request"></param>
		/// <param name="dateTimeUTC"></param>
		/// <returns></returns>
		public static DateTime ConvertToClientDatetime(HttpRequest request, DateTime dateTimeUTC)
		{
			var clientTimeZoneId = GetTimeZoneID(request);
			return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTimeUTC, TimeZoneInfo.Local.Id, clientTimeZoneId);
			//var offset = GetUTCOffsetHour(request);
			//return dateTimeUTC.AddHours(offset);
		}

		/// <summary>
		/// Get the local utc datetime based on the user time zone
		/// </summary>
		/// <param name="timezone"></param>
		/// <returns></returns>
		public static DateTime ConvertUTCWithUserTimeZone(string timezone)
		{
			DateTime dateTimeUtc = DateTime.UtcNow;
			TimeZoneInfo userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
			DateTime convertedDate = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, userTimeZone);
			return convertedDate;
		}

		/// <summary>
		/// Get the utc datetime based on the user time zone
		/// </summary>
		/// <param name="date"></param>
		/// <param name="timezone"></param>
		/// <returns></returns>
		public static DateTime GetUTCDateWithUserTimeZoneOffset(DateTime date, string timezone)
		{
			TimeZoneInfo userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
			DateTime convertedDate = TimeZoneInfo.ConvertTimeFromUtc(date, userTimeZone);
			return convertedDate;
		}

		/// <summary>
		/// convert and Get the utc datetime based on the user time zone
		/// </summary>
		/// <param name="date"></param>
		/// <param name="timezone"></param>
		/// <returns></returns>
		public static DateTime GetUTCDateFromUserFromDateAndTimeZoneOffset(DateTime date, string timezone)
		{
			TimeZoneInfo userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
			if (date.Kind != DateTimeKind.Utc)
			{
				DateTime convertedDate = TimeZoneInfo.ConvertTimeToUtc(date, userTimeZone);
				return convertedDate;
			}
			return date;
		}
	}
}
