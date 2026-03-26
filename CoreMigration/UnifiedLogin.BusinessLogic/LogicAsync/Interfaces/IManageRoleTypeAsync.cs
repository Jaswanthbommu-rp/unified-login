using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for role-type lookup operations.
/// Mirrors every method on the sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManageRoleType"/>
/// plus the higher-level controller-facing orchestration method.
/// </summary>
public interface IManageRoleTypeAsync
{
    // ── Direct async equivalents of IManageRoleType ───────────────────────

    /// <summary>
    /// Returns role types filtered by name and party, applying optional
    /// login-name–based persona-organisation filtering.
    /// <paramref name="orgMasterId"/> is accepted for API compatibility;
    /// it is not currently used in filtering logic.
    /// </summary>
    Task<IList<RoleType>> GetRoleTypeAsync(
        string roleTypeName,
        long? partyId,
        long? orgMasterId = null,
        string? loginName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns role type dependencies for the given <paramref name="roleTypeId"/>,
    /// applying optional login-name–based persona-organisation filtering.
    /// <paramref name="orgMasterId"/> is accepted for API compatibility;
    /// it is not currently used in filtering logic.
    /// </summary>
    Task<IList<RoleType>> GetRoleTypeDependencyAsync(
        long? roleTypeId,
        long? partyId,
        long? orgMasterId = null,
        string? loginName = null,
        CancellationToken cancellationToken = default);

    // ── Controller-facing orchestration method ────────────────────────────

    /// <summary>
    /// Resolves the role-type list for the current user, applying external-user
    /// operator filtering, unauthenticated RP-Employee removal, and optional
    /// relationship-type enrichment.
    /// <paramref name="persona"/> is non-null when the "User Role" branch applies
    /// (pre-resolved by the controller); <c>null</c> triggers the generic
    /// <see cref="GetRoleTypeAsync"/> path.
    /// Returns <c>null</c> when the role-type list is null (caller should return NoContent).
    /// </summary>
    Task<IList<RoleType>> ListRoleTypeAsync(
        string roleTypeName,
        string? loginName,
        bool includeRelationShips,
        Persona? persona,       
        CancellationToken cancellationToken = default);
}
