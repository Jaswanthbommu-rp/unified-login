using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first implementation of <see cref="IManageStreetAddressAsync"/>.
/// All I/O is awaited via <see cref="IStreetAddressRepositoryAsync"/>.
/// No <c>new</c> keyword; no parameterless constructor.
/// </summary>
public sealed class ManageStreetAddressAsync : IManageStreetAddressAsync
{
    private readonly IStreetAddressRepositoryAsync _repository;

    public ManageStreetAddressAsync(IStreetAddressRepositoryAsync repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc/>
    public Task<RepositoryResponse> CreateStreetAddressAsync(
        IStreetAddress streetAddress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(streetAddress);
        return _repository.CreateStreetAddressAsync(streetAddress, cancellationToken);
    }
}
