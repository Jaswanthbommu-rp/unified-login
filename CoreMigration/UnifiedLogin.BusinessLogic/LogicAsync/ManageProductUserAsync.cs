using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for product-user per-call user-context operations.
/// Encapsulates the legacy <see cref="ManageProductUser"/> per-call construction
/// pattern behind a mockable async interface.
/// </summary>
public sealed class ManageProductUserAsync : IManageProductUserAsync
{
    public Task<string> CreateProductUserAsync(DefaultUserClaim userClaim, ProductUserProperitiesRoles productUser, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductUser(userClaim).CreateProductUser(productUser));

    public Task<string> UpdateProductUserAccountDetailsAsync(DefaultUserClaim userClaim, ProductUserAccountDetails productUser, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductUser(userClaim).UpdateProductUserAccountDetails(productUser, true));

    public Task<string> DeleteSamlUserProductInfoAndStatusAsync(DefaultUserClaim userClaim, ProductUserAccountDetails productUser, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductUser(userClaim).DeleteSamlUserProductInfoAndStatus(productUser, true));

    public Task<IList<ProductBatchStatus>> GetProductStatusesAsync(DefaultUserClaim userClaim, long assignUserPersonaId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductUser(userClaim).GetProductStatuses(userClaim.UserRealPageGuid, assignUserPersonaId));
}
