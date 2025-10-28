namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for Configuration Setting
	/// </summary>
	public interface IConfigurationSetting
	{
		/// <summary>
		/// Master Configuration Setting unique Id
		/// </summary>
		long MasterConfigurationSettingId { get; set; }

		/// <summary>
		/// Setting Name
		/// </summary>
		string SettingName { get; set; }

		/// <summary>
		/// Setting Name
		/// </summary>
		string Value { get; set; }
	}
}