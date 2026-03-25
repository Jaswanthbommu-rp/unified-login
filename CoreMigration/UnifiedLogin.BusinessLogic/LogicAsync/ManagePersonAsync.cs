using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for person management operations.
/// Delegates to the existing sync <see cref="IManagePerson"/> via <see cref="Task.FromResult{TResult}"/>.
/// </summary>
public sealed class ManagePersonAsync : IManagePersonAsync
{
    private readonly IManagePerson _managePerson;

    public ManagePersonAsync(IManagePerson managePerson)
    {
        _managePerson = managePerson ?? throw new ArgumentNullException(nameof(managePerson));
    }

    public Task<Person> GetPersonAsync(Guid realPageId, CancellationToken cancellationToken = default)
        => Task.FromResult(_managePerson.GetPerson(realPageId));

    public Task<RepositoryResponse> CreatePersonAsync(IPerson person, CancellationToken cancellationToken = default)
        => Task.FromResult(_managePerson.CreatePerson(person));

    public Task<RepositoryResponse> UpdatePersonAsync(Guid realPageId, IPerson person, CancellationToken cancellationToken = default)
        => Task.FromResult(_managePerson.UpdatePerson(realPageId, person));
}
