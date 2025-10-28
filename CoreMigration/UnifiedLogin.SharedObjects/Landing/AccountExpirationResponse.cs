using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class AccountExpirationResponse : ResponseBase
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public SeverityLevelType SeverityLevel { get; set; }
        public int  DaysToExpire { get; set; }
		public int LogOutSetInterval { get; set; }
		public string Remaining{ get; set; }
	}
}
