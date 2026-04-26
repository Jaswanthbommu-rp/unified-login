using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Native-async contract for product-specific integration implementations.
/// Replaces <see cref="UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory.IManageProductIntegration"/>.
/// <para>
/// <list type="bullet">
///   <item><c>DefaultUserClaim</c> removed — caller context from <c>IUserClaimsAccessor</c>.</item>
///   <item><c>out List&lt;AdditionalParameters&gt;</c> in <c>CreateUpdateProductUser</c> replaced
///     with named-tuple return.</item>
///   <item>All nullable <c>string baseUrlAndQuery = null</c> params annotated as
///     <c>string? baseUrlAndQuery = null</c> for NRT compliance.</item>
///   <item><c>CancellationToken ct = default</c> appended to every method.</item>
/// </list>
/// </para>
/// </summary>
public interface IManageProductIntegrationAsync
{
    // ── Roles & Rights ────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the role list for the product, optionally scoped to
    /// <paramref name="baseUrlAndQuery"/>.
    /// </summary>
    Task<ListResponse> GetProductRolesAsync(
        RequestParameter dataFilter,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default);

    /// <summary>
    /// Returns rights assigned to <paramref name="roleId"/> for the product.
    /// Role IDs are strings across all product integrations (products such as DIQ
    /// and ClickPay expose them as strings natively).
    /// </summary>
    Task<ListResponse> GetProductRightsForRoleAsync(
        RequestParameter dataFilter,
        string roleId,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default);

    /// <summary>Returns all rights available for the editor's company on this product.</summary>
    Task<ListResponse> GetAllRightsAsync(
        RequestParameter dataFilter,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default);

    // ── Properties & Groups ───────────────────────────────────────────────────

    /// <summary>Returns the property list for the product.</summary>
    Task<ListResponse> GetProductPropertiesAsync(
        RequestParameter dataFilter,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default);

    /// <summary>Returns property groups (regions) for the product.</summary>
    Task<ListResponse> GetProductPropertyGroupsAsync(
        RequestParameter dataFilter,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default);

    /// <summary>Returns properties that belong to <paramref name="groupId"/>.</summary>
    Task<ListResponse> GetProductPropertiesByGroupAsync(
        string groupId,
        RequestParameter dataFilter,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default);

    // ── Organizations ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns organizations for the product, optionally filtered by role and type.
    /// Used by products such as ClickPay that expose an organizations API.
    /// </summary>
    Task<ListResponse> GetProductOrganizationsAsync(
        string organizationRoleId,
        string organizationType,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default);

    // ── User Groups ───────────────────────────────────────────────────────────

    /// <summary>Returns user groups for the product.</summary>
    Task<ListResponse> GetProductUserGroupsAsync(
        RequestParameter dataFilter,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default);

    // ── User Read ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the current product user record.
    /// When <paramref name="isThrowOnError"/> is <c>false</c>, returns <c>null</c>
    /// instead of throwing on a 404 / product API error.
    /// </summary>
    Task<IntegrationProductUser?> GetProductUserAsync(
        string? baseUrlAndQuery = null,
        bool isThrowOnError = true,
        CancellationToken ct = default);

    // ── User Create / Update ──────────────────────────────────────────────────

    /// <summary>
    /// Creates or updates the product user based on
    /// <see cref="ProductUserRolePropertiesGroups.IsAssigned"/>.
    /// <para>
    /// Replaces <c>CreateUpdateProductUser(…, out List&lt;AdditionalParameters&gt;, …)</c>
    /// — <c>out</c> is incompatible with <c>async</c>.
    /// </para>
    /// </summary>
    /// <returns>
    /// <c>result</c> — empty string on success, error message otherwise.<br/>
    /// <c>auditParams</c> — audit log entries generated during the operation.
    /// </returns>
    Task<(string result, List<AdditionalParameters> auditParams)> CreateUpdateProductUserAsync(
        ProductUserRolePropertiesGroups productUserRolePropertiesGroups,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        CancellationToken ct = default);

    /// <summary>
    /// Changes the product user type (regular ↔ admin / superuser).
    /// Returns empty string on success.
    /// </summary>
    Task<string> ChangeProductUserTypeAsync(
        ProductUserRolePropertiesGroups rolePropertiesGroups,
        BatchProcessType batchProcessType,
        CancellationToken ct = default);

    /// <summary>
    /// Updates the product user's profile fields (name, email, phone).
    /// Returns empty string on success.
    /// </summary>
    Task<string> UpdateProductUserProfileAsync(CancellationToken ct = default);

    // ── User Remove ───────────────────────────────────────────────────────────

    /// <summary>Disables the product user account. Returns empty string on success.</summary>
    Task<string> UnassignUserAsync(CancellationToken ct = default);

    // ── Migration ─────────────────────────────────────────────────────────────

    /// <summary>Returns the migration user list for the current product.</summary>
    Task<ListResponse> GetMigrationUsersAsync(
        RequestParameter dataFilter,
        CancellationToken ct = default);

    /// <summary>Submits a batch migration-status update to the product API.</summary>
    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        IList<MigrateUser> migrateUsers,
        CancellationToken ct = default);

    // ── External Profile Sync ─────────────────────────────────────────────────

    /// <summary>
    /// Directly calls the product API to apply a profile change
    /// (activate/deactivate, name change). Used by the Migration Tool.
    /// Returns <c>true</c> on success.
    /// </summary>
    Task<bool> ExternalProductUserProfileChangeAsync(
        ProductUserProfile productUserProfile,
        CancellationToken ct = default);
}
