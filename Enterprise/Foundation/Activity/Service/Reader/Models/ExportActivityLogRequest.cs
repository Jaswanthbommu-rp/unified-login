using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Models
{
	public class ExportActivityLogRequest
	{
		public ActivityLogFilterCriteria FilterCriteria { get; set; }
		public IDictionary<string, string> HeaderValuePropMap { get; set; }
	}
}