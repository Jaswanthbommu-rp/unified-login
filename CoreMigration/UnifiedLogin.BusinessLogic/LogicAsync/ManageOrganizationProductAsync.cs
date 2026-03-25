using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for organization–product relationship operations.
/// Delegates to the existing sync <see cref="IManageOrganizationProduct"/> via <see cref="Task.FromResult{TResult}"/>.
/// </summary>
public sealed class ManageOrganizationProductAsync : IManageOrganizationProductAsync
{
    private readonly IManageOrganizationProduct _manageOrganizationProduct;

    public ManageOrganizationProductAsync(IManageOrganizationProduct manageOrganizationProduct)
    {
        _manageOrganizationProduct = manageOrganizationProduct ?? throw new ArgumentNullException(nameof(manageOrganizationProduct));
    }

    public Task<RepositoryResponse> CheckSharedProductsEnabledAsync(IList<ProductUI> orgEnabledProductList, List<int> addProductList, List<int> removeProductList, CancellationToken cancellationToken = default)
        => Task.FromResult((RepositoryResponse)_manageOrganizationProduct.CheckSharedProductsEnabled(orgEnabledProductList, addProductList, removeProductList));

    public Task<IRepositoryResponse> InsertUpdateOrganizationProductAsync(Organization org, List<int> productList, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganizationProduct.InsertUpdateOrganizationProduct(org, productList));

    public Task<RepositoryResponse> DeleteProductsFromOrganizationAsync(List<int> unassignProductList, Organization org, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageOrganizationProduct.DeleteProductsFromOrganization(unassignProductList, org));
}
