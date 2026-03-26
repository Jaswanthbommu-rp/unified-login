using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.IntelligentBuilding;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for Intelligent Building user management.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManageIntelligentBuilding"/>.
/// </summary>
public interface IManageIntelligentBuildingAsync
{
    /// <summary>
    /// Creates or updates a user in Intelligent Building.
    /// </summary>
    Task<string> ManageIntelligentBuildingUserAsync(
        long editorPersonaId,
        long userPersonaId,
        IBPropertyRole userAssignProductPropertyRole,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unassigns a user from Intelligent Building, removing all roles and properties.
    /// </summary>
    Task<string> UnassignUserAsync(
        long editorPersonaId,
        long userPersonaId,
        IBPropertyRole userAssignProductPropertyRole,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the available roles for a given party, merged with the user's currently assigned role.
    /// </summary>
    Task<ListResponse> GetRolesAsync(
        long editorPersonaId,
        long userPersonaId,
        long partyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns rights for a role, with the role's currently assigned rights flagged.
    /// </summary>
    Task<ListResponse> GetRightsByRoleAsync(
        long editorPersonaId,
        long partyId,
        long roleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns translated UPFM properties assigned to the given user.
    /// </summary>
    Task<ListResponse> GetUPFMPropertiesAsync(
        long userPersonaId,
        string include = null,
        CancellationToken cancellationToken = default);
}
