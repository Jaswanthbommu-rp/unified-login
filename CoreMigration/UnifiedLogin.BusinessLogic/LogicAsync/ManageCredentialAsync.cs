using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for credential management operations.
/// Delegates to the existing sync <see cref="IManageCredential"/> via <see cref="Task.FromResult{TResult}"/>.
/// </summary>
public sealed class ManageCredentialAsync : IManageCredentialAsync
{
    private readonly IManageCredential _manageCredential;

    public ManageCredentialAsync(IManageCredential manageCredential)
    {
        _manageCredential = manageCredential ?? throw new ArgumentNullException(nameof(manageCredential));
    }

    public Task<SecurityQuestionResponse> GetSecurityQuestionAsync(string enterpriseUserName, UserDeviceDetails userDeviceDetails, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageCredential.GetSecurityQuestion(enterpriseUserName, userDeviceDetails));

    public Task<SecurityAnswerResponse> VerifySecurityAnswersAsync(UserSecurityAnswer userSecurityAnswer, UserDeviceDetails userDeviceDetails, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageCredential.VerifySecurityAnswers(userSecurityAnswer, userDeviceDetails));

    public Task<ChangePasswordResponse> ForgotPasswordAsync(ChangePassword changePassword, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageCredential.ForgotPassword(changePassword));

    public Task<ValidatePasswordResponse> ValidatePasswordForUserAsync(string enterpriseUserName, string passwordToValidate, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageCredential.ValidatePasswordForUser(enterpriseUserName, passwordToValidate));

    public Task<CheckPasswordExpirationResponse> CheckPasswordExpirationAsync(long userId, Guid enterpriseUserRealPageId, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageCredential.CheckPasswordExpiration(userId, enterpriseUserRealPageId));

    public Task<UserAllSecurityQuestionResponse> UserAllSecurityQuestionsAsync(string enterpriseUserName, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageCredential.UserAllSecurityQuestions(enterpriseUserName));

    public Task<ListResponse> GetUserAsync(string enterpriseUserName, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageCredential.GetUser(enterpriseUserName));

    public Task<ChangePasswordResponse> SetPasswordAsync(SetPassword setPassword, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageCredential.SetPassword(setPassword));

    public Task<SetUserSecurityQuestionsResponse> SetUserSecurityQuestionsAsync(UserSecurityAnswer userSecurityQuestionsAnswers, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageCredential.SetUserSecurityQuestions(userSecurityQuestionsAnswers));

    public Task<ResetPasswordResponse> ResetPasswordAsync(Guid realPageId, UserResetPassword userResetPassword, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageCredential.ResetPassword(realPageId, userResetPassword));

    public Task<UsersAllSecurityQuestionResponse> GetUserSelectedSecurityQuestionsAsync(Guid realPageId, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageCredential.GetUserSelectedSecurityQuestions(realPageId));

    public Task<SaveUserSelectedSecurityQuestionResponse> SaveUserSelectedSecurityQuestionsAsync(Guid realPageId, IList<SecurityQuestionAnswer> securityQuestionAnswer, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageCredential.SaveUserSelectedSecurityQuestions(realPageId, securityQuestionAnswer));

    public Task<ResetPasswordResponse> SetTemporaryPasswordAsync(Guid realPageId, UserResetPassword userResetPassword, DefaultUserClaim userClaim, CancellationToken cancellationToken = default)
    {
        var credentialService = new ManageCredential(userClaim);
        return Task.FromResult(credentialService.SetTemporaryPassword(realPageId, userResetPassword));
    }
}
