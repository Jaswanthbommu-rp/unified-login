using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.BatchProcessor;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for batch processing operations.
/// Delegates to the existing sync implementations via <see cref="Task.FromResult{TResult}"/>.
/// </summary>
public sealed class BatchProcessServiceAsync : IBatchProcessServiceAsync
{
    public Task<string> ProcessBatchAsync(ProductUserProperitiesRoles batchRecord, CancellationToken cancellationToken = default)
    {
        var logic = new BatchProcessorLogic();
        return Task.FromResult(logic.ProcessBatch(batchRecord));
    }

    public Task<string> ProcessEnterpriseRoleProductBatchAsync(EnterpriseRoleBatch batchRecord, DefaultUserClaim userClaim, CancellationToken cancellationToken = default)
    {
        var logic = new ManageEnterpriseRoleProductBatch(userClaim);
        return Task.FromResult(logic.GenerateEnterpriseRoleUserProductBatch(batchRecord));
    }

    public Task<string> ProcessPrimaryPropertyBatchAsync(PrimaryPropertyBatch batchRecord, DefaultUserClaim userClaim, CancellationToken cancellationToken = default)
    {
        var logic = new ManagePrimaryPropertiesBatch(userClaim);
        return Task.FromResult(logic.GeneratePrimaryPropertiesUserProductBatch(batchRecord));
    }

    public Task<string> ProcessBulkUserBatchAsync(BulkUserBatch batchRecord, DefaultUserClaim userClaim, CancellationToken cancellationToken = default)
    {
        var logic = new ManageBulkUserBatch(userClaim);
        return Task.FromResult(logic.GenerateProductUnAssignProductBatch(batchRecord));
    }
}
