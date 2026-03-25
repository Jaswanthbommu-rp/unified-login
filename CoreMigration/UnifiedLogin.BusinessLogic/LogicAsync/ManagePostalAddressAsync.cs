using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for postal address management operations.
/// Delegates to the existing sync <see cref="IManagePostalAddress"/> via <see cref="Task.FromResult{TResult}"/>.
/// </summary>
public sealed class ManagePostalAddressAsync : IManagePostalAddressAsync
{
    private readonly IManagePostalAddress _managePostalAddress;

    public ManagePostalAddressAsync(IManagePostalAddress managePostalAddress)
    {
        _managePostalAddress = managePostalAddress ?? throw new ArgumentNullException(nameof(managePostalAddress));
    }

    public Task<IList<PostalAddress>> ListPostalAddressForPersonAsync(Guid realPageId, string contactMechanismUsageTypeName, CancellationToken cancellationToken = default)
        => Task.FromResult(_managePostalAddress.ListPostalAddressForPerson(realPageId, contactMechanismUsageTypeName));
}
