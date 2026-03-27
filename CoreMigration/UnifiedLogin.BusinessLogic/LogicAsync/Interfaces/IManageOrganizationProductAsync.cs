using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for organization–product relationship operations.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManageOrganizationProduct"/>.
/// </summary>
public interface IManageOrganizationProductAsync
{
    // ── Validation ────────────────────────────────────────────────────────

    /// <summary>
    /// Checks whether any product in <paramref name="addProductList"/> is blocked by a
    /// shared-product prevention rule against an already-enabled product.
    /// </summary>
    Task<RepositoryResponse> CheckSharedProductsEnabledAsync(
        IList<ProductUI> orgEnabledProductList,
        List<int> addProductList,
        List<int> removeProductList,
        CancellationToken cancellationToken = default);

    // ── Insert / Update ───────────────────────────────────────────────────

    /// <summary>
    /// Adds a list of products to the organisation, calling UDM for products that require it,
    /// and writes a single compound audit entry.
    /// </summary>
    Task<IRepositoryResponse> InsertUpdateOrganizationProductAsync(
        Organization org,
        List<int> productList,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Low-level: upserts a single product against the organisation party with no UDM or audit logging.
    /// </summary>
    Task<RepositoryResponse> InsertUpdateOrganizationProductAsync(
        long partyId,
        int product,
        int? configurationId,
        DateTime? fromDate,
        DateTime? thruDate,
        string orgName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts a single product originating from a provisioning event and logs the action.
    /// </summary>
    Task<IRepositoryResponse> InsertUpdateOrganizationProductFromProvisioningAsync(
        int product,
        int? configurationId,
        DateTime? fromDate,
        DateTime? thruDate,
        Organization org,
        CancellationToken cancellationToken = default);

    // ── Delete ────────────────────────────────────────────────────────────

    /// <summary>
    /// Removes a single product from the organisation, optionally logging the action.
    /// </summary>
    Task<RepositoryResponse> DeleteOrganizationProductAsync(
        long partyId,
        int product,
        Organization org,
        bool logActivity = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a list of products from the organisation, calling UDM for products that require it,
    /// and writes a single compound audit entry.
    /// </summary>
    Task<RepositoryResponse> DeleteProductsFromOrganizationAsync(
        List<int> unassignProductList,
        Organization org,
        CancellationToken cancellationToken = default);

    // ── Users ─────────────────────────────────────────────────────────────

    /// <summary>Disables all users assigned to the given product within the organisation.</summary>
    Task<RepositoryResponse> DisableUsersForProductAsync(
        long partyId,
        ProductEnum product,
        CancellationToken cancellationToken = default);
}
