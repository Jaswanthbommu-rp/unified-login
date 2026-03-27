using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async implementation of postal address management operations.
/// Replaces the stepping-stone wrapper that delegated to the sync <c>IManagePostalAddress</c>.
/// </summary>
public sealed class ManagePostalAddressAsync : IManagePostalAddressAsync
{
    private readonly IPostalAddressRepositoryAsync _postalAddressRepository;

    public ManagePostalAddressAsync(IPostalAddressRepositoryAsync postalAddressRepository)
    {
        ArgumentNullException.ThrowIfNull(postalAddressRepository);
        _postalAddressRepository = postalAddressRepository;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="realPageId"/> is <see cref="Guid.Empty"/>,
    /// preserving the guard from the legacy <c>ManagePostalAddress.ListPostalAddressForPerson</c>.
    /// </exception>
    public async Task<IList<PostalAddress>> ListPostalAddressForPersonAsync(
        Guid realPageId,
        string contactMechanismUsageTypeName,
        CancellationToken cancellationToken = default)
    {
        if (realPageId == Guid.Empty)
            throw new ArgumentException("Invalid parameter realPageId.", nameof(realPageId));

        return await _postalAddressRepository
            .ListPostalAddressForPersonAsync(realPageId, contactMechanismUsageTypeName, cancellationToken);
    }
}
