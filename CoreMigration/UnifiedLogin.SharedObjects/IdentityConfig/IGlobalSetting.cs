namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    /// <summary>
    /// Used to store global settings for Identity server
    /// </summary>
    public interface IGlobalSetting
    {
        /// <summary>
        /// Used to store the id of the configuration setting
        /// </summary>
        int MasterConfigurationSettingId { get; set; }
        /// <summary>
        /// The name of the configuration
        /// </summary>
        string SettingName { get; set; }
        /// <summary>
        /// The value of the configuration
        /// </summary>
        string Value { get; set; }
    }
}