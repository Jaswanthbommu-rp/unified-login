using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Identity Provider Setting
	/// </summary>
	public class IdentityProviderSetting : IIdentityProviderSetting
	{
		/// <summary>
		/// Identity Provider Setting unique Id
		/// </summary>
		[JsonProperty(PropertyName = "IdentityProviderSettingId")]
		public int IdentityProviderSettingId { get; set; }

		/// <summary>
		/// Identity Provider Setting Type unique Id
		/// </summary>
		[JsonProperty(PropertyName = "IdentityProviderSettingTypeId")]
		public int IdentityProviderSettingTypeId { get; set; }

		/// <summary>
		/// Identity Provider Setting Type Name
		/// </summary>
		[JsonProperty(PropertyName = "Value")]
		public string Value { get; set; }

		#region Examples
		/// <summary>
		/// Example for New IdentityProviderSetting method
		/// </summary>
		/// <returns>Newly Created Identity Provider Setting Id</returns>
		public static IdentityProviderSettingOutputResult GetNewIdentityProviderSettingExample()
		{
			IdentityProviderSettingOutputResult result = new IdentityProviderSettingOutputResult();
			result.IdentityProviderSettingId = 1;
			return result;
		}

		/// <summary>
		/// Output result Identity Provider Setting Id
		/// </summary>
		public class IdentityProviderSettingOutputResult
		{
			/// <summary>
			/// Represents the newly created Identity Provider Setting Id
			/// </summary>
			public int IdentityProviderSettingId { get; set; }
		}
		#endregion
	}
}
