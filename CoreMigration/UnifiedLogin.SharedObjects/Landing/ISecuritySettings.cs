using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for Security Settings
	/// </summary>
	public interface ISecuritySettings
	{
		/// <summary>
		/// Configuration Security Settings
		/// </summary>
		IList<Activity> ConfigurationSettings { get; set; }

		/// <summary>
		/// Password Security Settings
		/// </summary>
		PasswordPolicyCommon PasswordSettings { get; set; }
	}
}