using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first implementation of <see cref="IManageElectronicAddressAsync"/>.
/// All I/O is awaited via <see cref="IElectronicAddressRepositoryAsync"/>.
/// No <c>new</c> keyword; no parameterless constructor.
/// </summary>
public sealed class ManageElectronicAddressAsync : IManageElectronicAddressAsync
{
    private readonly IElectronicAddressRepositoryAsync _repository;

    public ManageElectronicAddressAsync(IElectronicAddressRepositoryAsync repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc/>
    public Task<RepositoryResponse> CreateElectronicAddressAsync(
        IElectronicAddress electronicAddress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(electronicAddress);
        return _repository.CreateElectronicAddressAsync(electronicAddress, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IList<ElectronicAddress>> ListElectronicAddressForPersonAsync(
        Guid realPageId,
        string? contactMechanismUsageTypeName = null,
        CancellationToken cancellationToken = default)
    {
        if (realPageId == Guid.Empty)
            throw new ArgumentException("Invalid parameter realPageId.", nameof(realPageId));

        return _repository.ListElectronicAddressForPersonAsync(
            realPageId, contactMechanismUsageTypeName, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IList<ElectronicAddress>> ListElectronicAddressForPersonAsync(
        string loginName,
        long orgPartyId,
        string? contactMechanismUsageTypeName = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(loginName))
            throw new ArgumentException("Invalid parameter user login name.", nameof(loginName));

        return _repository.ListElectronicAddressForPersonAsync(
            loginName, orgPartyId, contactMechanismUsageTypeName, cancellationToken);
    }
}