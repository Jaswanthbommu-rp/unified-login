using Aspose.Cells;
using System;
using System.Collections.Generic;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Models
{
    public class ActivityLogDetailExportRequest
    {
        public SaveFormat DataFormat { get; set; } = SaveFormat.CSV;

        public IEnumerable<ActivityLogHeader> HeaderColumns { get; set; }

        public ICollection<object> RowData { get; set; }
    }

    public class ActivityLogHeader
    {
        public string Key { get; set; }

        public string Header { get; set; }

        public double Width { get; set; }
    }
}
