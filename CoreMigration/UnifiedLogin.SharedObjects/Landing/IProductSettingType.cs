namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for ProductSettingType
	/// </summary>
	public interface IProductSettingType
	{
		/// <summary>
		/// Product Setting Type Description
		/// </summary>
		string Description { get; set; }

		/// <summary>
		/// Product Setting Type Name
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// ProductSettingTypeId
		/// </summary>
		int ProductSettingTypeId { get; set; }
	}
}