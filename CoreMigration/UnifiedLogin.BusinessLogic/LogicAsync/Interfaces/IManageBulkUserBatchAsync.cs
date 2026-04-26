using UnifiedLogin.SharedObjects.Batch;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for the bulk-user-batch entry point.
/// <para>
/// Replaces <c>ManageBulkUserBatch</c> which accepted <c>DefaultUserClaim</c>
/// via its constructor and instantiated all dependencies inline.
/// </para>
/// <para><b>DI registration:</b> <c>Scoped</c>.</para>
/// </summary>
public interface IManageBulkUserBatchAsync
{
    /// <summary>
    /// Orchestrates the product un-assignment batch for a single <see cref="BulkUserBatch"/>
    /// entry: resolves the editor persona, delegates processing to
    /// <see cref="IManageBulkUsersAsync"/>, and updates the batch status.
    /// </summary>
    /// <returns>An empty string on success; <c>"Error"</c> on failure.</returns>
    Task<string> GenerateProductUnAssignProductBatchAsync(
        BulkUserBatch batch,
        CancellationToken cancellationToken = default);
}
