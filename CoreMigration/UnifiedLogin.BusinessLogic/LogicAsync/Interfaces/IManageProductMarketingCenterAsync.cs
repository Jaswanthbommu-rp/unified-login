using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.MarketingCenter;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Native-async interface for Marketing Center product operations.
/// All per-call <c>DefaultUserClaim</c> parameters removed — user context is resolved
/// internally via <see cref="IUserClaimsAccessor"/> injected at construction time.
/// </summary>
public interface IManageProductMarketingCenterAsync
{
    // ── User management ───────────────────────────────────────────────────────

    Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a Marketing Center user.
    /// Replaces the sync <c>ManageMarketingCenterUser</c> which used an <c>out</c> parameter
    /// for activity-log details — now returned as part of the result tuple.
    /// </summary>
    Task<(string Result, List<AdditionalParameters> ActivityLog)> ManageMarketingCenterUserAsync(
        long editorPersonaId, long userPersonaId,
        List<int> roleList, List<string> propertyList,
        bool isAssignedNewPropertyByDefault,
        CancellationToken cancellationToken = default);

    Task<string> UnassignUserAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken cancellationToken = default);

    Task<string> UpdateUserProfileAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken cancellationToken = default);

    Task<bool> ChangeUserStatusAsync(
        long editorPersonaId, string userName, string productUserId,
        bool isActive = false,
        CancellationToken cancellationToken = default);

    // ── Role / Right setup ────────────────────────────────────────────────────

    Task<ListResponse> GetRolesCountAsync(
        long editorPersonaId,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetRightsAsync(
        long editorPersonaId,
        CancellationToken cancellationToken = default);

    Task<ListResponse> DeleteRoleAsync(
        long editorPersonaId, int roleId,
        CancellationToken cancellationToken = default);

    Task<ListResponse> UpdateRoleStatusAsync(
        long editorPersonaId, int roleId, bool isActive,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetRolesForRightIdAsync(
        long editorPersonaId, int rightId,
        CancellationToken cancellationToken = default);

    Task<ListResponse> UpdateRolesForRightAsync(
        long editorPersonaId, int rightId, List<string> roleList,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetRightsForRoleIdAsync(
        long editorPersonaId, int roleId,
        CancellationToken cancellationToken = default);

    Task<ListResponse> CreateNewMCRoleWithRightsAsync(
        long editorPersonaId, MCRole mcRole,
        CancellationToken cancellationToken = default);

    Task<ListResponse> UpdateMCRoleWithRightsAsync(
        long editorPersonaId, MCRole mcRole,
        CancellationToken cancellationToken = default);

    // ── Migration ─────────────────────────────────────────────────────────────

    Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter datafilter,
        CancellationToken cancellationToken = default);

    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers,
        CancellationToken cancellationToken = default);
}
