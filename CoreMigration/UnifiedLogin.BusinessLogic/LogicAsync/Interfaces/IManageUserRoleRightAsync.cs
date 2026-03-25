using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first replacement for <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManageUserRoleRight"/>.
/// All methods delegate to <see cref="UnifiedLogin.BusinessLogic.Repository.Interfaces.IUserRoleRightRepositoryAsync"/>
/// — no <c>new</c> keyword, no blocking <c>.Result</c>.
/// </summary>
public interface IManageUserRoleRightAsync
{
    /// <summary>
    /// Returns the roles assigned to a persona for the given product.
    /// Replaces: <c>ManageUserRoleRight.GetAssignedRoleForPersona</c>.
    /// </summary>
    Task<IList<Role>> GetAssignedRoleForPersonaAsync(
        ProductEnum productId,
        long? userPersonaId = null,
        long? organizationPartyId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the single role ID assigned to a persona for the given product.
    /// </summary>
    Task<long> GetRoleIdByPersonaAsync(
        long userPersonaId,
        ProductEnum productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all role IDs assigned to a persona for the given product.
    /// </summary>
    Task<List<long>> GetRoleIdsByPersonaAsync(
        long userPersonaId,
        ProductEnum productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns or removes a role for a user persona.
    /// </summary>
    Task<RepositoryResponse> InsertAssignedRoleToUserAsync(
        long userPersonaId,
        long roleId,
        int userId,
        bool deleteRole = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all roles with their associated rights in a master-detail hierarchy.
    /// </summary>
    Task<IList<UserRoleRights>> GetAllRoleRightsAsync(
        long partyId,
        IList<int> productIdList,
        int productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all platform roles with their associated rights in a master-detail hierarchy.
    /// </summary>
    Task<IList<UnifiedLoginRoleRights>> GetPlatformRoleRightsAsync(
        long partyId,
        IList<int> productIdList,
        int productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all AD-group rights assigned to a persona.
    /// </summary>
    Task<IList<Right>> GetADGroupRightsByPersonaIdAsync(
        long personaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all rights that must persist during impersonation.
    /// </summary>
    Task<IList<Right>> GetPersistRightsAsync(
        CancellationToken cancellationToken = default);
}