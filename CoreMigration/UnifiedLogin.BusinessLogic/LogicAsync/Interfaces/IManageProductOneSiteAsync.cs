using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

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
}
