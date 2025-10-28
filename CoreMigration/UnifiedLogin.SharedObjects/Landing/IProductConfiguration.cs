using System;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for ProductConfiguration
	/// </summary>
	public interface IProductConfiguration
	{
		/// <summary>
		/// ConfigurationId
		/// </summary>
		int ConfigurationId { get; set; }

		/// <summary>
		/// Product Configuration From Date
		/// </summary>
		DateTime FromDate { get; set; }

		/// <summary>
		/// ProductConfiguration Unique Id
		/// </summary>
		int ProductConfigurationId { get; set; }

		/// <summary>
		/// ProductSettingId
		/// </summary>
		int ProductSettingId { get; set; }

		/// <summary>
		/// Product Configuration Thru Date
		/// </summary>
		DateTime ThruDate { get; set; }
	}
}