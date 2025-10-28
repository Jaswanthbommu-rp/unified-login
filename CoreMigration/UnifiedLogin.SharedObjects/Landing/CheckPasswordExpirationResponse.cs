using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Password Expiration Response Object
    /// </summary>
    public class CheckPasswordExpirationResponse : ResponseBase, ICheckPasswordExpirationResponse
	{
        /// <summary>
        /// Severity Level for message
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public SeverityLevelType SeverityLevel { get; set; }

        /// <summary>
        /// Days to expire password
        /// </summary>
        public int DaysToExpire { get; set; }

		/// <summary>
		/// IsPasswordExpired (true if daysToExpire less or equal to 0; otherwise false)  
		/// </summary>
		[JsonProperty(PropertyName = "IsPasswordExpired")]
		public bool IsPasswordExpired { get; set; } = false;
	}
}
