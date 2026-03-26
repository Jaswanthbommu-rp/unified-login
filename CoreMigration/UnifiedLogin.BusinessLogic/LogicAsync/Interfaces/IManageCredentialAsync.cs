using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for credential management operations.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManageCredential"/> + blocking <c>Task.Run</c> calls.
/// <c>SetTemporaryPasswordAsync</c> also replaces the <c>new ManageCredential(userClaim)</c> pattern.
/// </summary>
public interface IManageCredentialAsync
{
    Task<SecurityQuestionResponse> GetSecurityQuestionAsync(string enterpriseUserName, UserDeviceDetails userDeviceDetails, CancellationToken cancellationToken = default);
    Task<SecurityAnswerResponse> VerifySecurityAnswersAsync(UserSecurityAnswer userSecurityAnswer, UserDeviceDetails userDeviceDetails, CancellationToken cancellationToken = default);
    Task<ChangePasswordResponse> ForgotPasswordAsync(ChangePassword changePassword, CancellationToken cancellationToken = default);
    Task<ValidatePasswordResponse> ValidatePasswordForUserAsync(string enterpriseUserName, string passwordToValidate, CancellationToken cancellationToken = default);
    Task<CheckPasswordExpirationResponse> CheckPasswordExpirationAsync(long userId, Guid enterpriseUserRealPageId, CancellationToken cancellationToken = default);
    Task<UserAllSecurityQuestionResponse> UserAllSecurityQuestionsAsync(string enterpriseUserName, CancellationToken cancellationToken = default);
    Task<ListResponse> GetUserAsync(string enterpriseUserName, CancellationToken cancellationToken = default);
    Task<ChangePasswordResponse> SetPasswordAsync(SetPassword setPassword, CancellationToken cancellationToken = default);
    Task<SetUserSecurityQuestionsResponse> SetUserSecurityQuestionsAsync(UserSecurityAnswer userSecurityQuestionsAnswers, CancellationToken cancellationToken = default);
    Task<ResetPasswordResponse> ResetPasswordAsync(Guid realPageId, UserResetPassword userResetPassword, CancellationToken cancellationToken = default);
    Task<UsersAllSecurityQuestionResponse> GetUserSelectedSecurityQuestionsAsync(Guid realPageId, CancellationToken cancellationToken = default);
    Task<SaveUserSelectedSecurityQuestionResponse> SaveUserSelectedSecurityQuestionsAsync(Guid realPageId, IList<SecurityQuestionAnswer> securityQuestionAnswer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a temporary password using a per-request <see cref="ManageCredential"/> instance
    /// constructed with the caller's <paramref name="userClaim"/>.
    /// Replaces: <c>new ManageCredential(userClaim).SetTemporaryPassword(...)</c>.
    /// </summary>
    Task<ResetPasswordResponse> SetTemporaryPasswordAsync(Guid realPageId, UserResetPassword userResetPassword, CancellationToken cancellationToken = default);
}
