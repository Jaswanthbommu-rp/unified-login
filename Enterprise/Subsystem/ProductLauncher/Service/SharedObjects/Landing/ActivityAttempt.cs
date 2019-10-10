using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
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