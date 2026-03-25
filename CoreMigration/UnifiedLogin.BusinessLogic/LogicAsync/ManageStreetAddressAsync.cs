using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for street address operations.
/// Delegates to the existing sync <see cref="IManageStreetAddress"/> via <see cref="Task.FromResult{TResult}"/>.
/// </summary>
public sealed class ManageStreetAddressAsync : IManageStreetAddressAsync
{
    private readonly IManageStreetAddress _manageStreetAddress;

    public ManageStreetAddressAsync(IManageStreetAddress manageStreetAddress)
    {
        _manageStreetAddress = manageStreetAddress ?? throw new ArgumentNullException(nameof(manageStreetAddress));
    }

    public Task<RepositoryResponse> CreateStreetAddressAsync(IStreetAddress streetAddress, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageStreetAddress.CreateStreetAddress(streetAddress));
}
