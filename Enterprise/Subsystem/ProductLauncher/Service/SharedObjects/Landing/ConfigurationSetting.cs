using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	/// <summary>
	/// Configuration Setting
	/// </summary>
	public class ConfigurationSetting : IConfigurationSetting
	{
		/// <summary>
		/// Master Configuration Setting unique Id
		/// </summary>
		[JsonProperty(PropertyName = "MasterConfigurationSettingId")]
		public long MasterConfigurationSettingId { get; set; }
		
		/// <summary>
		/// Setting Name
		/// </summary>
		[JsonProperty(PropertyName = "SettingName")]
		public string SettingName { get; set; }

		/// <summary>
		/// Setting Name
		/// </summary>
		[JsonProperty(PropertyName = "Value")]
		public string Value { get; set; }
	}
}