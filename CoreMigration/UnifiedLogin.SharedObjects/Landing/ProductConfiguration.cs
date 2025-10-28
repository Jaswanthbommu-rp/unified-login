using System;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Product Configuration
	/// </summary>
	public class ProductConfiguration : IProductConfiguration
	{
		/// <summary>
		/// ProductConfiguration Unique Id
		/// </summary>
		public int ProductConfigurationId { get; set; }

		/// <summary>
		/// ConfigurationId
		/// </summary>
		public int ConfigurationId { get; set; }

		/// <summary>
		/// ProductSettingId
		/// </summary>
		public int ProductSettingId { get; set; }

		/// <summary>
		/// Product Configuration From Date
		/// </summary>
		public DateTime FromDate { get; set; }

		/// <summary>
		/// Product Configuration Thru Date
		/// </summary>
		public DateTime ThruDate { get; set; }
	}
}
