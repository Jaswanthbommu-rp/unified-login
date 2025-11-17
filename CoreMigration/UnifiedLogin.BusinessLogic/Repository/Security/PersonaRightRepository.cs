using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using RealPage.DataAccess.Dapper;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing.Security;

namespace UnifiedLogin.BusinessLogic.Repository.Security;

/// <summary>
///
/// </summary>
public class PersonaRightRepository([FromKeyedServices("DBConnection")] SqlConnection sql) : IPersonaRightRepository
{
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
        IEnumerable<PersonaActionRight> personaRights = rpcache.GetFromCache<IEnumerable<PersonaActionRight>>(cacheKey, 120, () =>
        {
            var param = new { personaId, routeId };
            var result = sql.GetMany<PersonaActionRight>(StoredProcNameConstants.SP_ListPersonaRightsAndActionsByRoute, param);
            return result;
        });
        return personaRights;
    }
}
