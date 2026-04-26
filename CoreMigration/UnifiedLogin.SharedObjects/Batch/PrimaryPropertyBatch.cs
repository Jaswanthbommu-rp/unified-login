using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Batch
{
    public class PrimaryPropertyBatch
    {
		public int PrimaryPropertyBatchProcessId { get; set; }
		public long EditorUserPersonaId { get; set; }
		public long SubjectUserPersonaId { get; set; }		
		public int StatusTypeId { get; set; }
		public int BatchProcessTypeId { get; set; }
    }

    public class BulkUserProduct
    {
        public int BulkUserBatchProcessId { get; set; }

        public int ProductId { get; set; }  
    }

    public class BulkUserBatch
    {
        public BulkUserBatch()
        {
            BulkUserProducts = new List<BulkUserProduct>();
        }

        public int BulkUserBatchProcessId { get; set; }
        public long EditorUserPersonaId { get; set; }
        public long SubjectUserPersonaId { get; set; }
        public int StatusTypeId { get; set; }
        public int BatchProcessTypeId { get; set; }
        public List<BulkUserProduct> BulkUserProducts { get; set; }

        /// <summary>
        /// The <c>UserId</c> (bigint) of the user who was impersonating when this batch was created.
        /// Matches the <c>ImpersonatorUserId</c> column on the DB table.
        /// Populated at batch-creation time (HTTP context); <c>0</c> when no impersonation was active.
        /// </summary>
        public long? ImpersonatorUserId { get; set; }
    }

}
