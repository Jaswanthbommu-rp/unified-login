using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for person management operations.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManagePerson"/>.
/// </summary>
public interface IManagePersonAsync
{
    Task<Person> GetPersonAsync(Guid realPageId, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> CreatePersonAsync(IPerson person, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> UpdatePersonAsync(Guid realPageId, IPerson person, CancellationToken cancellationToken = default);
}
