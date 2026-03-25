using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for batch processing operations.
/// Replaces: <c>new BatchProcessorLogic()</c>, <c>new ManageEnterpriseRoleProductBatch(userClaim)</c>,
/// <c>new ManagePrimaryPropertiesBatch(userClaim)</c>, and <c>new ManageBulkUserBatch(userClaim)</c>
/// instantiated inside controller action bodies.
/// </summary>
public interface IBatchProcessServiceAsync
{
    /// <summary>Process a product user properties/roles batch record.</summary>
    Task<string> ProcessBatchAsync(ProductUserProperitiesRoles batchRecord, CancellationToken cancellationToken = default);

    /// <summary>Process an enterprise role product batch record.</summary>
    Task<string> ProcessEnterpriseRoleProductBatchAsync(EnterpriseRoleBatch batchRecord, DefaultUserClaim userClaim, CancellationToken cancellationToken = default);

    /// <summary>Process a primary property batch record.</summary>
    Task<string> ProcessPrimaryPropertyBatchAsync(PrimaryPropertyBatch batchRecord, DefaultUserClaim userClaim, CancellationToken cancellationToken = default);

    /// <summary>Process a bulk user batch record.</summary>
    Task<string> ProcessBulkUserBatchAsync(BulkUserBatch batchRecord, DefaultUserClaim userClaim, CancellationToken cancellationToken = default);
}
