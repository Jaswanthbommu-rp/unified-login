using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Landing
{
	public class UserTimeZone //: IUserTimeZone
    {
        /// <summary>
		/// TimeZone
		/// </summary>
		[JsonProperty(PropertyName = "timeZone")]
        public string TimeZone { get; set; }

        /// <summary>
		/// TimeZoneOffset
		/// </summary>
		[JsonProperty(PropertyName = "timeZoneOffset")]
        public string TimeZoneOffset { get; set; }
    }
}
