using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for product-user per-call user-context operations.
/// Wraps legacy <see cref="UnifiedLogin.BusinessLogic.Logic.Product.ManageProductUser"/>
/// calls that require <see cref="DefaultUserClaim"/> at construction time.
/// </summary>
public interface IManageProductUserAsync
{
    Task<string> CreateProductUserAsync(DefaultUserClaim userClaim, ProductUserProperitiesRoles productUser, CancellationToken cancellationToken = default);

    Task<string> UpdateProductUserAccountDetailsAsync(DefaultUserClaim userClaim, ProductUserAccountDetails productUser, CancellationToken cancellationToken = default);

    Task<string> DeleteSamlUserProductInfoAndStatusAsync(DefaultUserClaim userClaim, ProductUserAccountDetails productUser, CancellationToken cancellationToken = default);

    Task<IList<ProductBatchStatus>> GetProductStatusesAsync(DefaultUserClaim userClaim, long assignUserPersonaId, CancellationToken cancellationToken = default);
}
