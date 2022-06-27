namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Model
{
	public class BatchConfiguration
	{
		public int BatchProcessTypeId { get; set; }
		public string ProcessName { get; set; }
		public string ConfigurationTypeName { get; set; }
		public string Value { get; set; }
	}

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
