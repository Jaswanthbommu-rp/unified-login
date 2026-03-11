using System.Security.Claims;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
    public interface IPersonaRepositoryAsync
    {
        Task<Persona> GetPersonaAsync(long personaId, ClaimsPrincipal user, bool withRights = true, CancellationToken token = default);

        Task<IEnumerable<Persona>> ListActivePersonaAsync(Guid realPageId, bool includeOrganization, CancellationToken token = default);
        //Task<Guid> ChangeCompanyNotification(long personaId, CancellationToken token = default);
        //Task<IEnumerable<ProductInternalSetting>> GetProductInternalSettingsAsync(int productId, CancellationToken token = default);
        //Task<IEnumerable<Persona>> ListEmployeePersonas(long userId, long orgPartyId, CancellationToken token);
        //Task<IEnumerable<UserProductDetail>> GetAllProductsByPersona(long personaId, ProductBatchStatusType statusType, CancellationToken token);
        //Task<long> GetSuperUserCountByOrganizationAsync(long OrganizationPartyId, CancellationToken token = default);
    }
}
