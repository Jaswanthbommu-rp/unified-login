using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.ResearchApplication;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// True-async interface for Research Application (Black Book) user operations.
/// Replaces the stepping-stone wrapper that required <see cref="DefaultUserClaim"/> at call time.
/// Context resolution is handled internally via <see cref="IProductContextServiceAsync"/>.
/// </summary>
public interface IManageResearchApplicationAsync
{
    /// <summary>Returns all roles available for the given party/product, marking any already assigned to the user.</summary>
    Task<ListResponse> GetRolesAsync(
        long editorPersonaId,
        long userPersonaId,
        long partyId,
        CancellationToken cancellationToken = default);

    /// <summary>Returns all rights for the given role within the party/product context.</summary>
    Task<ListResponse> GetRightsByRoleAsync(
        long editorPersonaId,
        long partyId,
        long roleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates the Research Application user in the GB database and notifies the
    /// Research Application via the event API when the user's role changes.
    /// </summary>
    Task<string> ManageResearchApplicationUserAsync(
        long editorPersonaId,
        long userPersonaId,
        ResearchAppRoleAndPropertyList userAssignProductPropertyRole,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the user's role from the GB database, sets product status to Deleted,
    /// and notifies the Research Application via the event API.
    /// </summary>
    Task<string> UnassignUserAsync(
        long editorPersonaId,
        long userPersonaId,
        ResearchAppRoleAndPropertyList userAssignProductPropertyRole,
        CancellationToken cancellationToken = default);
}
