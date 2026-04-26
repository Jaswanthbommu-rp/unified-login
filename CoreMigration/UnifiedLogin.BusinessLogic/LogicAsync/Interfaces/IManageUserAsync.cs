using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for core user management operations.
/// Replaces: direct instantiation of sync <c>ManageUser</c> inside async services.
/// </summary>
/// <remarks>
/// <para>
/// <b>Stepping-stone status:</b> <see cref="UpdateUserAsync"/> and <see cref="UpdateUserStatusAsync"/>
/// still wrap two sync <c>IUserRepository</c> calls that have no async counterpart yet
/// (<c>UpdateUser</c>, <c>DisableUserProduct</c>, <c>ActivateUserProducts</c>).
/// Those calls are dispatched via <c>Task.Run</c> until <c>IUserRepositoryAsync</c> gains the methods.
/// </para>
/// <para>
/// <b>DI registration:</b> register as <c>Scoped</c>.
/// </para>
/// </remarks>
public interface IManageUserAsync
{
    /// <summary>
    /// Updates an existing user's profile, products, and optional notification email.
    /// Fetches the existing profile snapshot and persona list asynchronously before persisting.
    /// </summary>
    Task<RepositoryResponse> UpdateUserAsync(
        Guid loggedInUserRealPageId,
        IProfileDetail profile,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cascades a login-status change to all active product assignments for the given users.
    /// Handles both <see cref="UserUiStatusType.Disabled"/> and <see cref="UserUiStatusType.Active"/>.
    /// </summary>
    Task<RepositoryResponse> UpdateUserStatusAsync(
        Guid editorRealPageId,
        long editorPersonaId,
        IList<UserLoginOnly> userLogins,
        UserUiStatusType? userLoginStatusType,
        CancellationToken cancellationToken = default);
}
