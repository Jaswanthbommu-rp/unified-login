using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for user registration and password-reset email dispatch.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManageUserRegistrationEmail"/>.
/// </summary>
public interface IManageUserRegistrationEmailAsync
{
    // ── New-User Registration ─────────────────────────────────────────────

    /// <summary>
    /// Sends a welcome/registration email derived from a fully-populated profile.
    /// Delegates to the explicit-parameter overload below.
    /// </summary>
    Task<bool> SendNewUserRegistrationEmailAsync(
        IProfileDetail profile,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a welcome/registration email using explicit user and organization parameters.
    /// Generates an activity token when the user is in a pending/expired/disabled state.
    /// </summary>
    Task<bool> SendNewUserRegistrationEmailAsync(
        UserLoginOnly userLoginOnly,
        string companyName,
        int userTypeId,
        long organizationPartyId,
        CancellationToken cancellationToken = default);

    // ── Password Reset ────────────────────────────────────────────────────

    /// <summary>Sends a password-reset email to the given user.</summary>
    Task<bool> SendPasswordResetEmailAsync(
        ProfileDetail profileDetail,
        CancellationToken cancellationToken = default);
}
