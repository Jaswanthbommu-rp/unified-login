using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Focused service for product-access activity logging.
/// Extracted from the <c>ManageProductBase.WriteXxxActivityLog</c> family — only managers
/// that raise product-access events inject this.
/// <para>
/// Centralises the seven audit operations that were previously scattered as protected helpers
/// on <c>ManageProductBase</c>:
/// </para>
/// <list type="number">
///   <item><see cref="GetUserActivityLogInfoAsync"/> — shared persona-to-log-info resolver.</item>
///   <item><see cref="WriteProductEventAsync"/> — generic event with a caller-supplied template.</item>
///   <item><see cref="WriteUnassignAsync"/> — replaces <c>WriteUnassignActivityLog</c>.</item>
///   <item><see cref="WriteDeactivateAsync"/> — replaces <c>WriteDeActivatedActivityLog</c>.</item>
///   <item><see cref="WriteReactivateAsync"/> — replaces <c>WriteReActivatedActivityLog</c>.</item>
///   <item><see cref="WriteCreateUserAsync"/> — replaces <c>WriteCreateUserActivityLog</c>.</item>
///   <item><see cref="WriteResetVerificationCodeAsync"/> — replaces <c>WriteResetVerificationCodeActivityLog</c>.</item>
///   <item><see cref="WriteUserTypeChangeAsync"/> — replaces <c>WriteUpdateUserTypeActivityLog</c>.</item>
/// </list>
/// </summary>
public interface IProductAuditServiceAsync
{
    // ── 0. Shared resolver ────────────────────────────────────────────────

    /// <summary>
    /// Resolves a <see cref="UserActivityLogInfoAsync"/> for <paramref name="personaId"/>.
    /// Exposed on the interface so any manager can reuse the lookup without duplicating
    /// the persona + person + userLogin fetch logic.
    /// Previously duplicated as private <c>GetUserActivityLogInfo(personaId)</c>
    /// on <c>ManageProductBase</c> and as a private helper in several async managers.
    /// </summary>
    Task<UserActivityLogInfoAsync> GetUserActivityLogInfoAsync(
        long personaId, CancellationToken ct = default);

    // ── 1. Generic ────────────────────────────────────────────────────────

    /// <summary>
    /// Logs a product-access event using a caller-supplied format string.
    /// Placeholders follow the original convention:
    /// <c>{0}</c>=toFirst, <c>{1}</c>=toLast, <c>{2}</c>=productName,
    /// <c>{3}</c>=fromFirst, <c>{4}</c>=fromLast.
    /// Replaces: <c>WriteActivityLogWithMessage</c> and
    /// <c>WriteActivityLogWithMessageByProduct</c> (pass the specific <paramref name="productId"/>).
    /// </summary>
    Task WriteProductEventAsync(
        long fromPersonaId, long toPersonaId, int productId,
        string messageTemplate, CancellationToken ct = default);

    // ── 2–4. Lifecycle events ─────────────────────────────────────────────

    /// <summary>Replaces: <c>WriteUnassignActivityLog(fromPersonaId, toPersonaId)</c>.</summary>
    Task WriteUnassignAsync(
        long fromPersonaId, long toPersonaId, int productId, CancellationToken ct = default);

    /// <summary>Replaces: <c>WriteDeActivatedActivityLog(fromPersonaId, toPersonaId)</c>.</summary>
    Task WriteDeactivateAsync(
        long fromPersonaId, long toPersonaId, int productId, CancellationToken ct = default);

    /// <summary>Replaces: <c>WriteReActivatedActivityLog(fromPersonaId, toPersonaId)</c>.</summary>
    Task WriteReactivateAsync(
        long fromPersonaId, long toPersonaId, int productId, CancellationToken ct = default);

    // ── 5–7. User management events ───────────────────────────────────────

    /// <summary>
    /// Replaces: <c>WriteCreateUserActivityLog(fromPersonaId, toPerson, toUserGbLogin)</c>.
    /// </summary>
    Task WriteCreateUserAsync(
        long fromPersonaId, long toPersonaId, int productId, CancellationToken ct = default);

    /// <summary>
    /// Replaces: <c>WriteResetVerificationCodeActivityLog(fromPersonaId, toPerson, toUserGbLogin)</c>.
    /// </summary>
    Task WriteResetVerificationCodeAsync(
        long fromPersonaId, long toPersonaId, int productId, CancellationToken ct = default);

    /// <summary>
    /// Replaces: <c>WriteUpdateUserTypeActivityLog(fromPersonaId, toPerson, toUserGbLogin, batchProcessType)</c>.
    /// No-ops silently when <paramref name="batchProcessType"/> does not map to a user-type change.
    /// </summary>
    Task WriteUserTypeChangeAsync(
        long fromPersonaId, long toPersonaId, int productId,
        BatchProcessType batchProcessType, CancellationToken ct = default);
}