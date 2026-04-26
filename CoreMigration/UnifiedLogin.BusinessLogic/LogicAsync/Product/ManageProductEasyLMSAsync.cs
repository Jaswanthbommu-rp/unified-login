using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product;

/// <summary>
/// Stepping-stone async wrapper for EasyLMS product operations.
/// Encapsulates the legacy <see cref="ManageProductEasyLMS"/> per-call construction
/// pattern (the class requires <see cref="DefaultUserClaim"/> at construction time) behind a
/// mockable async interface.
/// </summary>
public sealed class ManageProductEasyLMSAsync : IManageProductEasyLMSAsync
{
    public Task<CustomerCompanyMap> GetCompanyAPICodeAndKeyAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductEasyLMS(userClaim).GetCompanyAPICodeAndKey(editorPersonaId, userPersonaId));

    public Task<string> GetProductNameAsync(DefaultUserClaim userClaim, ProductEnum product, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductEasyLMS(userClaim).getProductName(product));
}
