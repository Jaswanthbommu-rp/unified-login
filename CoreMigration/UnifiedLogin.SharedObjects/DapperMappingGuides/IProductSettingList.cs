namespace UnifiedLogin.SharedObjects.DapperMappingGuides
{
	/// <summary>
	/// Interface for ProductSettingList
	/// </summary>
	public interface IProductSettingList
	{
		/// <summary>
		/// Product Setting Name
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Unique Product Id
		/// </summary>
		int ProductId { get; set; }

		/// <summary>
		/// Unique Product Setting Id
		/// </summary>
		int ProductSettingId { get; set; }

		/// <summary>
		/// Product Setting Value
		/// </summary>
		string Value { get; set; }
	}
}