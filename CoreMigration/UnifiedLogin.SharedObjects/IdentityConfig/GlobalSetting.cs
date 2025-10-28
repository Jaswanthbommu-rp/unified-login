using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    /// <summary>
    /// Used to store global settings for Identity server
    /// </summary>
    public class GlobalSetting : IGlobalSetting
    {
        /// <summary>
        /// Used to store the id of the configuration setting
        /// </summary>
        [JsonProperty(PropertyName = "MasterConfigurationSettingId")]
        public int MasterConfigurationSettingId { get; set; }

        /// <summary>
        /// The name of the configuration
        /// </summary>
        [JsonProperty(PropertyName = "SettingName")]
        public string SettingName { get; set; }

        /// <summary>
        /// The value of the configuration
        /// </summary>
        [JsonProperty(PropertyName = "Value")]
        public string Value { get; set; }
    }
}
