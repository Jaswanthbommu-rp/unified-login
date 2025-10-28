namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Filter BY Type matching SQL Table Value Enterprise.FilterTableType
	/// </summary>
	public interface IFilterTableType
	{
		/// <summary>
		/// Column name to filter by
		/// </summary>
		string ColumnName { get; set; }

		/// <summary>
		/// Search Vlaue
		/// </summary>
		string SearchValue { get; set; }
	}
}