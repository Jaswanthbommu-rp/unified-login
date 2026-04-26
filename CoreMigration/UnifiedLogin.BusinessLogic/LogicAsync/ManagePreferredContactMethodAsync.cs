using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

public sealed class ManagePreferredContactMethodAsync : IManagePreferredContactMethodAsync
{
    private readonly IPreferredContactMethodRepositoryAsync _repository;

    public ManagePreferredContactMethodAsync(IPreferredContactMethodRepositoryAsync repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<IList<PreferredContactMethod>> ListPreferredContactMethodAsync(
        CancellationToken cancellationToken = default)
        => _repository.ListPreferredContactMethodAsync(cancellationToken);
}
