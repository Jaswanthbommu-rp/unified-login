using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class ActivityAttempt
    {
		public string EnterpriseUserName { get; set; }
		public ActivityType ActivityType { get; set; }
        public UserDeviceDetails UserDeviceDetails { get; set; }
		public string UserAgent { get; set; }
		//public string IPAddress { get; set; }
		//public string Timezone { get; set; }
		public string AuthenticationServiceId { get; set; }
	}
}