using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.ProductAssigner.Model
{
    public class ProductBatch
    {
        public int ProductBatchId { get; set; }
        public Guid RealPageId { get; set; }
        public long CreateUserPersonaId { get; set; }
        public long AssignUserPersonaId { get; set; }
        public int ProductId { get; set; }
        public int StatusTypeId { get; set; }
        public int RetryCount { get; set; }
        public string InputJson { get; set; }
        public string ErrorDetails { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime LastRunDate { get; set; }
    }
}
