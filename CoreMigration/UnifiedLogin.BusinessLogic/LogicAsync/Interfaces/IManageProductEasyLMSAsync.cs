using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for EasyLMS product operations.
/// DefaultUserClaim is passed per-call because ManageProductEasyLMS requires it at construction time.
/// </summary>
public interface IManageProductEasyLMSAsync
{
    Task<CustomerCompanyMap> GetCompanyAPICodeAndKeyAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default);

    Task<string> GetProductNameAsync(DefaultUserClaim userClaim, ProductEnum product, CancellationToken cancellationToken = default);
}
