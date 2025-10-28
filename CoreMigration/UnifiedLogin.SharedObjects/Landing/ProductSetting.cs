using System;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Product Setting
    /// </summary>
    public class ProductSetting : IProductSetting
    {
        /// <summary>
		/// ProductId
		/// </summary>
        public int ProductId { get; set; } = 0;

        /// <summary>
        /// ProductSettingId
        /// </summary>
        public int ProductSettingId { get; set; } = 0;

        /// <summary>
        /// ProductSettingTypeId
        /// </summary>
        public int ProductSettingTypeId { get; set; } = 0;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Value
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; } = string.Empty;

		/// <summary>
		/// Product Setting From Date
		/// </summary>
		public DateTime FromDate { get; set; }

		/// <summary>
		/// Product Setting Thru Date
		/// </summary>
		public DateTime ThruDate { get; set; }
	}
}

