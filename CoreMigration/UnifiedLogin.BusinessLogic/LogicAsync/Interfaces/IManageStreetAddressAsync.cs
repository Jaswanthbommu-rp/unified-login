using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for street address management operations.
/// </summary>
public interface IManageStreetAddressAsync
{
    Task<RepositoryResponse> CreateStreetAddressAsync(IStreetAddress streetAddress, CancellationToken cancellationToken = default);
}
