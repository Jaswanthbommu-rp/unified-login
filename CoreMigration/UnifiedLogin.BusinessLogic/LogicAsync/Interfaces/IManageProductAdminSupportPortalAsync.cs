using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for Admin Support Portal (Salesforce / Client Portal) product-management operations.
/// <para>
/// Replaces the stepping-stone wrapper that required a <c>DefaultUserClaim</c> on every call.
/// User context is now resolved internally via <see cref="IProductContextServiceAsync"/>,
/// so callers only supply the persona IDs they already have.
/// </para>
/// </summary>
public interface IManageProductAdminSupportPortalAsync
{
    /// <summary>
    /// Returns all available Admin Support Portal roles, with the currently-assigned role
    /// pre-selected when <paramref name="userPersonaId"/> belongs to an existing product user.
    /// </summary>
    Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long userPersonaId, RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the BlueBook property list for the editor's company, with the property currently
    /// assigned in Salesforce pre-selected when <paramref name="userPersonaId"/> belongs to an
    /// existing product user.
    /// </summary>
    Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId, long userPersonaId, RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Returns a paged list of Salesforce users for the editor's company, optionally filtered
    /// by migration status via <c>datafilter.FilterBy["filter"]</c> ("migrated" / other).
    /// </summary>
    Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Marks each user in <paramref name="migrateUsers"/> as migrated in both the Salesforce
    /// Contact and User objects.
    /// </summary>
    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers,
        CancellationToken ct = default);

    /// <summary>
    /// Disables the Salesforce user identified by <paramref name="productUserId"/>.
    /// Returns <c>true</c> on success, <c>false</c> on any error.
    /// </summary>
    Task<bool> ChangeUserStatusAsync(
        long editorPersonaId, string productUserId,
        CancellationToken ct = default);
}
