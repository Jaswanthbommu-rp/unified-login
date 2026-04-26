using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Accounting;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// True async interface for OneSite Accounting per-call user-management operations.
/// <para>
/// <see cref="DefaultUserClaim"/> has been removed from all signatures — per-call context is
/// resolved internally via <see cref="IProductContextServiceAsync"/> from
/// <paramref name="editorPersonaId"/> / <paramref name="userPersonaId"/>.
/// </para>
/// </summary>
public interface IManageProductOneSiteAccountingAsync
{
    /// <summary>
    /// Activates or deactivates the Accounting user identified by <paramref name="userPersonaId"/>.
    /// Returns an empty string on success or an error/status message on failure.
    /// </summary>
    Task<string> ChangeStatusAccountingUserAsync(
        long editorPersonaId,
        long userPersonaId,
        bool isActive,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the Accounting user's claim-linked status (federated SSO link).
    /// Returns <c>true</c> on success.
    /// </summary>
    Task<bool> ChangeAccountingUserClaimStatusAsync(
        long editorPersonaId,
        long userPersonaId,
        bool isLinked,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Disables the Accounting user identified by their product login name (<paramref name="userName"/>).
    /// Returns <c>true</c> on success.
    /// </summary>
    Task<bool> ChangeUserStatusAsync(
        long editorPersonaId,
        string userName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paged list of Accounting users for migration purposes,
    /// filtered by <c>datafilter.FilterBy["filter"]</c> ("NONMIGRATED" / other).
    /// </summary>
    Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId,
        RequestParameter datafilter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits a batch migration-status update to the Accounting SOAP API,
    /// enabling or disabling the GreenBook (unified-login) flag per user.
    /// </summary>
    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId,
        IList<MigrateUser> migrateUsers,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the full Accounting application-permissions (role) list for the company.
    /// </summary>
    Task<ListResponse> GetRolesCountAsync(
        long editorPersonaId,
        RequestParameter datafilter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the rights assigned to a specific Accounting role identified by
    /// <paramref name="roleName"/> / <paramref name="roleId"/>.
    /// </summary>
    Task<ListResponse> GetRightsForRoleAsync(
        long editorPersonaId,
        RequestParameter datafilter,
        string roleName,
        int roleId,
        CancellationToken cancellationToken = default);

    // ── Properties ───────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the Accounting properties assigned (and available) for the given user,
    /// falling back to the full company property list when none are directly assigned.
    /// </summary>
    Task<ListResponse> GetUserPropertiesAsync(
        long editorPersonaId,
        long userPersonaId,
        RequestParameter datafilter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the Accounting property groups (location groups) for the given user.
    /// </summary>
    Task<ListResponse> GetUserPropertyGroupsAsync(
        long editorPersonaId,
        long userPersonaId,
        RequestParameter datafilter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the entities (properties) that belong to the given location group
    /// identified by <paramref name="locationGrpId"/>.
    /// </summary>
    Task<ListResponse> GetPropertyGroupEntitiesAsync(
        long editorPersonaId,
        long userPersonaId,
        string locationGrpId,
        RequestParameter datafilter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the Accounting entities list (the "Entities" tab),
    /// sourced from all company properties and filtered to non-empty entries.
    /// </summary>
    Task<ListResponse> GetUserPropertiesNewAsync(
        long editorPersonaId,
        long userPersonaId,
        RequestParameter datafilter,
        CancellationToken cancellationToken = default);

    // ── Companies / User detail ───────────────────────────────────────────────

    /// <summary>
    /// Returns the companies associated with the given user together with
    /// an <see cref="AccountingUser"/> aggregate (admin flag, MConsole flag, etc.)
    /// stored in <c>ListResponse.Additional</c>.
    /// </summary>
    Task<ListResponse> GetUserCompaniesAsync(
        long editorPersonaId,
        long userPersonaId,
        RequestParameter datafilter,
        CancellationToken cancellationToken = default);

    // ── Roles ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the Accounting roles (assigned and available) for the given user.
    /// </summary>
    Task<ListResponse> GetUserRolesAsync(
        long editorPersonaId,
        long userPersonaId,
        RequestParameter datafilter,
        CancellationToken cancellationToken = default);

    // ── Assignment helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Assigns all current companies to the given user (used for
    /// "Allow access to all current and future companies" toggle).
    /// Returns empty string on success, an error message on failure, together with audit entries.
    /// </summary>
    Task<(string error, List<AdditionalParameters> auditParams)> AssignAllCurrentCompaniesToUserAsync(
        long editorPersonaId,
        long userPersonaId,
        List<string> propertiesToAssign,
        bool isAccountingAdmin,
        BatchProcessType batchProcessType,
        List<ACProperty>? beforeUpdatePropertiesList = null,
        List<ProductPropertyGroup>? beforeUpdateLocationGrpList = null,
        List<ACProperty>? beforeUpdateEntitiesList = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes the property delta and assigns/removes Accounting properties for the given user.
    /// Returns empty string on success, an error message on failure, together with audit entries.
    /// </summary>
    Task<(string error, List<AdditionalParameters> auditParams)> UpdatePropertiesToUserAsync(
        long editorPersonaId,
        long userPersonaId,
        List<string> propertiesToAssign,
        bool isAccountingAdmin,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        bool isUnRestrictedAccessToProp = false,
        List<ACProperty>? beforeUpdatePropertiesList = null,
        List<ProductPropertyGroup>? beforeUpdateLocationGrpList = null,
        List<ACProperty>? beforeUpdateEntitiesList = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes the role delta and assigns/removes Accounting roles for the given user.
    /// Returns empty string on success, an error message on failure, together with audit entries.
    /// </summary>
    Task<(string error, List<AdditionalParameters> auditParams)> UpdateRolesToUserAsync(
        long editorPersonaId,
        long userPersonaId,
        List<string> rolesToAssign,
        bool isAccountingAdmin,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        ListResponse? currentRolesList = null,
        CancellationToken cancellationToken = default);

    // ── User lifecycle ────────────────────────────────────────────────────────

    /// <summary>
    /// Creates or updates an Accounting user and optionally assigns roles and properties.
    /// Returns empty string on success or an error message on failure, together with audit entries.
    /// </summary>
    Task<(string error, List<AdditionalParameters> auditParams)> ManageAccountingUserAsync(
        long editorPersonaId,
        long userPersonaId,
        List<string> roleList,
        List<string> propertyList,
        List<string> companyList,
        bool isAccountingAdmin,
        bool isSiteSpendManagementUser,
        bool isUnRestrictedAccessToProp,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the Accounting user's profile (name, email, supervisor) without changing roles or properties.
    /// Returns empty string on success or an error message on failure.
    /// </summary>
    Task<string> UpdateAccountingUserProfileAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the Accounting user type (e.g. Regular → Admin) by delegating to
    /// <see cref="ManageAccountingUserAsync"/> with the appropriate <paramref name="batchProcessType"/>.
    /// Returns empty string on success or an error message on failure, together with audit entries.
    /// </summary>
    Task<(string error, List<AdditionalParameters> auditParams)> ChangeAccountingServiceUserTypeAsync(
        long editorPersonaId,
        long userPersonaId,
        List<string> roleList,
        List<string> propertyList,
        List<string> companyList,
        bool isAccountingAdmin,
        bool isSiteSpendManagementUser,
        bool isUnRestrictedAccessToProp,
        BatchProcessType batchProcessType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Disables the Accounting user and marks their product status as Deleted.
    /// Returns empty string on success or an error message on failure.
    /// </summary>
    Task<string> UnassignUserAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently deletes the Accounting user and removes their product status record.
    /// Returns empty string on success or an error message on failure.
    /// </summary>
    Task<string> DeleteAccountingUserAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken cancellationToken = default);
}
