using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.ProductAssigner.Model;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.ProductAssigner.Repository
{
    public interface IProductBatchRepository
    {
        IList<ProductBatch> GetProductBatchToProcess(int batchSize, bool shouldIncludeErrorRecords, int retrycount = 3);
        void UpdateBatch(IList<ProductBatch> batch, BatchStatusType batchStatusType, string inputJson = null, string errorDetails = null);
        int UpdateBatchRecord(int productBatchId, BatchStatusType batchStatusType, string inputJson = null, string errorDetails = null);
    }
}