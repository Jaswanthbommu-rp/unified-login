using System.Security.Claims;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic;

public class ManagePersonaAsync(IPersonaRepositoryAsync personaRepositoryAsync) : IManagePersonaAsync
{
    public async Task<Persona?> GetPersonaAsync(long personaId, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        if (personaId == 0)
        {
            throw new Exception("Invalid parameter personaId.");
        }
        return await personaRepositoryAsync.GetPersonaAsync(personaId, user, true, cancellationToken);
    }

    /// <summary>
    /// Lists active personas by Enterprise UserId, does NOT include rights correctly!
    /// </summary>
    /// <param name="realPageId">Person Enterprise Id</param>
    /// <param name="includeOrganization">Include organization details</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A List of Persona Object(s)</returns>
    public async Task<IEnumerable<Persona>> ListActivePersonaAsync(Guid realPageId, bool includeOrganization, CancellationToken cancellationToken)
    {
        if (realPageId == Guid.Empty)
        {
            throw new Exception("Invalid parameter realPageId.");
        }

        return await personaRepositoryAsync.ListActivePersonaAsync(realPageId, includeOrganization, cancellationToken);
    }
}

