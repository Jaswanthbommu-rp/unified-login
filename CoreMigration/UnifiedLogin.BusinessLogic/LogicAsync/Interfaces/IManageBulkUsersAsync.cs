namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for bulk user product un-assignment operations.
/// <para>
/// Replaces <c>ManageBulkUsers</c> which extended <c>BaseRepository</c>
/// and accepted <c>DefaultUserClaim</c> via its constructor, creating all
/// dependencies inline with <c>new Xxx(_userClaim)</c>.
/// </para>
/// <para><b>DI registration:</b> <c>Scoped</c>.</para>
/// </summary>
public interface IManageBulkUsersAsync
{
    /// <summary>
    /// Processes the product un-assignment batch for a bulk user operation.
    /// Resolves the subject user's active products, intersects them with the
    /// requested un-assignment list, builds the product batch payload, and
    /// persists it via <c>IBatchProductBulkUpdateRepositoryAsync</c>.
    /// </summary>
    /// <param name="editorUserPersonaId">Persona ID of the user who initiated the batch.</param>
    /// <param name="subjectUserPersonaId">Persona ID of the user being un-assigned.</param>
    /// <param name="bulkUserBatchProcessId">Batch record identifier.</param>
    /// <param name="impersonatorUserId">
    /// The <c>UserId</c> of the impersonating user at batch-creation time, or <c>null</c>
    /// when no impersonation was active.  Stored on
    /// <see cref="UnifiedLogin.SharedObjects.Batch.BulkUserBatch.ImpersonatorUserId"/>
    /// (DB column: <c>ImpersonatorUserId bigint null</c>) so it survives the hand-off
    /// from the HTTP context to the batch processor service.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// An empty string on success; <c>"Error"</c> when the batch insert fails.
    /// </returns>
    Task<string> ProcessProductUnAssignBatchDataAsync(
        long editorUserPersonaId,
        long subjectUserPersonaId,
        int bulkUserBatchProcessId,
        long? impersonatorUserId,
        CancellationToken cancellationToken = default);
}
