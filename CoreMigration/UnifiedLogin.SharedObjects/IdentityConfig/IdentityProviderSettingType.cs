using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Identity Provider Setting Type
	/// </summary>
	public class IdentityProviderSettingType : IIdentityProviderSettingType
	{
		/// <summary>
		/// Identity Provider Setting Type unique Id
		/// </summary>
		[JsonProperty(PropertyName = "IdentityProviderSettingTypeId")]
		public int IdentityProviderSettingTypeId { get; set; }

		/// <summary>
		/// Identity Provider type unique Id
		/// </summary>
		[JsonProperty(PropertyName = "IdentityProviderTypeId")]
		public int IdentityProviderTypeId { get; set; }

		/// <summary>
		/// Identity Provider Setting Type Name
		/// </summary>
		[JsonProperty(PropertyName = "Name")]
		public string Name { get; set; }

		#region Examples
		/// <summary>
		/// Example for New IdentityProviderSettingType method
		/// </summary>
		/// <returns>Newly Created Identity Provider Setting Type Id</returns>
		public static IdentityProviderSettingTypeOutputResult GetNewIdentityProviderSettingTypeExample()
		{
			IdentityProviderSettingTypeOutputResult result = new IdentityProviderSettingTypeOutputResult();
			result.IdentityProviderSettingTypeId = 1;
			return result;
		}

		/// <summary>
		/// Output result Identity Provider Setting Type Id
		/// </summary>
		public class IdentityProviderSettingTypeOutputResult
		{
			/// <summary>
			/// Represents the newly created Identity Provider Setting Type Id
			/// </summary>
			public int IdentityProviderSettingTypeId { get; set; }
		}
		#endregion
	}
}
