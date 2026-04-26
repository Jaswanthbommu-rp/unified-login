using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first bulk-user-batch entry point.
/// <para>
/// Replaces <c>ManageBulkUserBatch</c> which accepted <c>DefaultUserClaim</c>
/// via its constructor and created all dependencies inline with
/// <c>new Xxx(_userClaim)</c>.
/// </para>
/// <para>
/// Key improvements over the sync version:
/// <list type="bullet">
///   <item>All I/O is awaited — no blocking calls.</item>
///   <item>All dependencies are DI-injected; no <c>DefaultUserClaim</c> constructor parameter.</item>
///   <item><c>ManageBulkUsers</c> inline construction replaced by injected
///         <see cref="IManageBulkUsersAsync"/>.</item>
///   <item>Impersonation context read from <see cref="BulkUserBatch.ImpersonatedBy"/>
///         (stored at batch-creation time in an HTTP context) and forwarded explicitly
///         to <see cref="IManageBulkUsersAsync.ProcessProductUnAssignBatchDataAsync"/>.
///         This removes the dependency on <c>IUserClaimsAccessor</c>, which is unavailable
///         in the batch-processor service that calls this method via an HTTP API call
///         using a service-account token rather than the original user's JWT.</item>
/// </list>
/// </para>
/// <para>
/// <b>Note:</b> The sync call to <c>ManageProductBatch.GetPersonaRoleRights</c>
/// (which populated <c>_userClaim.Rights</c>) is intentionally omitted.
/// <see cref="ManageBulkUsersAsync.ProcessProductUnAssignBatchDataAsync"/> does not
/// consult <c>Rights</c> in its logic; the mutation was only needed because the sync
/// <c>ManageBulkUsers</c> received the mutated <c>DefaultUserClaim</c> object.
/// Blocked on: <c>IManageProductBatchAsync.GetPersonaRoleRightsAsync</c> — add if
/// rights must be re-populated in a background / out-of-request context.
/// </para>
/// <para><b>DI registration:</b> <c>Scoped</c>.</para>
/// </summary>
public sealed class ManageBulkUserBatchAsync : IManageBulkUserBatchAsync
{
    #region Fields

    private readonly IManagePersonaAsync                    _managePersona;
    private readonly IManageBulkUsersAsync                  _manageBulkUsers;
    private readonly IBatchProductBulkUpdateRepositoryAsync _batchRepository;
    private readonly ILogger<ManageBulkUserBatchAsync>      _logger;

    #endregion

    #region Constructor

    public ManageBulkUserBatchAsync(
        IManagePersonaAsync                    managePersona,
        IManageBulkUsersAsync                  manageBulkUsers,
        IBatchProductBulkUpdateRepositoryAsync batchRepository,
        ILogger<ManageBulkUserBatchAsync>      logger)
    {
        _managePersona   = managePersona   ?? throw new ArgumentNullException(nameof(managePersona));
        _manageBulkUsers = manageBulkUsers ?? throw new ArgumentNullException(nameof(manageBulkUsers));
        _batchRepository = batchRepository ?? throw new ArgumentNullException(nameof(batchRepository));
        _logger          = logger          ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Public Methods

    /// <inheritdoc/>
    public async Task<string> GenerateProductUnAssignProductBatchAsync(
        BulkUserBatch batch,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batch);

        try
        {
            // ── 1. Resolve editor persona for RealPageId / OrgId context ─────
            // NOTE: GetPersonaRoleRights (sync) is intentionally omitted here.
            // See class-level XML doc for rationale. Add IManageProductBatchAsync
            // injection if rights re-population in background context is required.
            var editorPersona = await _managePersona.GetPersonaAsync(
                batch.EditorUserPersonaId, withRights: false, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogDebug(
                "GenerateProductUnAssignProductBatchAsync: starting for editor={EditorPersonaId} subject={SubjectPersonaId} batch={BatchId}",
                batch.EditorUserPersonaId, batch.SubjectUserPersonaId, batch.BulkUserBatchProcessId);

            // ── 2. Delegate the core un-assignment logic ──────────────────────
            // batch.ImpersonatorUserId (DB: ImpersonatorUserId bigint) was stored at
            // batch-creation time (HTTP context). Forwarding it explicitly means no
            // IUserClaimsAccessor dependency and no secondary GetUserLoginOnly lookup.
            string statusMessage = await _manageBulkUsers.ProcessProductUnAssignBatchDataAsync(
                batch.EditorUserPersonaId,
                batch.SubjectUserPersonaId,
                batch.BulkUserBatchProcessId,
                batch.ImpersonatorUserId,
                cancellationToken).ConfigureAwait(false);

            // ── 3. Update batch status ────────────────────────────────────────
            int statusTypeId = string.IsNullOrEmpty(statusMessage)
                ? (int)ProductBatchStatusType.Success
                : (int)ProductBatchStatusType.Error;

            await _batchRepository.UpdateBulkUserProductBatchAsync(
                batch.BulkUserBatchProcessId, statusTypeId, cancellationToken)
                .ConfigureAwait(false);

            return statusMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "GenerateProductUnAssignProductBatchAsync failed for batch {BatchId}",
                batch.BulkUserBatchProcessId);

            // Best-effort status update on exception — do not re-throw the update failure
            try
            {
                await _batchRepository.UpdateBulkUserProductBatchAsync(
                    batch.BulkUserBatchProcessId, (int)ProductBatchStatusType.Error, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception updateEx)
            {
                _logger.LogWarning(updateEx,
                    "GenerateProductUnAssignProductBatchAsync: failed to update batch status after exception for batch {BatchId}",
                    batch.BulkUserBatchProcessId);
            }

            return "Error";
        }
    }

    #endregion
}
