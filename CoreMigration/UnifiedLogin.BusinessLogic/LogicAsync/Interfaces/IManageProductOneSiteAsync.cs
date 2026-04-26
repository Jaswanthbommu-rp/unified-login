using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.OneSite;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// True async interface for OneSite per-call user-management operations.
/// <para>
/// <see cref="DefaultUserClaim"/> has been removed from all signatures — per-call context is
/// resolved internally via <see cref="IProductContextServiceAsync"/> from
/// <paramref name="editorPersonaId"/> / <paramref name="userPersonaId"/>.
/// </para>
/// </summary>
public interface IManageProductOneSiteAsync
{
    /// <summary>
    /// Returns the list of OneSite roles for the given user persona.
    /// When the user has no OneSite login yet, the full company role list is returned
    /// with <c>IsAssigned = false</c> on every entry.
    /// </summary>
    Task<ListResponse> GetRolesAsync(
        long editorPersonaId,
        long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the list of OneSite properties for the given user persona.
    /// When <paramref name="userPersonaId"/> is zero, the editor's property context is used.
    /// </summary>
    Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId,
        long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the full company-scoped OneSite role list (used by the "Regions" UI tab).
    /// </summary>
    Task<ListResponse> GetRegionsAsync(
        long editorPersonaId,
        long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Returns migration user records from the OneSite Management Tool API
    /// for the company associated with <paramref name="editorPersonaId"/>.
    /// </summary>
    Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Submits a batch migration-status update to the OneSite Management Tool API.
    /// </summary>
    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId,
        IList<MigrateUser> migrateUsers,
        CancellationToken ct = default);

    /// <summary>
    /// Disables the OneSite user account identified by their OneSite login name (<paramref name="userId"/>).
    /// Returns <c>true</c> on success.
    /// </summary>
    Task<bool> ChangeUserStatusAsync(
        long editorPersonaId,
        string userId,
        CancellationToken ct = default);

    /// <summary>
    /// Creates or updates an OneSite user (regular or superuser) and optionally
    /// assigns roles and properties in a single atomic call.
    /// Returns an empty string on success or an error message on failure,
    /// together with the audit log entries for the operation.
    /// </summary>
    Task<(string error, List<AdditionalParameters> auditParams)> ManageOneSiteUserAsync(
        long editorPersonaId,
        long userPersonaId,
        List<string> roleList,
        List<string> propertyList,
        bool isUserProfileChanged = false,
        CancellationToken ct = default);

    // ── Cross-product helpers (used by Lead2Lease) ───────────────────────────

    /// <summary>
    /// Returns the OneSite user record for the given product system identifier.
    /// Used by Lead2Lease to resolve the OneSite <c>UserId</c> when a property
    /// has a <c>PMSystemID</c> that maps to a OneSite site.
    /// </summary>
    Task<OneSiteUser?> GetOneSiteUserInfoAsync(
        string systemIdentifier,
        CancellationToken ct = default);

    /// <summary>
    /// Returns <c>true</c> if the user identified by <paramref name="systemIdentifier"/>
    /// has the Leasing Consultant right for the given <paramref name="siteId"/>.
    /// Used by Lead2Lease to decide whether to populate OneSite PMUser fields.
    /// </summary>
    Task<bool> UserInLeasingAgentListAsync(
        string systemIdentifier,
        int siteId,
        CancellationToken ct = default);

    // ── Property / Role / User list helpers ─────────────────────────────────────

    /// <summary>Returns the list of OneSite users associated with the given property.</summary>
    Task<ListResponse> GetUsersForPropertyAsync(
        long editorPersonaId, int propertyId, bool assignedOnly,
        RequestParameter datafilter, CancellationToken ct = default);

    /// <summary>Returns the list of OneSite users associated with the given role.</summary>
    Task<ListResponse> GetUsersForRoleAsync(
        long editorPersonaId, int roleId, bool assignedOnly,
        RequestParameter datafilter, CancellationToken ct = default);

    // ── Rights ────────────────────────────────────────────────────────────────────

    /// <summary>Returns the list of OneSite rights centers available for the editor's PMC.</summary>
    Task<ListResponse> GetRightsCentersAsync(
        long editorPersonaId, CancellationToken ct = default);

    /// <summary>Returns the OneSite rights list, optionally filtered by role assignment.</summary>
    Task<ListResponse> GetRightsAsync(
        long editorPersonaId, RequestParameter datafilter,
        long roleId = 0, bool assignedToRoleOnly = false,
        CancellationToken ct = default);

    /// <summary>Returns the list of OneSite roles associated with the given right.</summary>
    Task<ListResponse> GetRolesForRightAsync(
        long editorPersonaId, int rightId, bool assignedOnly,
        RequestParameter datafilter, CancellationToken ct = default);

    /// <summary>Assigns or removes a right to/from the given list of roles.</summary>
    Task<string> UpdateRightToRolesAsync(
        long editorPersonaId, int rightId, List<string> roles,
        bool assignRight, CancellationToken ct = default);

    /// <summary>Adds and/or removes rights from the given role in a single call.</summary>
    Task<string> UpdateRoleToRightsAsync(
        long editorPersonaId, int roleId,
        List<string> rightsToAdd, List<string> rightsToRemove,
        CancellationToken ct = default);

    // ── Role admin ────────────────────────────────────────────────────────────────

    /// <summary>Creates a new OneSite role or updates an existing one by role ID.</summary>
    Task<ListResponse> AddUpdateRoleAsync(
        long editorPersonaId, int roleId, string roleName, string inheritRoleId,
        CancellationToken ct = default);

    /// <summary>Deletes the OneSite role identified by <paramref name="roleId"/>.</summary>
    Task<string> DeleteRoleAsync(
        long editorPersonaId, int roleId, CancellationToken ct = default);

    // ── User management ───────────────────────────────────────────────────────────

    /// <summary>Disables the OneSite user account and optionally removes all SAML product info and status.</summary>
    Task<string> UnassignUserAsync(
        long editorPersonaId, long userPersonaId,
        bool deleteSamlUserProductInfoAndStatus = false,
        CancellationToken ct = default);

    /// <summary>Computes role delta and assigns/removes OneSite roles for the given user, returning audit entries.</summary>
    Task<(string resultCount, List<AdditionalParameters> auditParams)> UpdateRolesForUserAsync(
        long editorPersonaId, long userPersonaId,
        List<string> rolesToAssign, CancellationToken ct = default);

    /// <summary>Computes property delta and assigns/removes OneSite properties for the given user, returning audit entries.</summary>
    Task<(string resultCount, List<AdditionalParameters> auditParams)> UpdatePropertiesForUserAsync(
        long editorPersonaId, long userPersonaId,
        List<string> propertiesToAssign, CancellationToken ct = default);

    // ── SAML helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the PMC server info for a given <paramref name="pmcId"/>.
    /// Result is cached per-PMC for 10 minutes.
    /// Used by <c>RealPageSamlAsync</c> to rewrite the OneSite SAML endpoint URL
    /// to the correct PMC hostname.
    /// </summary>
    Task<PMCInfo?> GetPmcInfoAsync(int pmcId, CancellationToken ct = default);
}
