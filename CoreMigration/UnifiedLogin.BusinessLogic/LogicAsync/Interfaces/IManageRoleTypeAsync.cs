using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for role-type lookup operations.
/// Wraps legacy <see cref="UnifiedLogin.BusinessLogic.Logic.ManageRoleType"/> calls plus
/// external-user relationship and relationship-type lookups behind a single async method.
/// </summary>
public interface IManageRoleTypeAsync
{
    /// <summary>
    /// Resolves the role-type list for the current user.
    /// <paramref name="persona"/> is non-null when the "User Role" branch applies
    /// (pre-resolved by the controller); null triggers the generic <c>GetRoleType</c> path.
    /// Returns null when the role-type list itself is null (caller should return NoContent).
    /// </summary>
    Task<IList<RoleType>> ListRoleTypeAsync(
        string roleTypeName,
        string loginName,
        bool includeRelationShips,
        Persona persona,
        DefaultUserClaim userClaim,
        CancellationToken cancellationToken = default);
}
