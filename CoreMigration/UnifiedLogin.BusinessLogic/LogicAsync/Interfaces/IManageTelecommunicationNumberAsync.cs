using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for telecommunication number management.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManageTelecommunicationNumber"/>.
/// </summary>
public interface IManageTelecommunicationNumberAsync
{
    /// <summary>Links a telecommunication number to a person.</summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="telecommunicationNumber"/> is null.</exception>
    Task<RepositoryResponse> CreateTelecommunicationNumberAsync(
        ITelecommunicationNumber telecommunicationNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all telecommunication numbers for the person, optionally filtered by
    /// <paramref name="contactMechanismUsageTypeName"/>, each enriched with its usage type.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="realPageId"/> is <see cref="Guid.Empty"/>.</exception>
    Task<IList<TelecommunicationNumber>> ListTelecommunicationNumberForPersonAsync(
        Guid realPageId,
        string? contactMechanismUsageTypeName = null,
        CancellationToken cancellationToken = default);
}