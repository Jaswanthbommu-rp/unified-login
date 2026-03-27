using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first implementation of <see cref="IManagePersonAsync"/>.
/// All I/O is awaited via <see cref="IPersonRepositoryAsync"/>.
/// No <c>new</c> keyword; no parameterless constructor.
/// </summary>
public sealed class ManagePersonAsync : IManagePersonAsync
{
    private readonly IPersonRepositoryAsync _repository;

    public ManagePersonAsync(IPersonRepositoryAsync repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc/>
    public Task<Person> GetPersonAsync(
        Guid realPageId,
        CancellationToken cancellationToken = default)
    {
        if (realPageId == Guid.Empty)
            throw new ArgumentException("Invalid parameter realPageId.", nameof(realPageId));

        return _repository.GetPersonAsync(realPageId, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<RepositoryResponse> CreatePersonAsync(
        IPerson person,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(person);
        return _repository.CreatePersonAsync(person, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<RepositoryResponse> UpdatePersonAsync(
        Guid realPageId,
        IPerson person,
        CancellationToken cancellationToken = default)
    {
        if (realPageId == Guid.Empty)
            throw new ArgumentException("Invalid parameter realPageId.", nameof(realPageId));
        ArgumentNullException.ThrowIfNull(person);

        return _repository.UpdatePersonAsync(realPageId, person, cancellationToken);
    }
}
