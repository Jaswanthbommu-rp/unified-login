namespace UnifiedLogin.SharedObjects
{
    /// <summary>
    /// Product Setting
    /// </summary>
    public class ProductSettingList : IProductSettingList
	{
        /// <summary>
        /// Unique Product Id
        /// </summary>
        public int ProductId { get; set; } = 0;

        /// <summary>
        /// Unique Product Setting Id
        /// </summary>
        public int ProductSettingId { get; set; } = 0;
        
        /// <summary>
        /// Product Setting Name
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Product Setting Value
        /// </summary>
        public string Value { get; set; } = "";        
    }
}
