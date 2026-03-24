using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

public interface IStreetAddressRepositoryAsync
{
    Task<RepositoryResponse> CreateStreetAddressAsync(IStreetAddress streetAddress, CancellationToken cancellationToken = default);
}