using Aspose.Cells;
using System.Collections.Generic;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Models
{
	/// <summary>
	/// Activity Log Filter Criteria
	/// </summary>
    public class ActivityLogFilterCriteria
    {
		/// <summary>
		/// List of Activity Search Criteria
		/// </summary>
        public List<ActivitySearchCriteria> ActivitySearchCriteria { get; set; }

		/// <summary>
		/// SortOrder
		/// </summary>
        public string SortOrder { get; set; }

		/// <summary>
		/// SortOrder ColumnName
		/// </summary>
        public string SortOrderColumnName { get; set; }

		/// <summary>
		/// Rows Per Page
		/// </summary>
        public int RowsPerPage { get; set; }

		/// <summary>
		/// Page Number
		/// </summary>
        public int PageNumber { get; set; }

		/// <summary>
		/// Export file format (default = CSV)
		/// </summary>
		public SaveFormat DataFormat { get; set; } = SaveFormat.CSV;
	}

	/// <summary>
	/// Activity Search Criteria
	/// </summary>
    public class ActivitySearchCriteria
    {
		/// <summary>
		/// Column Name	
		/// </summary>
        public string Name { get; set; }

		/// <summary>
		/// Value
		/// </summary>
        public string Value { get; set; }
    }
}