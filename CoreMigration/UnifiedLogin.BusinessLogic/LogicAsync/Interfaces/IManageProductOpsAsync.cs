using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.Ops;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// True async interface for Ops (Spend Management) user-management operations.
/// <para>
/// Replaces the stepping-stone wrapper that required a <c>DefaultUserClaim</c> on every call.
/// User context is now resolved internally via <see cref="IProductContextServiceAsync"/>,
/// so callers only supply the persona IDs they already have.
/// </para>
/// <para>
/// The <c>out List&lt;AdditionalParameters&gt;</c> parameter from the sync
/// <c>ManageOpsUser</c> is replaced by a tuple return since async methods cannot
/// use <c>out</c> parameters.
/// </para>
/// </summary>
public interface IManageProductOpsAsync
{
    // ── Asset Groups ────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the list of Ops asset groups (or a single group when
    /// <paramref name="assetGroupId"/> is non-zero), with the user's assigned group
    /// pre-selected when <paramref name="userPersonaId"/> belongs to an existing product user.
    /// </summary>
    Task<ListResponse> GetOpsAssetGroupsAsync(
        long editorPersonaId, long userPersonaId, int assetGroupId = 0,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the Ops property / portfolio list, filtered by <paramref name="status"/>,
    /// with the user's assigned asset pre-selected for existing product users.
    /// </summary>
    Task<ListResponse> GetOpsAssetsAsync(
        long editorPersonaId, long userPersonaId, string status,
        CancellationToken ct = default);

    /// <summary>Creates a new Ops asset group.</summary>
    Task<ListResponse> CreateOpsAssetGroupAsync(
        long editorPersonaId, long userPersonaId, AssetGroupCreate assetGroup,
        CancellationToken ct = default);

    /// <summary>Replaces an existing Ops asset group.</summary>
    Task<ListResponse> UpdateOpsAssetGroupAsync(
        long editorPersonaId, long userPersonaId, int assetGroupId, AssetGroupCreate assetGroup,
        CancellationToken ct = default);

    /// <summary>Partially updates an Ops asset group (name / status).</summary>
    Task<ListResponse> PatchOpsAssetGroupAsync(
        long editorPersonaId, long userPersonaId, int assetGroupId, AssetGroupPatch assetGroup,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the company's portfolio or asset-group list, optionally including disabled entries,
    /// built as a hierarchy when the asset type is Portfolio.
    /// </summary>
    Task<ListResponse> GetCompanyAssetsAsync(
        long editorPersonaId, long userPersonaId, bool includeDisabled, RequestParameter datafilter,
        CancellationToken ct = default);

    // ── Roles and Rights ────────────────────────────────────────────────────

    /// <summary>
    /// Returns the Ops role list, optionally scoped to <paramref name="assetCode"/>,
    /// with the user's assigned role pre-selected for existing product users.
    /// </summary>
    Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long userPersonaId, string assetCode, RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>Returns the full Ops role list (all scopes) for count/management purposes.</summary>
    Task<ListResponse> GetRolesCountAsync(
        long editorPersonaId, string assetCode,
        CancellationToken ct = default);

    /// <summary>Returns all available Ops roles in context of a specific right.</summary>
    Task<ListResponse> GetRolesForRightAsync(
        long editorPersonaId, int rightId,
        CancellationToken ct = default);

    /// <summary>Returns the Ops rights hierarchy for the company.</summary>
    Task<ListResponse> GetRightsAsync(
        long editorPersonaId,
        CancellationToken ct = default);

    /// <summary>Returns the rights assigned to a specific Ops role.</summary>
    Task<ListResponse> GetRightsByRoleAsync(
        long editorPersonaId, long roleId,
        CancellationToken ct = default);

    /// <summary>
    /// Creates a new Ops role (<paramref name="roleId"/> == 0) or replaces an
    /// existing one (<paramref name="roleId"/> &gt; 0), including its rights assignment.
    /// </summary>
    Task<ListResponse> CreateRoleAsync(
        long editorPersonaId, OpsInput rightInput, long roleId,
        CancellationToken ct = default);

    // ── User Management ─────────────────────────────────────────────────────

    /// <summary>
    /// Creates or updates an Ops user for <paramref name="userPersonaId"/>, assigning the
    /// specified role and property.
    /// <para>
    /// Returns <c>(string.Empty, additionalParameters)</c> on success, or
    /// <c>(errorMessage, [])</c> on failure. The <c>additionalParameters</c> list contains
    /// role/property change entries suitable for the audit activity log.
    /// </para>
    /// </summary>
    Task<(string error, List<AdditionalParameters> additionalParameters)> ManageOpsUserAsync(
        long editorPersonaId, long userPersonaId, List<int> roleList, List<int> propertyList,
        CancellationToken ct = default);

    /// <summary>
    /// Syncs the Ops user profile (name, email, employee ID, status) from the UnifiedLogin
    /// platform to the Ops system via a PATCH call.
    /// Returns an empty string on success, or an error message on failure.
    /// </summary>
    Task<string> UpdateOPSUserProfileAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken ct = default);

    /// <summary>
    /// Activates, deactivates, or marks a user as deleted in the Ops system.
    /// Returns an empty string on success, or an error message on failure.
    /// </summary>
    Task<string> EnableUserAsync(
        long editorPersonaId, long userPersonaId, bool isActive, bool deleteUser,
        CancellationToken ct = default);

    /// <summary>
    /// Sets the Ops user status to <c>inactive</c> and marks the product setting as Deleted.
    /// Returns an empty string on success, or an error message on failure.
    /// </summary>
    Task<string> UnassignUserAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns a paged list of Ops users, filtered by <c>datafilter.FilterBy</c> key-value pairs.
    /// </summary>
    Task<ListResponse> GetUsersAsync(
        long editorPersonaId, RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Activates or deactivates the Ops user identified by <paramref name="productUserId"/>.
    /// Returns <c>true</c> on success, <c>false</c> on any error.
    /// </summary>
    Task<bool> ChangeUserStatusAsync(
        long editorPersonaId, string userName, string productUserId, bool isActive = false,
        CancellationToken ct = default);

    // ── Migration ───────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a paged list of Ops users filtered by unified-login migration status
    /// via <c>datafilter.FilterBy["filter"]</c> ("migrated" / other / "all").
    /// </summary>
    Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Updates the unified-login migration status for each user in
    /// <paramref name="migrateUsers"/> via a PATCH call to the Ops API.
    /// </summary>
    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers,
        CancellationToken ct = default);
}
