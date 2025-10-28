using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// List Response
	/// </summary>
	public class ListResponse : ResponseBase
    {
		/// <summary>
		/// Data
		/// </summary>
        public IList<object> Records { get; set; }

		/// <summary>
		/// Rows Per Page
		/// </summary>
        public int RowsPerPage { get; set; }

		/// <summary>
		/// Skip Rows
		/// </summary>
        public int SkipRows { get; set; }

		/// <summary>
		/// Current Page
		/// </summary>
        public int CurrentPage { get; set; }

		/// <summary>
		/// Total Pages
		/// </summary>
        public int TotalPages { get; set; }

		/// <summary>
		/// Total Rows
		/// </summary>
        public int TotalRows { get; set; }

		/// <summary>
		/// Additional
		/// </summary>
        public object Additional {get;set;}
		
	}
}
