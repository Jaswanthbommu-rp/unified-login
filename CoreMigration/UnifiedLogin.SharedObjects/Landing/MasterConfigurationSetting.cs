namespace UnifiedLogin.SharedObjects.Landing
{
    public class MasterConfigurationSetting
    {
        /// <summary>
        /// The configuration type. Global
        /// </summary>
        public string ConfigurationType { get; set; }

        /// <summary>
        /// The setting type allowed. IdentityServerCorsAllowedOrigins, LandingApiCorsAllowedOrigins
        /// </summary>
        public string SettingType { get; set; }

        /// <summary>
        /// The value for the setting
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The company id the setting belongs, null should be used for global settings
        /// </summary>
        public string PartyId { get; set; } = null;

        /// <summary>
        /// Created by id
        /// </summary>
        public long CreatedBy { get; set; }

        /// <summary>
        /// MappingName
        /// </summary>
        public string MappingName { get; set; }
    }
}
