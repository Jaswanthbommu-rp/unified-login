using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

public interface IManagePreferredContactMethodAsync
{
    Task<IList<PreferredContactMethod>> ListPreferredContactMethodAsync(
        CancellationToken cancellationToken = default);
}
