using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for postal address management operations.
/// </summary>
public interface IManagePostalAddressAsync
{
    Task<IList<PostalAddress>> ListPostalAddressForPersonAsync(Guid realPageId, string contactMechanismUsageTypeName, CancellationToken cancellationToken = default);
}
