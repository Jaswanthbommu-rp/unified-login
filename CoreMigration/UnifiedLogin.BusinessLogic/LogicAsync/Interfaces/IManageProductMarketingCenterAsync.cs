using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.MarketingCenter;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// True async interface for Marketing Center product-management operations.
/// <para>
/// <see cref="DefaultUserClaim"/> has been removed from all signatures — per-call context is
/// resolved internally via <see cref="IProductContextServiceAsync"/> from
/// <paramref name="editorPersonaId"/> / <paramref name="userPersonaId"/>.
/// </para>
/// </summary>
public interface IManageProductMarketingCenterAsync
{
    /// <summary>
    /// Returns the list of Marketing Center roles for the company.
    /// When <paramref name="userPersonaId"/> is non-zero and the user has a product login,
    /// the user's assigned role is pre-selected.
    /// </summary>
    Task<ListResponse> GetRolesAsync(
        long editorPersonaId,
        long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the list of Marketing Center properties for the company.
    /// When <paramref name="userPersonaId"/> is non-zero, the user's currently assigned
    /// properties are flagged and any extra properties are appended to the list.
    /// </summary>
    Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId,
        long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Creates or updates a Marketing Center user identified by <paramref name="userPersonaId"/>,
    /// assigning the specified role and properties.
    /// Returns <c>(string.Empty, additionalParameters)</c> on success, or
    /// <c>(errorMessage, [])</c> on failure.
    /// </summary>
    Task<(string result, List<AdditionalParameters> additionalParameters)> ManageMarketingCenterUserAsync(
        long editorPersonaId,
        long userPersonaId,
        List<int> roleList,
        List<string> propertyList,
        bool isAssignedNewPropertyByDefault,
        CancellationToken ct = default);

    /// <summary>
    /// Activates or deactivates the Marketing Center user identified by
    /// <paramref name="productUserId"/>. Returns <c>true</c> on success.
    /// </summary>
    Task<bool> ChangeUserStatusAsync(
        long editorPersonaId,
        string userName,
        string productUserId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the full Marketing Center role list for the company
    /// (used by the Roles and Rights Access page).
    /// </summary>
    Task<ListResponse> GetRolesCountAsync(
        long editorPersonaId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the full Marketing Center rights list for the company.
    /// </summary>
    Task<ListResponse> GetRightsAsync(
        long editorPersonaId,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes the Marketing Center role identified by <paramref name="roleId"/>.
    /// </summary>
    Task<ListResponse> DeleteRoleAsync(
        long editorPersonaId,
        int roleId,
        CancellationToken ct = default);

    /// <summary>
    /// Activates or deactivates the Marketing Center role identified by <paramref name="roleId"/>.
    /// </summary>
    Task<ListResponse> UpdateRoleStatusAsync(
        long editorPersonaId,
        int roleId,
        bool isActive,
        CancellationToken ct = default);

    /// <summary>
    /// Returns all Marketing Center roles that have access to the right identified by
    /// <paramref name="rightId"/>, with <c>IsAssigned</c> set on each record.
    /// </summary>
    Task<ListResponse> GetRolesForRightIdAsync(
        long editorPersonaId,
        int rightId,
        CancellationToken ct = default);

    /// <summary>
    /// Replaces the role-to-right assignment for <paramref name="rightId"/> with the
    /// supplied <paramref name="roleList"/>.
    /// </summary>
    Task<ListResponse> UpdateRolesForRightAsync(
        long editorPersonaId,
        int rightId,
        List<string> roleList,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the rights assigned to the Marketing Center role identified by
    /// <paramref name="roleId"/>. When <paramref name="roleId"/> is zero, all company
    /// rights are returned.
    /// </summary>
    Task<ListResponse> GetRightsForRoleIdAsync(
        long editorPersonaId,
        int roleId,
        CancellationToken ct = default);

    /// <summary>
    /// Creates a new Marketing Center role with the supplied rights assignment.
    /// </summary>
    Task<ListResponse> CreateNewMCRoleWithRightsAsync(
        long editorPersonaId,
        MCRole mcRole,
        CancellationToken ct = default);

    /// <summary>
    /// Replaces an existing Marketing Center role definition and its rights assignment.
    /// </summary>
    Task<ListResponse> UpdateMCRoleWithRightsAsync(
        long editorPersonaId,
        MCRole mcRole,
        CancellationToken ct = default);

    /// <summary>
    /// Returns a paged list of Marketing Center users filtered by migration status
    /// via <c>datafilter.FilterBy["filter"]</c> (default: "NonMigrated").
    /// </summary>
    Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Submits a batch migration-status update to the Marketing Center API,
    /// enabling the unified-login flag per user.
    /// </summary>
    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId,
        IList<MigrateUser> migrateUsers,
        CancellationToken ct = default);
}
