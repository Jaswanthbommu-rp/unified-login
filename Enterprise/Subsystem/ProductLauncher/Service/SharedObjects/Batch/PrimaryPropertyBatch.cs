using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Batch
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
    }
    public class CompanyPropertyBatch
    {
        public long CompanyBatchJobId { get; set; }
        public string CompanyInstanceSourceId { get; set; }
        public int StatusTypeId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastRunDateTime { get; set; }
        public long CreatedBy { get; set; }
        public int BatchProcessorTypeId { get; set; }
        public long CreateUserPersonaId { get; set; }
        public int IsActive { get; set; }
    }

}
