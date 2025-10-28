namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    /// <summary>
    /// Used to store settings used with a product
    /// </summary>
    public class ProductInternalSetting
    {
        /// <summary>
        /// The specific product configuration id of the setting
        /// </summary>
        public string ProductConfigurationId { get; set; }

        /// <summary>
        /// The configuration id the product belongs to
        /// </summary>
        public string ConfigurationId { get; set; }

        /// <summary>
        /// The type of setting
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The value of the setting
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Does the setting contain sensitive data
        /// </summary>
        public bool SensitiveData { get; set; }
    }
}
