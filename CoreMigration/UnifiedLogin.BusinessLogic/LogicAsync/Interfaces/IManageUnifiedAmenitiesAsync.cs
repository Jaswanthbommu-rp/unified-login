using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.UnifiedAmenities;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for Unified Amenities user management.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Product.Interfaces.IManageUnifiedAmenities"/>.
/// </summary>
public interface IManageUnifiedAmenitiesAsync
{
    /// <summary>
    /// Creates or updates a user in Unified Amenities.
    /// </summary>
    Task<string> ManageUnifiedAmenitiesUserAsync(
        long editorPersonaId,
        long userPersonaId,
        UnifiedAmenitiesPropertyRole userAssignProductPropertyRole,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unassigns a user from Unified Amenities, removing all assigned roles.
    /// </summary>
    Task<string> UnassignUserAsync(
        long editorPersonaId,
        long userPersonaId,
        UnifiedAmenitiesPropertyRole userAssignProductPropertyRole,
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
    /// Returns the list of properties for the company or for the given user.
    /// </summary>
    Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId,
        long userPersonaId,
        bool assignedOnly,
        RequestParameter datafilter,
        CancellationToken cancellationToken = default);
}
