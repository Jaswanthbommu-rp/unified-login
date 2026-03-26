using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for credential management operations.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManageCredential"/> + blocking <c>Task.Run</c> calls.
/// </summary>
public interface IManageCredentialAsync
{
    // ── Forgot Password workflow ──────────────────────────────────────────

    /// <summary>Gets security questions for the forgot-password flow and creates an activity token.</summary>
    Task<SecurityQuestionResponse> GetSecurityQuestionAsync(
        string enterpriseUserName,
        UserDeviceDetails userDeviceDetails,
        CancellationToken cancellationToken = default);

    /// <summary>Verifies the user's security answers and returns a correct-answer token on success.</summary>
    Task<SecurityAnswerResponse> VerifySecurityAnswersAsync(
        UserSecurityAnswer userSecurityAnswer,
        UserDeviceDetails userDeviceDetails,
        CancellationToken cancellationToken = default);

    /// <summary>Completes the forgot-password flow by setting the new password using the correct-answer token.</summary>
    Task<ChangePasswordResponse> ForgotPasswordAsync(
        ChangePassword changePassword,
        CancellationToken cancellationToken = default);

    // ── Validate Password ─────────────────────────────────────────────────

    /// <summary>Validates a password against the org's password policy for the given user.</summary>
    Task<ValidatePasswordResponse> ValidatePasswordForUserAsync(
        string enterpriseUserName,
        string passwordToValidate,
        CancellationToken cancellationToken = default);

    /// <summary>Core password-policy validation logic accepting a <see cref="ValidatePassword"/> request.</summary>
    Task<ValidatePasswordResponse> ValidatePasswordAsync(
        ValidatePassword validatePassword,
        CancellationToken cancellationToken = default);

    // ── Reset / Set Password ──────────────────────────────────────────────

    /// <summary>Resets the password for an existing user who knows their old password.</summary>
    Task<ResetPasswordResponse> ResetPasswordAsync(
        Guid realPageId,
        UserResetPassword userResetPassword,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a temporary password for the user (admin-initiated reset).
    /// The user will be required to change it on next login.
    /// </summary>
    Task<ResetPasswordResponse> SetTemporaryPasswordAsync(
        Guid realPageId,
        UserResetPassword userResetPassword,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a new password for a user completing the new-user registration flow
    /// (verified via activity token).
    /// </summary>
    Task<ChangePasswordResponse> SetPasswordAsync(
        SetPassword setPassword,
        CancellationToken cancellationToken = default);

    // ── Password Expiration ───────────────────────────────────────────────

    /// <summary>Calculates days until password expiry and severity level based on the org's password policy.</summary>
    Task<CheckPasswordExpirationResponse> CheckPasswordExpirationAsync(
        long userId,
        Guid enterpriseUserRealPageId,
        CancellationToken cancellationToken = default);

    // ── Security Questions ────────────────────────────────────────────────

    /// <summary>Returns all available security questions (system default + optional custom).</summary>
    Task<UserAllSecurityQuestionResponse> UserAllSecurityQuestionsAsync(
        string enterpriseUserName,
        CancellationToken cancellationToken = default);

    /// <summary>Sets the user's security question answers during registration or profile update.</summary>
    Task<SetUserSecurityQuestionsResponse> SetUserSecurityQuestionsAsync(
        UserSecurityAnswer userSecurityQuestionsAnswers,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the security questions the user has already selected.</summary>
    Task<UsersAllSecurityQuestionResponse> GetUserSelectedSecurityQuestionsAsync(
        Guid realPageId,
        CancellationToken cancellationToken = default);

    /// <summary>Saves or updates the user's selected security questions and answers.</summary>
    Task<SaveUserSelectedSecurityQuestionResponse> SaveUserSelectedSecurityQuestionsAsync(
        Guid realPageId,
        IList<SecurityQuestionAnswer> securityQuestionAnswer,
        CancellationToken cancellationToken = default);

    // ── Misc ──────────────────────────────────────────────────────────────

    /// <summary>Returns a minimal <see cref="ListResponse"/> containing the user's login record.</summary>
    Task<ListResponse> GetUserAsync(
        string enterpriseUserName,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the identity provider type for the given login name.</summary>
    Task<IdentityProviderType> GetIdentityProviderTypeByLoginNameAsync(
        string enterpriseUserName,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a new-user-registration verification token for the given user.</summary>
    Task<string> GetNewUserRegistrationVerificationTokenAsync(
        long userId,
        Guid realPageId,
        CancellationToken cancellationToken = default);
}
