namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for Identity Provider Setting Type
	/// </summary>
	public interface IIdentityProviderSettingType
	{
		/// <summary>
		/// Identity Provider Setting Type unique Id
		/// </summary>
		int IdentityProviderSettingTypeId { get; set; }

		/// <summary>
		/// Identity Provider type unique Id
		/// </summary>
		int IdentityProviderTypeId { get; set; }

		/// <summary>
		/// Identity Provider Setting Type Name
		/// </summary>
		string Name { get; set; }
	}
}