using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

public interface IUserLoginRepositoryAsync
{
    Task<List<Organization>> ListOrganizationByRealPageIdAsync(Guid realPageId, CancellationToken token);
}

