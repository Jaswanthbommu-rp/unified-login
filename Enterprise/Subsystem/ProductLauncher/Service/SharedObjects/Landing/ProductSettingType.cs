namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	/// <summary>
	/// Product Setting Type
	/// </summary>
	public class ProductSettingType : IProductSettingType
	{
		/// <summary>
		/// ProductSettingTypeId
		/// </summary>
		public int ProductSettingTypeId { get; set; }

		/// <summary>
		/// Product Setting Type Name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Product Setting Type Description
		/// </summary>
		public string Description { get; set; }
	}
}
