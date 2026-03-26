using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for electronic address management.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManageElectronicAddress"/>.
/// </summary>
public interface IManageElectronicAddressAsync
{
    /// <summary>Links an electronic address to a person.</summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="electronicAddress"/> is null.</exception>
    Task<RepositoryResponse> CreateElectronicAddressAsync(
        IElectronicAddress electronicAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all electronic addresses for the person, optionally filtered by
    /// <paramref name="contactMechanismUsageTypeName"/>, each enriched with its usage type.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="realPageId"/> is <see cref="Guid.Empty"/>.</exception>
    Task<IList<ElectronicAddress>> ListElectronicAddressForPersonAsync(
        Guid realPageId,
        string? contactMechanismUsageTypeName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns electronic addresses for the person identified by login name and organisation,
    /// each enriched with its usage type.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="loginName"/> is null or empty.</exception>
    Task<IList<ElectronicAddress>> ListElectronicAddressForPersonAsync(
        string loginName,
        long orgPartyId,
        string? contactMechanismUsageTypeName = null,
        CancellationToken cancellationToken = default);
}