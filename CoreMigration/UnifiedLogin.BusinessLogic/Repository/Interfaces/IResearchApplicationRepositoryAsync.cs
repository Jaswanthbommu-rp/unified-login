using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.ResearchApplication;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

public interface IResearchApplicationRepositoryAsync
{
    Task<List<ProductRole>> ListRolesByPartyAsync(long partyId, CancellationToken cancellationToken = default);
    Task<List<ProductRole>> ListRolesAssignedToPersonaAsync(long userPersonaId, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> InsertDelAssignedPropRoleToUserAsync(long userPersonaId, long productId, UserLocation property, UserAccessGroup role, long del = 0, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> InsertDelAssignedPropRoleToUserNewAsync(long userPersonaId, int productId, long propertyId, long roleId, long del = 0, CancellationToken cancellationToken = default);
}