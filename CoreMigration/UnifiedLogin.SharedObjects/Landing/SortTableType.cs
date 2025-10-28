using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Sort BY Type matching SQL Table Value Enterprise.SortTableType
	/// </summary>
	public class SortTableType : ISortTableType
	{
		#region Constructor
		/// <summary>
		/// Profile base Constructor
		/// </summary>
		public SortTableType()
		{
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Column name to sort by
		/// </summary>
		public string ColumnName { get; set; }

		/// <summary>
		/// Sort direction (ASC or DESC)
		/// </summary>
		public string SortDirection { get; set; }
		#endregion
	}
}
