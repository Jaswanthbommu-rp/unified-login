using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

/// <summary>
/// Async data-access interface for street address records.
/// Replaces: sync <see cref="IStreetAddressRepository"/>.
/// </summary>
public interface IStreetAddressRepositoryAsync
{
    /// <summary>Creates the street address for a person.</summary>
    Task<RepositoryResponse> CreateStreetAddressAsync(
        IStreetAddress streetAddress,
        CancellationToken cancellationToken = default);
}