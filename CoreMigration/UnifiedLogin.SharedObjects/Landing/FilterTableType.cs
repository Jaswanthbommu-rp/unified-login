using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Filter BY Type matching SQL Table Value Enterprise.FilterTableType
	/// </summary>
	public class FilterTableType : IFilterTableType
	{
		#region Constructor
		/// <summary>
		/// Profile base Constructor
		/// </summary>
		public FilterTableType()
		{
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Column name to filter by
		/// </summary>
		public string ColumnName { get; set; }

		/// <summary>
		/// Search Vlaue
		/// </summary>
		public string SearchValue { get; set; }
		#endregion
	}
}
