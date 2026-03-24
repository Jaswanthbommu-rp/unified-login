using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Hots;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

public interface IHOTSCloneUserRepositoryAsync
{
    Task<IList<BaseLineCustomerCompanyUser>> ListUsersAsync(long organizationId, CancellationToken cancellationToken = default);
    Task<List<PersonaProductUserDetails>> GetUserProductsAsync(long personaId, CancellationToken cancellationToken = default);
    Task<Guid> GetBaseCompanyUPFMIdAsync(Guid cloneUpfmId, CancellationToken cancellationToken = default);
    Task<UserLoginOnly> GetUserLoginOnlyAsync(string enterpriseUserName, CancellationToken cancellationToken = default);
    Task<IList<Persona>> ListPersonaAsync(Guid realPageId, CancellationToken cancellationToken = default);
    Task<HotsUser> CreateUserAsync(DefaultUserClaim cloneCompanyAdminUserClaim, long partyId, BaseLineCustomerCompanyUser user, IProfileDetail baseUserProfile, List<ProductBatch> productBatch, UserLogin userLogin, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> InsertHotsCompanyRelationshipAsync(Guid baselineCompanyRealPageId, Guid cloneCompanyRealPageId, int userId, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> InsertHotsPropertyRelationshipAsync(Guid baselinePropertyInstanceId, Guid clonePropertyInstanceId, Guid cloneCompanyRealPageId, int userId, CancellationToken cancellationToken = default);
}