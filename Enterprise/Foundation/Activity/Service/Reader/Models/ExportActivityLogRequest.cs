using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Models
{
	public class ExportActivityLogRequest
	{
		public ActivityLogFilterCriteria FilterCriteria { get; set; }

		public IList<ColumnMapping> ColumnMappings { get; set; }
	}

	public class ColumnMapping
	{
		public string Key { get; set; }

		public string Label { get; set; }

		public double Width { get; set; }
	}
}