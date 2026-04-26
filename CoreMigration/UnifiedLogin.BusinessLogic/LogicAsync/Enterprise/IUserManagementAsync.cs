using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.ResponseObject;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Enterprise;

/// <summary>
/// Async-first interface for enterprise user management operations.
/// <para>
/// Replaces <c>UserManagement</c> whose methods accepted <c>DefaultUserClaim</c> via the
/// constructor and created all repositories inline with <c>new Xxx(_userClaims)</c>.
/// Caller identity is now resolved internally via <c>IUserClaimsAccessor</c>.
/// </para>
/// </summary>
public interface IUserManagementAsync
{
    /// <summary>
    /// Creates a new enterprise Unity user, queues initial product assignments, and
    /// optionally sends an invitation email.
    /// </summary>
    Task<ObjectResponse> CreateEnterpriseUnityUserAsync(
        UserProductDetails userProductDetails,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing enterprise Unity user's profile and product assignments.
    /// </summary>
    Task<ObjectResponse> UpdateEnterpriseUnityUserAsync(
        UserProductDetails userProductDetails,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates or deactivates the specified user and cascades the status change
    /// across all active product assignments.
    /// </summary>
    Task<ObjectResponse> ActivateDeactivateUserAsync(
        Guid unityRealPageUserId,
        UserUiStatusType statusTypeName,
        CancellationToken cancellationToken = default);

    /// <summary>Returns a paged/filtered list of users for the given organisation.</summary>
    Task<IList<UsersData>> ListUsersAsync(
        long organizationPartyId,
        int statusTypeId,
        Guid? realPageId = null,
        string? name = null,
        int rowsPerPage = 0,
        int pageNumber = 1,
        CancellationToken cancellationToken = default);

    /// <summary>Returns product-login details for each product assigned to the persona.</summary>
    Task<IList<UserProductDetailLogin>> ListUserProductDetailsLoginByPersonaIdAsync(
        long personaId,
        CancellationToken cancellationToken = default);

    /// <summary>Returns product-login details keyed by login name, across all companies.</summary>
    Task<IList<UserProductDetailLogin>> ListUserProductDetailsLoginByLoginNameAsync(
        string loginName,
        CancellationToken cancellationToken = default);
}
