namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for Identity Provider Setting
	/// </summary>
	public interface IIdentityProviderSetting
	{
		/// <summary>
		/// Identity Provider Setting unique Id
		/// </summary>
		int IdentityProviderSettingId { get; set; }

		/// <summary>
		/// Identity Provider Setting Type unique Id
		/// </summary>
		int IdentityProviderSettingTypeId { get; set; }

		/// <summary>
		/// Identity Provider Setting Type Name
		/// </summary>
		string Value { get; set; }
	}
}