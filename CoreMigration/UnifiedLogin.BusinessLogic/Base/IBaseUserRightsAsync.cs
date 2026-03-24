using System.Security.Claims;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Base;

/// <summary>
/// Async contract for the user-rights engine.
/// Replaces sync <see cref="IBaseUserRights"/> which depended on <c>new RPObjectCache()</c>
/// and sync repository calls.
/// </summary>
public interface IBaseUserRightsAsync
{
    /// <summary>
    /// Resolve the full rights list for the authenticated principal.
    /// This is the async equivalent of <c>BaseUserRights.GetUserRightsBy</c>.
    /// </summary>
    Task<List<string>> GetUserRightsAsync(
        ClaimsPrincipal userPrincipal,
        DefaultUserClaim userClaim,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolve impersonated rights for a given impersonator's RealPageId.
    /// </summary>
    Task<List<string>> GetImpersonatedUserRightsAsync(
        Guid impersonatedBy,
        DefaultUserClaim userClaims,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolve impersonated rights when the impersonator's persona is already known.
    /// </summary>
    Task<List<string>> GetImpersonatedUserRightsByPersonaAsync(
        Persona impersonateUserPersona,
        DefaultUserClaim userClaims,
        CancellationToken cancellationToken = default);
}