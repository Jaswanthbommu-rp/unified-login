using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

public interface IUnifiedSettingsRepositoryAsync
{
    Task<IList<Setting>> GetUnifiedSettingsAsync(long partyId, string category, CancellationToken cancellationToken = default);
}