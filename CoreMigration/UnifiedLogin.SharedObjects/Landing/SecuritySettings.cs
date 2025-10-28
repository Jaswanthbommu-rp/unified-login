using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Security Settings
	/// </summary>
	public class SecuritySettings : ISecuritySettings
	{
		/// <summary>
		/// Password Security Settings
		/// </summary>
		[JsonProperty(PropertyName = "PasswordSettings")]
		public PasswordPolicyCommon PasswordSettings{ get; set; }

		/// <summary>
		/// Activity Configuration Security Settings
		/// </summary>
		[JsonProperty(PropertyName = "ConfigurationSettings")]
		public IList<Activity> ConfigurationSettings { get; set; }
	}
}
