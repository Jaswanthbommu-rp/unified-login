using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// 
	/// </summary>
	public class LogOutIntervalResponse : ResponseBase
	{
		/// <summary>
		/// 
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public SeverityLevelType SeverityLevel { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int DaysToExpire { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int LogOutSetInterval { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Remaining { get; set; }
	}
}
