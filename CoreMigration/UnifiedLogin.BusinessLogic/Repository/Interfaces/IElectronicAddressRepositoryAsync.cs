using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

/// <summary>
/// Async data-access interface for electronic address records.
/// Replaces: sync <see cref="IElectronicAddressRepository"/>.
/// </summary>
public interface IElectronicAddressRepositoryAsync
{
    /// <summary>Links an electronic address to a person.</summary>
    Task<RepositoryResponse> CreateElectronicAddressAsync(
        IElectronicAddress electronicAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all electronic addresses for the person identified by <paramref name="realPageId"/>,
    /// each enriched with its <see cref="ContactMechanismUsageType"/>.
    /// </summary>
    Task<IList<ElectronicAddress>> ListElectronicAddressForPersonAsync(
        Guid realPageId,
        string? contactMechanismUsageTypeName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns electronic addresses for the person identified by <paramref name="loginName"/>
    /// within <paramref name="orgPartyId"/>, each enriched with its <see cref="ContactMechanismUsageType"/>.
    /// </summary>
    Task<IList<ElectronicAddress>> ListElectronicAddressForPersonAsync(
        string loginName,
        long orgPartyId,
        string? contactMechanismUsageTypeName = null,
        CancellationToken cancellationToken = default);
}