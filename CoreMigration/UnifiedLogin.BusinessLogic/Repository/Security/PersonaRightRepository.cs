using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing.Enum;
using UnifiedLogin.SharedObjects.Landing.Security;
using IRepository = UnifiedLogin.DataAccess.IRepository;

namespace UnifiedLogin.BusinessLogic.Repository.Security;

/// <summary>
///
/// </summary>
public class PersonaRightRepository : BaseRepository, IPersonaRightRepository
{
    IProductInternalSettingRepository _productInternalSettingRepository;
    public PersonaRightRepository() : base(DbConnectionEnum.IdpConfigurationDb)
    {
        _productInternalSettingRepository = new ProductInternalSettingRepository();
    }

    public PersonaRightRepository(IRepository repository) : base(repository)
    {
        _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
    }
    /// <summary>
    /// List Rights By PersonaId for given route
    /// </summary>
    /// <param name="personaId"></param>
    /// <param name="routeId"></param>
    /// <returns></returns>
    public IEnumerable<PersonaActionRight> ListRightsAndActionsByPersonaId(long personaId, string routeId)
    {
        RPObjectCache rpcache = new RPObjectCache();
        var cacheKey = $"listRightsAndActionsByPersonaId_{personaId}_{routeId}";
        using (var repository = GetRepository())
        {
            IEnumerable<PersonaActionRight> personaRights = rpcache.GetFromCache<IEnumerable<PersonaActionRight>>(cacheKey, 120, () =>
            {
                var param = new { personaId, routeId };
                var result = repository.GetMany<PersonaActionRight>(StoredProcNameConstants.SP_ListPersonaRightsAndActionsByRoute, param);
                return result;
            });
            return personaRights;
        }        
    }
}
