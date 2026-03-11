using System.Security.Claims;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces;

public interface IManagePersonaAsync
{
    Task<Persona?> GetPersonaAsync(long personaId, ClaimsPrincipal user, CancellationToken cancellationToken);
    Task<IEnumerable<Persona>> ListActivePersonaAsync(Guid realPageId, bool includeOrganization, CancellationToken cancellationToken);
}

