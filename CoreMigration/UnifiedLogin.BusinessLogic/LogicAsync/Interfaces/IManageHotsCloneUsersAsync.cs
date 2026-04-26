using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Hots;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

public interface IManageHotsCloneUsersAsync
{
    Task<ClonedUsers> CloneUsersFromBaseLineCompanyAsync(
        CloneUsers cloneUsers,
        long basePartyId,
        long clonePartyId,
        long baseOrgAdminPersonaId,
        CancellationToken cancellationToken = default);

    Task<Guid> GetBaseCompanyUPFMIdAsync(
        Guid cloneUpfmId,
        CancellationToken cancellationToken = default);

    Task<RepositoryResponse> InsertHotsCompanyRelationshipAsync(
        Guid baselineCompanyRealPageId,
        Guid cloneCompanyRealPageId,
        int userId,
        CancellationToken cancellationToken = default);

    Task<RepositoryResponse> InsertHotsPropertyRelationshipAsync(
        Guid baselinePropertyInstanceId,
        Guid clonePropertyInstanceId,
        Guid cloneCompanyRealPageId,
        int userId,
        CancellationToken cancellationToken = default);
}
