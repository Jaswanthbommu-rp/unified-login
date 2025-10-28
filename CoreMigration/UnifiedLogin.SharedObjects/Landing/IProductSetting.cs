using System;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Interface for ProductSetting
    /// </summary>
    public interface IProductSetting
    {
        /// <summary>
		/// ProductId
		/// </summary>
        int ProductId { get; set; }

        /// <summary>
        /// ProductSettingId
        /// </summary>
        int ProductSettingId { get; set; }

        /// <summary>
        /// ProductSettingTypeId
        /// </summary>
        int ProductSettingTypeId { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        string Description { get; set; }

		/// <summary>
		/// Product Setting From Date
		/// </summary>
		DateTime FromDate { get; set; }

		/// <summary>
		/// Product Setting Thru Date
		/// </summary>
		DateTime ThruDate { get; set; }
	}
}
