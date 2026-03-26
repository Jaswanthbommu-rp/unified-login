using UnifiedLogin.BusinessLogic.Repository.Security;
using UnifiedLogin.SharedObjects.Landing.Security;

namespace UnifiedLogin.BusinessLogic.Repository.Security;

/// <summary>
/// Async data-access interface for persona rights.
/// Replaces: sync <see cref="IPersonaRightRepository"/>.
/// </summary>
public interface IPersonaRightRepositoryAsync
{
    /// <summary>
    /// Returns all rights and actions assigned to the persona for the given route.
    /// Results are cached per persona + route for 2 minutes.
    /// </summary>
    Task<IEnumerable<PersonaActionRight>> ListRightsAndActionsByPersonaIdAsync(
        long personaId,
        string routeId,
        CancellationToken cancellationToken = default);
}