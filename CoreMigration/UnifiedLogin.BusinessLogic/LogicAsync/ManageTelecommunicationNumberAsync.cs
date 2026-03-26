using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first implementation of <see cref="IManageTelecommunicationNumberAsync"/>.
/// All I/O is awaited via <see cref="ITelecommunicationNumberRepositoryAsync"/>.
/// No <c>new</c> keyword; no parameterless constructor.
/// </summary>
public sealed class ManageTelecommunicationNumberAsync : IManageTelecommunicationNumberAsync
{
    private readonly ITelecommunicationNumberRepositoryAsync _repository;

    public ManageTelecommunicationNumberAsync(ITelecommunicationNumberRepositoryAsync repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc/>
    public Task<RepositoryResponse> CreateTelecommunicationNumberAsync(
        ITelecommunicationNumber telecommunicationNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(telecommunicationNumber);

        return _repository.CreateTelecommunicationNumberAsync(telecommunicationNumber, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IList<TelecommunicationNumber>> ListTelecommunicationNumberForPersonAsync(
        Guid realPageId,
        string? contactMechanismUsageTypeName = null,
        CancellationToken cancellationToken = default)
    {
        if (realPageId == Guid.Empty)
            throw new ArgumentException("Invalid parameter realPageId.", nameof(realPageId));

        return _repository.ListTelecommunicationNumberForPersonAsync(
            realPageId, contactMechanismUsageTypeName, cancellationToken);
    }
}