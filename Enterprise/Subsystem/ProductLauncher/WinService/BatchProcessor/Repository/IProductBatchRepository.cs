using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Model;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Repository
{
    public interface IBatchRepository
    {
        IList<Batch> GetBatchToProcess(int batchSize, bool shouldIncludeErrorRecords, int retrycount = 3);
        IList<CompanyPropertyBatch> GetCompanyBatchByStatus(int batchSize, BatchStatusType statusType);
        void UpdateBatch(IList<Batch> batch, BatchStatusType batchStatusType, string inputJson = null, string errorDetails = null);
        int UpdateBatchRecord(int productBatchId, BatchStatusType batchStatusType, string inputJson = null, string errorDetails = null);
        IList<EnterpriseRoleBatch> GetEnterpriseRoleProductUpdateBatchToProcess(int batchSize);
        void UpdateEnterpriseRoleProductBatch(long productBatchId, int statusTypeId);
        IList<PrimaryPropertyBatch> GetPrimaryPropertyProductUpdateBatchToProcess(int batchSize);
        IList<BulkUserBatch> GetBulkUsersUpdateBatchToProcess(int batchSize);        
        void UpdatePrimaryPropertyProductBatch(long productBatchId, int statusTypeId);
        void UpdateBulkUserBatch(long productBatchId, int statusTypeId);
        void UpdateCompanyPropertyBatch(long companyBatchJobId, int statusTypeId);
    }
}