using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Model
{
    public class BulkUserBatch
    {
        public int BulkUserBatchProcessId { get; set; }
        public long EditorUserPersonaId { get; set; }
        public long SubjectUserPersonaId { get; set; }
        public int StatusTypeId { get; set; }
        public int BatchProcessTypeId { get; set; }
    }
}
