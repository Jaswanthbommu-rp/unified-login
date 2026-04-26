using System.Runtime.CompilerServices;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Native-async contract for integration-type orchestration.
/// Replaces <see cref="UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Types.IIntegrationType"/>.
/// <para>
/// <list type="bullet">
///   <item><c>DefaultUserClaim</c> removed from all signatures — caller context is resolved
///     internally via <c>IUserClaimsAccessor</c>.</item>
///   <item><c>out List&lt;AdditionalParameters&gt;</c> replaced with named-tuple return —
///     <c>out</c> params are incompatible with <c>async</c>.</item>
///   <item><c>CancellationToken ct = default</c> appended to every method.</item>
/// </list>
/// </para>
/// </summary>
public interface IIntegrationTypeAsync
{
    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>Returns the property list for <paramref name="userPersonaId"/>.</summary>
    Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId,
        long userPersonaId,
        RequestParameter dataFilter,
        CancellationToken ct = default);

    /// <summary>Returns the enterprise-scoped property list for <paramref name="userPersonaId"/>.</summary>
    Task<ListResponse> GetEnterprisePropertiesAsync(
        long userPersonaId,
        RequestParameter dataFilter,
        CancellationToken ct = default);

    /// <summary>Returns property groups, optionally filtered by <paramref name="userLoginName"/>.</summary>
    Task<ListResponse> GetPropertyGroupsAsync(
        long editorPersonaId,
        long userPersonaId,
        RequestParameter dataFilter,
        string userLoginName = "",
        CancellationToken ct = default);

    /// <summary>Returns properties that belong to <paramref name="propertyGroupId"/>.</summary>
    Task<ListResponse> GetPropertiesByGroupAsync(
        long editorPersonaId,
        long userPersonaId,
        string propertyGroupId,
        RequestParameter dataFilter,
        CancellationToken ct = default);

    // ── Organizations ─────────────────────────────────────────────────────────

    /// <summary>Returns organizations matching the given role and type filters.</summary>
    Task<ListResponse> GetOrganizationsAsync(
        long editorPersonaId,
        long userPersonaId,
        string organizationRoleId,
        string organizationType,
        CancellationToken ct = default);

    // ── Roles & Rights ────────────────────────────────────────────────────────

    /// <summary>Returns the role list for <paramref name="userPersonaId"/>.</summary>
    Task<ListResponse> GetRolesAsync(
        long editorPersonaId,
        long userPersonaId,
        long partyId,
        AccessType? accessType,
        RequestParameter dataFilter,
        CancellationToken ct = default);

    /// <summary>
    /// Returns rights for a role identified by a numeric <paramref name="roleId"/>.
    /// This overload is preferred by the compiler via
    /// <see cref="OverloadResolutionPriorityAttribute"/> when both numeric and string
    /// overloads are candidates.
    /// </summary>
    [OverloadResolutionPriority(1)]
    Task<ListResponse> GetRightsForRoleAsync(
        long editorPersonaId,
        long userPersonaId,
        long roleId,
        long partyId,
        bool assignedToRoleOnly,
        RequestParameter dataFilter,
        CancellationToken ct = default);

    /// <summary>
    /// Returns rights for a role identified by a string <paramref name="roleId"/>
    /// (used by products that expose role IDs as strings, e.g. DIQ).
    /// </summary>
    Task<ListResponse> GetRightsForRoleAsync(
        long editorPersonaId,
        long userPersonaId,
        string roleId,
        long partyId,
        bool assignedToRoleOnly,
        RequestParameter dataFilter,
        CancellationToken ct = default);

    /// <summary>Returns all rights available for the editor's company.</summary>
    Task<ListResponse> GetAllRightsAsync(
        long editorPersonaId,
        long userPersonaId,
        RequestParameter dataFilter,
        CancellationToken ct = default);

    // ── User Groups ───────────────────────────────────────────────────────────

    /// <summary>Returns user groups for the given persona and party.</summary>
    Task<ListResponse> GetUserGroupsAsync(
        long editorPersonaId,
        long userPersonaId,
        long partyId,
        RequestParameter dataFilter,
        CancellationToken ct = default);

    // ── User Creation & Mutation ──────────────────────────────────────────────

    /// <summary>
    /// Creates a product user and returns the operation result with any audit parameters.
    /// <para>
    /// Replaces <c>CreateUser(ProductUserProperitiesRoles, out List&lt;AdditionalParameters&gt;)</c>
    /// — <c>out</c> is incompatible with <c>async</c>.
    /// </para>
    /// </summary>
    /// <returns>
    /// <c>result</c> — empty string on success, error message otherwise.<br/>
    /// <c>auditParams</c> — audit log entries generated during creation.
    /// </returns>
    Task<(string result, List<AdditionalParameters> auditParams)> CreateUserAsync(
        ProductUserProperitiesRoles productUser,
        CancellationToken ct = default);

    /// <summary>Changes the user type (regular ↔ admin) for <paramref name="batchRecord"/>.</summary>
    Task<string> ChangeUserTypeAsync(
        ProductUserProperitiesRoles batchRecord,
        CancellationToken ct = default);

    /// <summary>Updates the product user's profile fields.</summary>
    Task<string> UpdateUserProfileAsync(
        ProductUserProperitiesRoles productUser,
        CancellationToken ct = default);

    /// <summary>
    /// Updates the user's account details (name, email, phone).
    /// <paramref name="internalChange"/> suppresses external product notifications when
    /// <c>true</c>.
    /// </summary>
    Task<string> UpdateUserDetailsAsync(
        ProductUserAccountDetails productUserAccountDetails,
        bool internalChange = false,
        CancellationToken ct = default);

    // ── Migration ─────────────────────────────────────────────────────────────

    /// <summary>Returns the migration user list for <paramref name="editorPersonaId"/>'s company.</summary>
    Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId,
        RequestParameter dataFilter,
        CancellationToken ct = default);

    /// <summary>Submits a batch migration-status update for <paramref name="migrateUsers"/>.</summary>
    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId,
        IList<MigrateUser> migrateUsers,
        CancellationToken ct = default);

    // ── External Profile Sync ─────────────────────────────────────────────────

    /// <summary>
    /// Pushes a profile change directly to the product (activate/deactivate, name change).
    /// Returns <c>true</c> on success.
    /// </summary>
    Task<bool> ExternalUserProfileChangeAsync(
        long editorPersonaId,
        ProductUserProfile productUserProfile,
        CancellationToken ct = default);
}
