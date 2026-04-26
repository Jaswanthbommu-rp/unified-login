using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for product-user operations.
/// Claims are resolved from <see cref="IUserClaimsAccessor"/> internally —
/// callers no longer pass <c>DefaultUserClaim</c> per method.
/// </summary>
public interface IManageProductUserAsync
{
    /// <summary>
    /// Creates or updates a product user batch record.
    /// Claims resolved from the current <see cref="IUserClaimsAccessor"/> context.
    /// </summary>
    Task<string> CreateProductUserAsync(
        ProductUserProperitiesRoles productUser,
        CancellationToken cancellationToken = default);

    /// <summary>Updates product-specific identifiers for a user.</summary>
    Task<string> UpdateProductUserAccountDetailsAsync(
        ProductUserAccountDetails productUser,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes all SAML product information and status for a user.</summary>
    Task<string> DeleteSamlUserProductInfoAndStatusAsync(
        ProductUserAccountDetails productUser,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the current batch statuses for a user across products.</summary>
    Task<IList<ProductBatchStatus>> GetProductStatusesAsync(
        long assignUserPersonaId,
        CancellationToken cancellationToken = default);

    // ── Batch-pipeline methods ─────────────────────────────────────────────
    // Called exclusively by the BatchProcessAsync process classes.
    // Previously on ManageProductUser and invoked via Activator.CreateInstance
    // inside ProcessExecutionFactory; now resolved through DI.

    /// <summary>
    /// Changes a product user's type (admin ↔ regular ↔ external) for the given batch record.
    /// Covers <c>UserTypeAdminToRegular</c>, <c>UserTypeRegularToAdmin</c>,
    /// <c>UserTypeAdminToExternal</c>, and <c>UserTypeExternalToAdmin</c> process types.
    /// </summary>
    Task<string> ChangeUserTypeAsync(
        ProductUserProperitiesRoles batchRecord,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates an enterprise-role product user for the given batch record.
    /// Covers the <c>EnterpriseRoleCreateUpdateProductUser</c> process type.
    /// </summary>
    Task<string> CreateEnterpriseRoleProductUserAsync(
        ProductUserProperitiesRoles batchRecord,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the product-specific user profile for the given batch record.
    /// Covers the <c>ProfileUpdate</c> process type.
    /// </summary>
    Task<string> UpdateProductUserProfileAsync(
        ProductUserProperitiesRoles batchRecord,
        CancellationToken cancellationToken = default);
}
