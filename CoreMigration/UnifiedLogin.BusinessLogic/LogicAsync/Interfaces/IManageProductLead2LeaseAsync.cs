using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Native-async interface for Lead2Lease product operations.
/// All per-call <c>DefaultUserClaim</c> parameters removed — user context is resolved
/// internally via <see cref="IUserClaimsAccessor"/> injected at construction time.
/// </summary>
public interface IManageProductLead2LeaseAsync
{
    // ── Role / Property reads ────────────────────────────────────────────────

    Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter,
        CancellationToken cancellationToken = default);

    // ── User management ──────────────────────────────────────────────────────

    /// <summary>
    /// Creates or updates a Lead2Lease user.
    /// Replaces the sync <c>ManageLead2LeaseUser</c> which used an <c>out</c> parameter
    /// for activity-log details — now returned as part of the result tuple.
    /// </summary>
    Task<(string Error, List<AdditionalParameters> ActivityLog)> ManageLead2LeaseUserAsync(
        long editorPersonaId, long userPersonaId,
        List<string> roleList, List<string> propertyList,
        CancellationToken cancellationToken = default);

    Task<string> UnassignUserAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken cancellationToken = default);

    Task<string> UpdateLead2LeaseUserProfileAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken cancellationToken = default);

    Task<bool> ChangeUserStatusAsync(
        long editorPersonaId, string userName, string productUserId,
        bool isActive = false,
        CancellationToken cancellationToken = default);

    // ── Migration ────────────────────────────────────────────────────────────

    Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter datafilter,
        CancellationToken cancellationToken = default);

    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers,
        CancellationToken cancellationToken = default);
}
