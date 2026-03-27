using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
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
}
