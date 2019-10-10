using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
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
