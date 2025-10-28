namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Sort BY Type matching SQL Table Value Enterprise.SortTableType
	/// </summary>
	public interface ISortTableType
	{
		/// <summary>
		/// Column name to sort by
		/// </summary>
		string ColumnName { get; set; }

		/// <summary>
		/// Sort direction (ASC or DESC)
		/// </summary>
		string SortDirection { get; set; }
	}
}