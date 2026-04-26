using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// True async interface for On-Site (1S) REST API product user-management operations.
/// <para>
/// <see cref="DefaultUserClaim"/> has been removed from all signatures — per-call context is
/// resolved internally via <see cref="IProductContextServiceAsync"/> from
/// <paramref name="editorPersonaId"/> / <paramref name="userPersonaId"/>.
/// </para>
/// </summary>
public interface IManageProductOnSiteAsync
{
    // ── Catalog reads ──────────────────────────────────────────────────────────

    /// <summary>
    /// Returns active On-Site properties for the company, with <c>IsAssigned</c> flags set
    /// when <paramref name="userPersonaId"/> has an existing On-Site account.
    /// For new users (<paramref name="userPersonaId"/> = 0) returns the undecorated company list.
    /// </summary>
    Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Returns On-Site regions for the company, with <c>IsAssigned</c> flags set
    /// when <paramref name="userPersonaId"/> has an existing On-Site account.
    /// </summary>
    Task<ListResponse> GetRegionsAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Returns On-Site roles (access groups) for the company, with <c>IsAssigned</c> flags set
    /// when <paramref name="userPersonaId"/> has an existing On-Site account.
    /// </summary>
    Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    // ── User management ────────────────────────────────────────────────────────

    /// <summary>
    /// Deactivates the On-Site user identified by <paramref name="userPersonaId"/>
    /// and marks their product status as <c>Deleted</c> in Unified Login.
    /// Returns an empty string on success or an error message on failure.
    /// </summary>
    Task<string> UnassignUserAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken ct = default);

    /// <summary>
    /// Creates or updates an On-Site user and assigns the specified properties, regions, and roles.
    /// Returns an empty string on success or an error message on failure,
    /// together with structured audit entries for role/property changes.
    /// <para>
    /// Replaces <c>ManageOnSiteUser(out additionalParameters)</c> — the <c>out</c> parameter
    /// is replaced by a tuple return so the method is awaitable.
    /// </para>
    /// </summary>
    Task<(string error, List<AdditionalParameters> auditParams)> ManageOnSiteUserAsync(
        long editorPersonaId, long userPersonaId,
        List<int> propertyList, List<int> regionList, List<int> roleList,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        CancellationToken ct = default);

    /// <summary>
    /// Changes the On-Site user type (e.g. Regular → Admin) by delegating to
    /// <see cref="ManageOnSiteUserAsync"/> with the specified <paramref name="batchProcessType"/>.
    /// </summary>
    Task<(string error, List<AdditionalParameters> auditParams)> ChangeOnSiteServiceUserTypeAsync(
        long editorPersonaId, long userPersonaId,
        List<int> propertyList, List<int> regionList, List<int> roleList,
        BatchProcessType batchProcessType,
        CancellationToken ct = default);

    /// <summary>
    /// Updates the On-Site user's profile (name, email) without changing roles or properties.
    /// Returns an empty string on success or an error message on failure.
    /// </summary>
    Task<string> UpdateOnSiteUserProfileAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken ct = default);

    // ── User listing / status ──────────────────────────────────────────────────

    /// <summary>Returns all On-Site users for the editor's company.</summary>
    Task<ListResponse> GetUsersAsync(
        long editorPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Activates or deactivates the On-Site user identified by <paramref name="productUserId"/>.
    /// Returns <c>true</c> on success.
    /// </summary>
    Task<bool> ChangeUserStatusAsync(
        long editorPersonaId, string productUserId,
        bool isDeactivate = true,
        CancellationToken ct = default);

    // ── Migration tool ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a paged list of On-Site users for the migration tool,
    /// filtered by <c>datafilter.FilterBy["filter"]</c> (default: <c>"UnMigrated"</c>).
    /// </summary>
    Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Submits migrate/unmigrate status updates to the On-Site REST API
    /// for the supplied list of users.
    /// </summary>
    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId,
        IList<MigrateUser> migrateUsers,
        CancellationToken ct = default);
}
