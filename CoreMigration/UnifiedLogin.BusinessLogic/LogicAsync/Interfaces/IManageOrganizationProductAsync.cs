using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for organization–product relationship operations.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManageOrganizationProduct"/>.
/// </summary>
public interface IManageOrganizationProductAsync
{
    Task<RepositoryResponse> CheckSharedProductsEnabledAsync(IList<ProductUI> orgEnabledProductList, List<int> addProductList, List<int> removeProductList, CancellationToken cancellationToken = default);
    Task<IRepositoryResponse> InsertUpdateOrganizationProductAsync(Organization org, List<int> productList, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> DeleteProductsFromOrganizationAsync(List<int> unassignProductList, Organization org, CancellationToken cancellationToken = default);
}
