using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first credential management — directly consumes async repositories.
/// Replaces the <c>Task.FromResult</c> stepping-stone wrapper.
/// </summary>
public sealed class ManageCredentialAsync : IManageCredentialAsync
{
    #region Fields

    private readonly ICredentialRepositoryAsync _credentialRepository;
    private readonly IPasswordPolicyRepositoryAsync _passwordPolicyRepository;
    private readonly IUserLoginRepositoryAsync _userLoginRepository;
    private readonly IManageUserLoginAsync _manageUserLogin;
    private readonly IManagePersonAsync _managePerson;
    private readonly IManageOrganizationAsync _manageOrganization;
    private readonly IUserRepositoryAsync _userRepository;
    private readonly IUserClaimsAccessor _userClaims;
    private readonly ILogger<ManageCredentialAsync> _logger;

    private const int MaxRandomQuestions = 2;
    private const int RequiredSecurityQuestionCount = 3;
    private const int MaxAnswerLength = 50;

    #endregion

    #region Constructor

    public ManageCredentialAsync(
        ICredentialRepositoryAsync credentialRepository,
        IPasswordPolicyRepositoryAsync passwordPolicyRepository,
        IUserLoginRepositoryAsync userLoginRepository,
        IManageUserLoginAsync manageUserLogin,
        IManagePersonAsync managePerson,
        IManageOrganizationAsync manageOrganization,
        IUserRepositoryAsync userRepository,
        IUserClaimsAccessor userClaims,
        ILogger<ManageCredentialAsync> logger)
    {
        _credentialRepository = credentialRepository ?? throw new ArgumentNullException(nameof(credentialRepository));
        _passwordPolicyRepository = passwordPolicyRepository ?? throw new ArgumentNullException(nameof(passwordPolicyRepository));
        _userLoginRepository = userLoginRepository ?? throw new ArgumentNullException(nameof(userLoginRepository));
        _manageUserLogin = manageUserLogin ?? throw new ArgumentNullException(nameof(manageUserLogin));
        _managePerson = managePerson ?? throw new ArgumentNullException(nameof(managePerson));
        _manageOrganization = manageOrganization ?? throw new ArgumentNullException(nameof(manageOrganization));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _userClaims = userClaims ?? throw new ArgumentNullException(nameof(userClaims));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Forgot Password

    /// <inheritdoc/>
    public async Task<SecurityQuestionResponse> GetSecurityQuestionAsync(
        string enterpriseUserName,
        UserDeviceDetails userDeviceDetails,
        CancellationToken cancellationToken = default)
    {
        var response = new SecurityQuestionResponse { IsUserExist = true };

        if (string.IsNullOrEmpty(enterpriseUserName))
        {
            response.IsError = true;
            response.ErrorReason = "No Username specified.";
            return response;
        }

        var userLogin = await _manageUserLogin.GetUserLoginOnlyAsync(enterpriseUserName, cancellationToken);
        if (userLogin is null)
        {
            response.IsError = true;
            response.ErrorReason = $"The Username \"{enterpriseUserName}\" is incorrect or was not found.";
            return response;
        }

        var primaryOrgStatus = await _userLoginRepository.GetUserOrganizationWithStatusAsync(
            userLogin.UserId, userLogin.LastLogin, 0, true);

        response.IsUserPending = primaryOrgStatus.IsPending ?? false;
        response.IsUserActive = primaryOrgStatus.IsActive ?? false;
        response.IsUserExpired = primaryOrgStatus.IsExpired ?? false;

        if (!response.IsUserActive || response.IsUserExpired || response.IsUserPending)
        {
            response.IsError = true;
            response.ErrorReason = "The user is not active in the system.";
            return response;
        }

        if (primaryOrgStatus.StatusTypeId == (int)UserUiStatusType.Locked
            && primaryOrgStatus.StatusThruDate is not null
            && primaryOrgStatus.StatusThruDate.Value < DateTime.UtcNow)
        {
            primaryOrgStatus.IsLocked = false;
            await _userRepository.UpdateUserStatusByCompanyAsync(
                userLogin.RealPageId, primaryOrgStatus.PartyId, (int)UserDbStatusType.Active, DateTime.UtcNow, null, cancellationToken);
            await _userRepository.UpdateUserActivityAttemptsAsync(
                userLogin.LoginName, ActivityType.LoginSuccess, userDeviceDetails, primaryOrgStatus.PartyId, null, cancellationToken);
        }

        response.IsUserLocked = primaryOrgStatus.IsLocked ?? false;
        if (response.IsUserLocked)
        {
            response.IsError = true;
            response.ErrorReason = "The user account is locked.";
            return response;
        }

        if (userLogin.Is3rdPartyIDP)
        {
            response.IsError = true;
            response.ErrorReason = "Forgot password is not applicable to users on external identity provider.";
            return response;
        }

        var securityQuestions = await _credentialRepository.GetUserSecurityQuestionAsync(enterpriseUserName, cancellationToken);
        if (securityQuestions is null || securityQuestions.Count == 0)
        {
            response.IsError = true;
            response.ErrorReason = "User has no security questions defined.";
            return response;
        }

        response.SecurityQuestions = SelectRandomQuestions(securityQuestions);
        response.ActivityToken = await _credentialRepository.CreateActivityTokenAsync(
            primaryOrgStatus.PartyId, userLogin.RealPageId, (int)ActivityType.ForgotPassword, cancellationToken);
        response.EnterpriseUserName = enterpriseUserName;

        if (string.IsNullOrEmpty(response.ActivityToken))
            throw new InvalidOperationException("Unable to generate activity token.");

        return response;
    }

    /// <inheritdoc/>
    public async Task<SecurityAnswerResponse> VerifySecurityAnswersAsync(
        UserSecurityAnswer userSecurityAnswer,
        UserDeviceDetails userDeviceDetails,
        CancellationToken cancellationToken = default)
    {
        var response = new SecurityAnswerResponse();

        if (string.IsNullOrEmpty(userSecurityAnswer.EnterpriseUserName))
            return ErrorAnswer(response, "No Username specified.");

        if (userSecurityAnswer.SecurityQuestionAnswers is null || userSecurityAnswer.SecurityQuestionAnswers.Count == 0)
            return ErrorAnswer(response, "No questions received from user.", answersCorrect: false);

        if (string.IsNullOrEmpty(userSecurityAnswer.ActivityToken))
            return ErrorAnswer(response, "Null or empty security Forgot Password Activity Token.", answersCorrect: false);

        var userLogin = await _userLoginRepository.GetUserLoginOnlyAsync(userSecurityAnswer.EnterpriseUserName);
        if (userLogin is null)
        {
            response.IsError = true;
            response.ErrorReason = "User Name is incorrect or not found.";
            return response;
        }

        var orgPartyId = await _userLoginRepository.GetPrimaryOrgIdByUserIdAsync(userLogin.UserId, cancellationToken);
        var primaryOrgStatus = await _userLoginRepository.GetUserOrganizationWithStatusAsync(
            userLogin.UserId, userLogin.LastLogin, 0, true);

        if (primaryOrgStatus.IsLocked is true)
            return ErrorAnswer(response, "Your account is locked", answersCorrect: false);

        var activityAttempts = await _credentialRepository.GetActivityAttemptExceedsAsync(
            orgPartyId, userSecurityAnswer.EnterpriseUserName, (int)ActivityType.QuestionAttempts, cancellationToken);

        if (activityAttempts.AttemptCount >= activityAttempts.MaxActivitycount)
        {
            await _userRepository.UpdateUserStatusByCompanyAsync(
                userLogin.RealPageId, primaryOrgStatus.PartyId, (int)UserDbStatusType.Locked,
                DateTime.UtcNow, DateTime.UtcNow.AddMinutes(activityAttempts.ActivityTokenExpirationMinutes),
                cancellationToken);
            return ErrorAnswer(response, "Max attempts to answer security questions exceeded. Your account is locked", answersCorrect: false);
        }

        var tokenResult = await _credentialRepository.GetActivityTokenAsync(
            userSecurityAnswer.EnterpriseUserName, userSecurityAnswer.ActivityToken,
            (int)ActivityType.ForgotPassword, orgPartyId, cancellationToken);

        if (tokenResult is null || string.IsNullOrEmpty(tokenResult.Token))
            return ErrorAnswer(response, "Forgot Password Activity Token is expired.", answersCorrect: false);

        var savedAnswers = await _credentialRepository.GetUserSecurityQuestionAnswerAsync(
            userSecurityAnswer.EnterpriseUserName, cancellationToken);

        if (savedAnswers is null || savedAnswers.Count == 0)
            return ErrorAnswer(response, "User has no security questions defined.", answersCorrect: false);

        userSecurityAnswer.SecurityQuestionAnswers = GetHashedAnswers(userSecurityAnswer.SecurityQuestionAnswers);

        await _credentialRepository.UpdateUserActivityAttemptsAsync(
            userSecurityAnswer.EnterpriseUserName, ActivityType.QuestionAttempts,
            userDeviceDetails, orgPartyId, null, cancellationToken);

        foreach (var userQa in userSecurityAnswer.SecurityQuestionAnswers)
        {
            var match = savedAnswers.FirstOrDefault(x => x.SecurityQuestionId == userQa.SecurityQuestionId);
            if (match is null || match.Answer != userQa.Answer)
            {
                var attemptDetails = await _credentialRepository.GetActivityAttemptExceedsAsync(
                    orgPartyId, userSecurityAnswer.EnterpriseUserName,
                    (int)ActivityType.QuestionAttempts, cancellationToken);

                if (attemptDetails.AttemptCount <= attemptDetails.MaxActivitycount - 1)
                {
                    response.IsError = true;
                    response.ErrorReason = "One or more of your answers are incorrect. Please try again with a new set of questions.";
                    response.IsAnswersCorrect = false;
                    var questions = await _credentialRepository.GetUserSecurityQuestionAsync(
                        userSecurityAnswer.EnterpriseUserName, cancellationToken);
                    response.SecurityQuestions = SelectRandomQuestions(questions);
                    return response;
                }

                await _userRepository.UpdateUserStatusByCompanyAsync(
                    userLogin.RealPageId, orgPartyId, (int)UserDbStatusType.Locked,
                    DateTime.UtcNow, DateTime.UtcNow.AddMinutes(activityAttempts.ActivityTokenExpirationMinutes),
                    cancellationToken);
                return ErrorAnswer(response, "Max attempts to answer security questions exceeded. Your account is locked.", answersCorrect: false);
            }
        }

        response.IsAnswersCorrect = true;
        response.EnterpriseUserName = userSecurityAnswer.EnterpriseUserName;
        response.CorrectAnswerToken = await _credentialRepository.CreateActivityTokenAsync(
            orgPartyId, userLogin.RealPageId, (int)ActivityType.VerifyAnswers, cancellationToken);

        return response;
    }

    /// <inheritdoc/>
    public async Task<ChangePasswordResponse> ForgotPasswordAsync(
        ChangePassword changePassword,
        CancellationToken cancellationToken = default)
    {
        var response = new ChangePasswordResponse();

        if (string.IsNullOrEmpty(changePassword.EnterpriseUserName))
            return SetChangePasswordError(response, "No Username specified.");
        if (string.IsNullOrEmpty(changePassword.ActivityToken))
            return SetChangePasswordError(response, "Forgot Password Activity Token is not specified.");
        if (string.IsNullOrEmpty(changePassword.CorrectAnswerToken))
            return SetChangePasswordError(response, "Correct Answer Token is not specified.");
        if (string.IsNullOrEmpty(changePassword.NewPassword))
            return SetChangePasswordError(response, "New Password is not specified.");

        var user = await _userLoginRepository.GetUserLoginOnlyAsync(changePassword.EnterpriseUserName);
        if (user is null)
            return SetChangePasswordError(response, "User name is incorrect or not found.");

        var orgPartyId = await _userLoginRepository.GetPrimaryOrgIdByUserIdAsync(user.UserId, cancellationToken);

        var validation = await ValidatePasswordAsync(new ValidatePassword
        {
            EnterpriseUserName = changePassword.EnterpriseUserName,
            PasswordToValidate = changePassword.NewPassword,
            PartyId = orgPartyId,
            PasswordModifiedDate = user.PasswordModifiedDate,
            PasswordHash = user.PasswordHash,
            PasswordSalt = user.PasswordSalt,
            CheckPasswordHistory = true
        }, cancellationToken);

        if (validation.IsError)
            return SetChangePasswordError(response, validation.ErrorReason);

        var newHash = changePassword.NewPassword.PasswordHash();

        var token = await _credentialRepository.GetActivityTokenAsync(
            changePassword.EnterpriseUserName, changePassword.ActivityToken,
            (int)ActivityType.ForgotPassword, orgPartyId, cancellationToken);
        if (token is null || string.IsNullOrEmpty(token.Token))
            return SetChangePasswordError(response, "Forgot Password Activity Token is expired.");

        var answerToken = await _credentialRepository.GetActivityTokenAsync(
            changePassword.EnterpriseUserName, changePassword.CorrectAnswerToken,
            (int)ActivityType.VerifyAnswers, orgPartyId, cancellationToken);
        if (answerToken is null || string.IsNullOrEmpty(answerToken.Token))
            return SetChangePasswordError(response, "Correct Answer Token is expired.");

        var enterpriseUserId = await _credentialRepository.UpdateEnterpriseUserCredentialAsync(
            changePassword.EnterpriseUserName, newHash.PasswordHash, newHash.PasswordSalt,
            changePassword.CorrectAnswerToken, (int)ActivityType.VerifyAnswers, orgPartyId, cancellationToken);

        var person = await _managePerson.GetPersonAsync(user.RealPageId, cancellationToken);
        var booksMasterOrgId = await GetDefaultBooksMasterOrgIdForUserAsync(user.RealPageId, cancellationToken);

        if (string.IsNullOrEmpty(enterpriseUserId))
        {
            SetChangePasswordError(response, "Error while updating password.");
            LogActivityInfo(LogActivityTypeConstants.CHANGE_PASSWORD_FAILURE, orgPartyId, booksMasterOrgId,
                $"User {person.FirstName} {person.LastName} unable to change forgotten password.",
                changePassword.EnterpriseUserName, user.UserId, person, user.RealPageId);
        }
        else
        {
            response.IsSuccess = true;
            response.EnterpriseUserName = changePassword.EnterpriseUserName;
            LogActivityInfo(LogActivityTypeConstants.CHANGE_PASSWORD_SUCCESS, orgPartyId, booksMasterOrgId,
                $"User {person.FirstName} {person.LastName} successfully changed forgotten password.",
                changePassword.EnterpriseUserName, user.UserId, person, user.RealPageId);
        }

        return response;
    }

    #endregion

    #region Validate Password

    /// <inheritdoc/>
    public async Task<ValidatePasswordResponse> ValidatePasswordForUserAsync(
        string enterpriseUserName,
        string passwordToValidate,
        CancellationToken cancellationToken = default)
    {
        var user = await _userLoginRepository.GetUserLoginOnlyAsync(enterpriseUserName);
        if (user is null)
            return new ValidatePasswordResponse { IsError = true, ErrorReason = "User name is incorrect or not found." };

        var orgPartyId = await _userLoginRepository.GetPrimaryOrgIdByUserIdAsync(user.UserId, cancellationToken);

        return await ValidatePasswordAsync(new ValidatePassword
        {
            EnterpriseUserName = enterpriseUserName,
            PartyId = orgPartyId,
            PasswordToValidate = passwordToValidate,
            PasswordModifiedDate = user.PasswordModifiedDate,
            PasswordHash = user.PasswordHash,
            PasswordSalt = user.PasswordSalt,
            CheckPasswordHistory = true
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ValidatePasswordResponse> ValidatePasswordAsync(
        ValidatePassword validatePassword,
        CancellationToken cancellationToken = default)
    {
        var response = new ValidatePasswordResponse { IsSuccess = true };
        var errors = new List<string>();

        var passwordPolicy = await _passwordPolicyRepository.GetPasswordPolicyAsync(validatePassword.PartyId, cancellationToken);
        if (passwordPolicy is null)
        {
            response.IsError = true;
            response.ErrorReason = $"Unable to find password policy for organization - {validatePassword.PartyId}";
            return response;
        }

        if (validatePassword.PasswordToValidate.Contains(validatePassword.EnterpriseUserName, StringComparison.OrdinalIgnoreCase))
        {
            response.IsError = true;
            response.ErrorReason = "Your password cannot be the same as your Username.";
            return response;
        }

        if (validatePassword.PasswordToValidate.Length < passwordPolicy.MinimumLength)
            errors.Add($"Your password must be at least {passwordPolicy.MinimumLength} characters.");

        if (validatePassword.PasswordToValidate.Length > passwordPolicy.MaximumLength)
            errors.Add($"Your password must be {passwordPolicy.MaximumLength} characters or less.");

        int upperCount = validatePassword.PasswordToValidate.Count(char.IsUpper);
        int lowerCount = validatePassword.PasswordToValidate.Count(char.IsLower);

        if (lowerCount < passwordPolicy.MinimumLowercase || upperCount < passwordPolicy.MinimumUppercase)
            errors.Add($"Your password must include minimum {passwordPolicy.MinimumLowercase} lower case " +
                       $"characters and minimum {passwordPolicy.MinimumUppercase} upper case characters.");

        if (validatePassword.PasswordToValidate.Count(char.IsDigit) < passwordPolicy.MinimumNumeric)
            errors.Add($"Your password must include {passwordPolicy.MinimumNumeric} numeric characters.");

        if (!Regex.IsMatch(validatePassword.PasswordToValidate, "[^a-z0-9]", RegexOptions.IgnoreCase))
            errors.Add($"Your password must include minimum {passwordPolicy.MinimumSpecialCharacter} special characters.");

        // New-user check: password must not match the temporary password
        if (validatePassword.PasswordModifiedDate is null
            && validatePassword.PasswordHash is not null
            && validatePassword.CheckPasswordHistory)
        {
            var hashed = validatePassword.PasswordToValidate
                .PasswordHashBySalt(Convert.FromBase64String(validatePassword.PasswordSalt));
            if (hashed == validatePassword.PasswordHash)
                errors.Add("Your password cannot be the same as your temporary password.");
        }

        if (errors.Count == 0 && validatePassword.CheckPasswordHistory
            && passwordPolicy.PreventPasswordReuse
            && passwordPolicy.NumberOfPasswordsToRemember > 0)
        {
            var history = await _credentialRepository.GetPasswordHistoryAsync(
                validatePassword.EnterpriseUserName, passwordPolicy.NumberOfPasswordsToRemember, cancellationToken);

            if (history is not null)
            {
                foreach (var pwdHistory in history)
                {
                    if (pwdHistory.PasswordHash is null) continue;
                    var saltedHash = validatePassword.PasswordToValidate
                        .PasswordHashBySalt(Convert.FromBase64String(pwdHistory.PasswordSalt));
                    if (saltedHash == pwdHistory.PasswordHash)
                    {
                        errors.Add($"Your password should not be from past {passwordPolicy.NumberOfPasswordsToRemember} passwords");
                        break;
                    }
                }
            }
        }

        if (errors.Count > 0)
        {
            response.IsError = true;
            response.IsSuccess = false;
            response.ErrorReason = string.Join(" ", errors);
        }

        return response;
    }

    #endregion

    #region Reset / Set Password

    /// <inheritdoc/>
    public async Task<ResetPasswordResponse> ResetPasswordAsync(
        Guid realPageId,
        UserResetPassword userResetPassword,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userResetPassword);

        var response = new ResetPasswordResponse();

        if (realPageId == Guid.Empty)
            return Error(response, "RealPage Id for user not provided.");
        if (string.IsNullOrEmpty(userResetPassword.OldPassword))
            return Error(response, "Old Password is not specified.");
        if (string.IsNullOrEmpty(userResetPassword.NewPassword))
            return Error(response, "New Password is not specified.");

        var user = await _userLoginRepository.GetUserLoginOnlyAsync(realPageId);
        if (user is null)
            return Error(response, "User Name is incorrect or not found.");

        response.EnterpriseUserName = user.LoginName;
        response.UserId = user.UserId;

        if (!userResetPassword.OldPassword.Equals(_userClaims.UserRealPageGuid.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            var hashedSaltPwd = userResetPassword.OldPassword.PasswordHashBySalt(Convert.FromBase64String(user.PasswordSalt));
            if (hashedSaltPwd != user.PasswordHash)
                return Error(response, "Current password is incorrect.");
        }

        var primaryOrg = await _userLoginRepository.GetUserOrganizationWithStatusAsync(
            user.UserId, user.LastLogin, 0, true);

        var validation = await ValidatePasswordAsync(new ValidatePassword
        {
            EnterpriseUserName = user.LoginName,
            PasswordToValidate = userResetPassword.NewPassword,
            PartyId = primaryOrg.PartyId,
            PasswordModifiedDate = user.PasswordModifiedDate,
            PasswordHash = user.PasswordHash,
            PasswordSalt = user.PasswordSalt,
            CheckPasswordHistory = true
        }, cancellationToken);

        if (validation.IsError)
            return Error(response, validation.ErrorReason);

        var newHash = userResetPassword.NewPassword.PasswordHash();
        var spResponse = await _credentialRepository.ResetEnterpriseUserCredentialAsync(
            realPageId, newHash.PasswordHash, newHash.PasswordSalt, primaryOrg.PartyId, cancellationToken);

        if (spResponse is null || spResponse.Id == 0)
        {
            response.IsError = true;
            if (spResponse is not null) response.ErrorReason = spResponse.ErrorMessage;
            return response;
        }

        var updateStatus = await _credentialRepository.UpdateUserStatusByCompanyAsync(
            realPageId, primaryOrg.PartyId, UserUiStatusType.Active, primaryOrg.FromDate, null, cancellationToken);

        if (updateStatus is null || updateStatus.Id == 0)
        {
            response.IsError = true;
            if (updateStatus is not null) response.ErrorReason = updateStatus.ErrorMessage;
            return response;
        }

        LogCredentialActivityWithClaim(LogActivityTypeConstants.CHANGE_PASSWORD_SUCCESS,
            user.LoginName, user.UserId,
            "User {0} {1} successfully reset password.",
            nameof(ResetPasswordAsync));

        response.IsSuccess = true;
        return response;
    }

    /// <inheritdoc/>
    public async Task<ResetPasswordResponse> SetTemporaryPasswordAsync(
        Guid realPageId,
        UserResetPassword userResetPassword,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userResetPassword);

        var response = new ResetPasswordResponse();

        if (realPageId == Guid.Empty)
            return Error(response, "Real Page Id for user not provided.");
        if (string.IsNullOrEmpty(userResetPassword.NewPassword))
            return Error(response, "New Password is not specified");

        var userLogin = await _manageUserLogin.GetUserLoginOnlyAsync(realPageId, cancellationToken);
        if (userLogin is null)
            return Error(response, "User Name is incorrect or not found.");

        var primaryOrgStatus = await _userLoginRepository.GetUserOrganizationWithStatusAsync(
            userLogin.UserId, userLogin.LastLogin, 0, true);

        var validation = await ValidatePasswordAsync(new ValidatePassword
        {
            EnterpriseUserName = userLogin.LoginName,
            PasswordToValidate = userResetPassword.NewPassword,
            PartyId = primaryOrgStatus.PartyId,
            PasswordModifiedDate = userLogin.PasswordModifiedDate,
            PasswordHash = userLogin.PasswordHash,
            PasswordSalt = userLogin.PasswordSalt,
            CheckPasswordHistory = false
        }, cancellationToken);

        if (validation.IsError)
            return Error(response, validation.ErrorReason);

        if (!userLogin.Is3rdPartyIDP)
        {
            var newHash = userResetPassword.NewPassword.PasswordHash();
            var spResponse = await _credentialRepository.SetEnterpriseUserTemporaryPasswordAsync(
                realPageId, primaryOrgStatus.PartyId, newHash.PasswordHash, newHash.PasswordSalt,
                userLogin, primaryOrgStatus, cancellationToken);

            if (spResponse is not null && spResponse.Id == 0)
            {
                response.IsError = true;
                response.ErrorReason = spResponse.ErrorMessage;
                return response;
            }
        }

        try
        {
            var person = await _managePerson.GetPersonAsync(userLogin.RealPageId, cancellationToken);
            var booksMasterOrgId = await GetDefaultBooksMasterOrgIdForUserAsync(userLogin.RealPageId, cancellationToken);

            string message = _userClaims.ImpersonatedBy == Guid.Empty
                ? $"User {_userClaims.FirstName} {_userClaims.LastName} inserted a temporary password for user {person.FirstName} {person.LastName}."
                : $"User RealPage Access ({_userClaims.ImpersonatedByName}) inserted a temporary password for user {person.FirstName} {person.LastName}.";

            LogActivity.WriteActivity(new ActivityDetails
            {
                LogActivityTypeName = LogActivityTypeConstants.CHANGE_PASSWORD_SUCCESS,
                LogCategoryName = LogActivityCategoryType.Security.ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                BooksMasterOrganizationId = booksMasterOrgId,
                OrganizationPartyId = _userClaims.OrganizationPartyId,
                Message = message,
                FromUserLoginName = _userClaims.LoginName,
                FromUserLoginId = _userClaims.UserId,
                FromUserFirstName = _userClaims.FirstName,
                FromUserLastName = _userClaims.LastName,
                FromUserRealpageId = _userClaims.UserRealPageGuid.ToString(),
                ToUserLoginId = userLogin.UserId,
                ToUserLoginName = userLogin.LoginName
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Activity logging failed in {Method}", nameof(SetTemporaryPasswordAsync));
        }

        response.IsSuccess = true;
        return response;
    }

    /// <inheritdoc/>
    public async Task<ChangePasswordResponse> SetPasswordAsync(
        SetPassword setPassword,
        CancellationToken cancellationToken = default)
    {
        var response = new ChangePasswordResponse();

        if (string.IsNullOrEmpty(setPassword.EnterpriseUserName))
            return SetChangePasswordError(response, "No Username specified.");
        if (string.IsNullOrEmpty(setPassword.ActivityToken))
            return SetChangePasswordError(response, "Set Password Activity Token is not specified.");
        if (string.IsNullOrEmpty(setPassword.NewPassword))
            return SetChangePasswordError(response, "New Password is not specified.");

        var userLogin = await _userLoginRepository.GetUserLoginOnlyAsync(setPassword.EnterpriseUserName);
        if (userLogin is null)
            return SetChangePasswordError(response, "Username is incorrect or not found.");

        if (userLogin.Is3rdPartyIDP)
            return SetChangePasswordError(response, "Set password is not applicable to users on external identity provider.");

        var primaryOrgStatus = await _userLoginRepository.GetUserOrganizationWithStatusAsync(
            userLogin.UserId, userLogin.LastLogin, 0, true);
        var primaryOrg = await _manageOrganization.GetOrganizationAsync(primaryOrgStatus.RealPageId);

        // Populate claim org info for new-user registration where it may be absent
        if (_userClaims.OrganizationPartyId == 0)
        {
            _userClaims.UserClaim.OrganizationPartyId = primaryOrg.PartyId;
            _userClaims.UserClaim.OrganizationMasterId = primaryOrg.BooksMasterId;
            _userClaims.UserClaim.CustomerMasterId = primaryOrg.BooksCustomerMasterId;
            _userClaims.UserClaim.OrganizationName = primaryOrg.Name;
            _userClaims.UserClaim.OrganizationRealPageGuid = primaryOrg.RealPageId;
        }

        var tokenResult = await _credentialRepository.GetActivityTokenAsync(
            setPassword.EnterpriseUserName, setPassword.ActivityToken,
            (int)ActivityType.NewUserRegistrationVerification, primaryOrg.PartyId, cancellationToken);

        if (tokenResult is null || string.IsNullOrEmpty(tokenResult.Token))
        {
            _logger.LogError("{ActionName} - Activity Token is expired for user {UserName}",
                nameof(SetPasswordAsync), setPassword.EnterpriseUserName);
            return SetChangePasswordError(response, "Activity Token is expired.");
        }

        var validation = await ValidatePasswordAsync(new ValidatePassword
        {
            EnterpriseUserName = setPassword.EnterpriseUserName,
            PasswordToValidate = setPassword.NewPassword,
            PartyId = primaryOrgStatus.PartyId,
            PasswordModifiedDate = userLogin.PasswordModifiedDate,
            PasswordHash = userLogin.PasswordHash,
            PasswordSalt = userLogin.PasswordSalt,
            CheckPasswordHistory = true
        }, cancellationToken);

        if (validation.IsError)
            return SetChangePasswordError(response, validation.ErrorReason);

        var newHash = setPassword.NewPassword.PasswordHash();
        var enterpriseUserId = await _credentialRepository.UpdateEnterpriseUserCredentialAsync(
            setPassword.EnterpriseUserName, newHash.PasswordHash, newHash.PasswordSalt,
            setPassword.ActivityToken, (int)ActivityType.NewUserRegistrationVerification,
            primaryOrg.PartyId, cancellationToken);

        var person = await _managePerson.GetPersonAsync(userLogin.RealPageId, cancellationToken);

        if (string.IsNullOrEmpty(enterpriseUserId))
        {
            SetChangePasswordError(response, "Error while updating password.");
            LogActivityInfo(LogActivityTypeConstants.CHANGE_PASSWORD_FAILURE, primaryOrg.PartyId, primaryOrg.BooksMasterId,
                $"User {person.FirstName} {person.LastName} unable to set the password.",
                setPassword.EnterpriseUserName, userLogin.UserId, person, userLogin.RealPageId);
        }
        else
        {
            response.IsSuccess = true;
            response.EnterpriseUserName = setPassword.EnterpriseUserName;
            LogActivityInfo(LogActivityTypeConstants.CHANGE_PASSWORD_SUCCESS, primaryOrg.PartyId, primaryOrg.BooksMasterId,
                $"User {person.FirstName} {person.LastName} successfully set the password.",
                setPassword.EnterpriseUserName, userLogin.UserId, person, userLogin.RealPageId);
        }

        if (primaryOrgStatus.IsPending is true)
        {
            if (!await _manageUserLogin.UpdateActiveUserStatusAsync(userLogin.RealPageId, UserUiStatusType.Active, cancellationToken))
                SetChangePasswordError(response, "Error while updating user status to active.");
        }
        else if (primaryOrgStatus.IsForceReSetPassword is true)
        {
            if (!await _manageUserLogin.CreateUpdateUserStatusAsync(userLogin.RealPageId, UserUiStatusType.ForceResetPassword, cancellationToken))
                SetChangePasswordError(response, "Error while updating user status from forced reset password.");
        }

        return response;
    }

    #endregion

    #region Password Expiration

    /// <inheritdoc/>
    public async Task<CheckPasswordExpirationResponse> CheckPasswordExpirationAsync(
        long userId,
        Guid enterpriseUserRealPageId,
        CancellationToken cancellationToken = default)
    {
        var response = new CheckPasswordExpirationResponse { SeverityLevel = SeverityLevelType.None };

        if (enterpriseUserRealPageId == Guid.Empty)
            return Error(response, "RealPageId for user not supplied.");

        var primaryOrgPartyId = await _userLoginRepository.GetPrimaryOrgIdByUserIdAsync(userId, cancellationToken);
        if (primaryOrgPartyId == 0)
            return Error(response, "Unable to get organization Id for user from claims.");

        var passwordPolicy = await _passwordPolicyRepository.GetPasswordPolicyAsync(primaryOrgPartyId, cancellationToken);
        if (passwordPolicy is null)
            return Error(response, $"Unable to find password policy for organization - {primaryOrgPartyId}");

        if (!passwordPolicy.EnablePasswordExpiration)
            return response;

        var userLogin = await _userLoginRepository.GetUserLoginOnlyAsync(enterpriseUserRealPageId);
        if (userLogin is null)
            return Error(response, "User Name is incorrect or not found.");

        if (userLogin.Is3rdPartyIDP)
        {
            response.IsPasswordExpired = false;
            response.ErrorReason = "Password expiration is not applicable to users on external identity provider";
            return response;
        }

        if (userLogin.PasswordModifiedDate is not null)
        {
            const int starttOfNotice = 10;
            const int startofWarning = 4;
            const int startOfCritical = 1;

            int daysLeft = passwordPolicy.PasswordExpirationPeriodInDays
                - Convert.ToInt32((DateTime.Now - userLogin.PasswordModifiedDate.Value).TotalDays);
            response.DaysToExpire = daysLeft;

            if (daysLeft <= 0)
            {
                response.SeverityLevel = SeverityLevelType.Critical;
                response.IsPasswordExpired = true;
                response.ErrorReason = "User Password is already expired.";
                return response;
            }

            response.SeverityLevel = daysLeft switch
            {
                <= startOfCritical => SeverityLevelType.Critical,
                <= startofWarning => SeverityLevelType.Warning,
                <= starttOfNotice => SeverityLevelType.Information,
                _ => SeverityLevelType.None
            };
        }

        return response;
    }

    #endregion

    #region Security Questions

    /// <inheritdoc/>
    public async Task<UserAllSecurityQuestionResponse> UserAllSecurityQuestionsAsync(
        string enterpriseUserName,
        CancellationToken cancellationToken = default)
    {
        var response = new UserAllSecurityQuestionResponse();

        if (string.IsNullOrEmpty(enterpriseUserName))
            return Error(response, "User Name is not specified.");

        response.EnterpriseUserName = enterpriseUserName;

        var questions = await _credentialRepository.GetAllSecurityQuestionsAsync(enterpriseUserName, cancellationToken);
        if (questions is null || questions.Count == 0)
            return Error(response, "Error while getting security questions.");

        response.SecurityQuestions = questions;
        return response;
    }

    /// <inheritdoc/>
    public async Task<SetUserSecurityQuestionsResponse> SetUserSecurityQuestionsAsync(
        UserSecurityAnswer userSecurityQuestionsAnswers,
        CancellationToken cancellationToken = default)
    {
        var response = new SetUserSecurityQuestionsResponse();

        if (string.IsNullOrEmpty(userSecurityQuestionsAnswers.EnterpriseUserName))
            return Error(response, "No Username specified.");

        var idp = await _credentialRepository.GetIdentityProviderTypeByLoginNameAsync(
            userSecurityQuestionsAnswers.EnterpriseUserName, cancellationToken);
        if (idp is null || !idp.IsLocal)
            return Error(response, "Set security questions is not applicable to users on external identity provider.");

        var answers = userSecurityQuestionsAnswers.SecurityQuestionAnswers;

        if (answers is null || answers.Count == 0)
            return Error(response, "No questions received from user.");
        if (answers.Count != RequiredSecurityQuestionCount)
            return Error(response, "Incorrect number of questions received from user, 3 questions are required.");
        if (answers.Any(a => a.Answer.Trim().Length > MaxAnswerLength))
            return Error(response, $"Answer should be less than {MaxAnswerLength} chars.");

        if (string.IsNullOrEmpty(userSecurityQuestionsAnswers.ActivityToken))
            return Error(response, "Null or empty activity Token.");

        var userLogin = await _userLoginRepository.GetUserLoginOnlyAsync(userSecurityQuestionsAnswers.EnterpriseUserName);
        if (userLogin is null)
            return Error(response, "User name is incorrect or not found.");

        var orgPartyId = await _userLoginRepository.GetPrimaryOrgIdByUserIdAsync(userLogin.UserId, cancellationToken);

        var tokenResult = await _credentialRepository.GetActivityTokenAsync(
            userSecurityQuestionsAnswers.EnterpriseUserName,
            userSecurityQuestionsAnswers.ActivityToken,
            (int)ActivityType.NewUserRegistrationVerification, orgPartyId, cancellationToken);

        if (string.IsNullOrEmpty(tokenResult?.Token))
            return Error(response, "Set Password Activity Token is expired.");

        userSecurityQuestionsAnswers.SecurityQuestionAnswers = GetHashedAnswers(answers);
        await _credentialRepository.SaveSecurityQuestionAnswersAsync(
            userSecurityQuestionsAnswers, orgPartyId, cancellationToken);

        try
        {
            var person = await _managePerson.GetPersonAsync(userLogin.RealPageId, cancellationToken);
            var booksMasterOrgId = await GetDefaultBooksMasterOrgIdForUserAsync(userLogin.RealPageId, cancellationToken);
            LogActivity.WriteActivity(new ActivityDetails
            {
                LogActivityTypeName = LogActivityTypeConstants.CHANGE_SECURITY_QUESTIONS_SUCCESS,
                LogCategoryName = LogActivityCategoryType.Security.ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                BooksMasterOrganizationId = booksMasterOrgId,
                OrganizationPartyId = orgPartyId,
                Message = $"User {person.FirstName} {person.LastName} has successfully set their security questions.",
                FromUserLoginName = userLogin.LoginName,
                FromUserLoginId = userLogin.UserId,
                FromUserFirstName = person.FirstName,
                FromUserLastName = person.LastName,
                FromUserRealpageId = userLogin.RealPageId.ToString(),
                ToUserLoginId = userLogin.UserId,
                ToUserLoginName = userLogin.LoginName
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Activity logging failed in {Method}", nameof(SetUserSecurityQuestionsAsync));
        }

        response.IsSuccess = true;
        response.EnterpriseUserName = userSecurityQuestionsAnswers.EnterpriseUserName;
        return response;
    }

    /// <inheritdoc/>
    public async Task<UsersAllSecurityQuestionResponse> GetUserSelectedSecurityQuestionsAsync(
        Guid realPageId,
        CancellationToken cancellationToken = default)
    {
        var response = new UsersAllSecurityQuestionResponse();

        if (realPageId == Guid.Empty)
            return Error(response, "RealPageId for user not supplied.");

        var result = await _credentialRepository.GetUserSelectedSecurityQuestionsAsync(realPageId, cancellationToken);
        if (result is null || result.Count == 0)
            return Error(response, "User has not set security question.");

        response.SecurityQuestions = result;
        return response;
    }

    /// <inheritdoc/>
    public async Task<SaveUserSelectedSecurityQuestionResponse> SaveUserSelectedSecurityQuestionsAsync(
        Guid realPageId,
        IList<SecurityQuestionAnswer> securityQuestionAnswer,
        CancellationToken cancellationToken = default)
    {
        var response = new SaveUserSelectedSecurityQuestionResponse();

        if (realPageId == Guid.Empty)
            return Error(response, "RealPageId for user not supplied.");

        var userLogin = await _manageUserLogin.GetUserLoginOnlyAsync(realPageId, cancellationToken);
        if (userLogin is null)
            return Error(response, "User Name is incorrect or not found.");

        if (securityQuestionAnswer is null || securityQuestionAnswer.Count == 0)
            return Error(response, "No questions received from user.");
        if (securityQuestionAnswer.Count != RequiredSecurityQuestionCount)
            return Error(response, "Incorrect number of questions received from user.");
        if (securityQuestionAnswer.Any(a => a.Answer.Length > MaxAnswerLength))
            return Error(response, $"Answer should be less than {MaxAnswerLength} chars.");

        var existingSq = await GetUserSelectedSecurityQuestionsAsync(realPageId, cancellationToken);
        var hashedAnswers = GetHashedAnswers(securityQuestionAnswer);

        var result = await _credentialRepository.SaveUserSelectedSecurityQuestionsAsync(
            realPageId, hashedAnswers, cancellationToken);

        if (result == 0)
        {
            LogCredentialActivityWithClaim(LogActivityTypeConstants.CHANGE_SECURITY_QUESTIONS_FAILURE,
                userLogin.LoginName, userLogin.UserId,
                "User {0} {1} unable to change security questions.",
                nameof(SaveUserSelectedSecurityQuestionsAsync));
            return Error(response, "Unable to save security questions.");
        }

        bool isNew = existingSq.SecurityQuestions is null || existingSq.SecurityQuestions.Count == 0;
        LogCredentialActivityWithClaim(LogActivityTypeConstants.CHANGE_SECURITY_QUESTIONS_SUCCESS,
            userLogin.LoginName, userLogin.UserId,
            isNew ? "User {0} {1} has successfully set their security questions."
                  : "User {0} {1} has successfully changed their security questions.",
            nameof(SaveUserSelectedSecurityQuestionsAsync));

        return response;
    }

    #endregion

    #region Misc

    /// <inheritdoc/>
    public async Task<ListResponse> GetUserAsync(
        string enterpriseUserName,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();

        if (string.IsNullOrEmpty(enterpriseUserName))
            return Error(response, "User Name is not specified.");

        var user = await _userLoginRepository.GetUserLoginOnlyAsync(enterpriseUserName);
        if (user is null)
            return Error(response, $"The Username \"{enterpriseUserName}\" is incorrect or was not found.");

        IList<object> list = [user];
        return new ListResponse
        {
            Records = list,
            TotalRows = list.Count,
            RowsPerPage = 9999,
            TotalPages = 1
        };
    }

    /// <inheritdoc/>
    public async Task<IdentityProviderType> GetIdentityProviderTypeByLoginNameAsync(
        string enterpriseUserName,
        CancellationToken cancellationToken = default)
        => await _credentialRepository.GetIdentityProviderTypeByLoginNameAsync(enterpriseUserName, cancellationToken);

    /// <inheritdoc/>
    public async Task<string> GetNewUserRegistrationVerificationTokenAsync(
        long userId,
        Guid realPageId,
        CancellationToken cancellationToken = default)
    {
        var orgPartyId = await _userLoginRepository.GetPrimaryOrgIdByUserIdAsync(userId, cancellationToken);
        return await _credentialRepository.CreateActivityTokenAsync(
            orgPartyId, realPageId, (int)ActivityType.NewUserRegistrationVerification, cancellationToken);
    }

    #endregion

    #region Private Helpers

    private static IList<SecurityQuestion> SelectRandomQuestions(IList<SecurityQuestion> questions)
    {
        if (questions.Count <= MaxRandomQuestions) return questions;

        var rnd = new Random();
        return [.. Enumerable.Range(0, questions.Count)
            .OrderBy(_ => rnd.Next())
            .Take(MaxRandomQuestions)
            .Select(i => questions[i])];
    }

    private static IList<SecurityQuestionAnswer> GetHashedAnswers(IList<SecurityQuestionAnswer> answers)
    {
        foreach (var qa in answers)
            qa.Answer = qa.Answer.Trim().ToUpper().Sha256();
        return answers;
    }

    private async Task<long> GetDefaultBooksMasterOrgIdForUserAsync(
        Guid realPageId, CancellationToken cancellationToken)
    {
        var orgs = await _credentialRepository.ListOrganizationByRealPageIdAsync(realPageId, cancellationToken);
        return orgs?.Count > 0 ? orgs[0].BooksMasterId : 0;
    }

    private void LogCredentialActivityWithClaim(
        string activityType, string fromLoginName, long fromLoginId,
        string messageFormat, string stepName)
    {
        try
        {
            LogActivity.WriteActivity(new ActivityDetails
            {
                LogActivityTypeName = activityType,
                LogCategoryName = LogActivityCategoryType.Security.ToString(),
                CorrelationId = _userClaims.CorrelationId.ToString(),
                BooksMasterOrganizationId = _userClaims.OrganizationMasterId,
                OrganizationPartyId = _userClaims.OrganizationPartyId,
                Message = string.Format(messageFormat, _userClaims.FirstName, _userClaims.LastName),
                FromUserLoginName = fromLoginName,
                FromUserLoginId = fromLoginId,
                FromUserFirstName = _userClaims.FirstName,
                FromUserLastName = _userClaims.LastName,
                FromUserRealpageId = _userClaims.UserRealPageGuid.ToString(),
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Activity logging failed in {Step}", stepName);
        }
    }

    private void LogActivityInfo(
        string activityType, long orgPartyId, long booksMasterOrgId,
        string message, string loginName, long userId,
        IPerson person, Guid realPageId)
    {
        try
        {
            LogActivity.WriteActivity(new ActivityDetails
            {
                LogActivityTypeName = activityType,
                LogCategoryName = LogActivityCategoryType.Security.ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                BooksMasterOrganizationId = booksMasterOrgId,
                OrganizationPartyId = orgPartyId,
                Message = message,
                FromUserLoginName = loginName,
                FromUserLoginId = userId,
                FromUserFirstName = person.FirstName,
                FromUserLastName = person.LastName,
                FromUserRealpageId = realPageId.ToString(),
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Activity logging failed");
        }
    }

    // ── Response error helpers ────────────────────────────────────────────

    //private static T Error<T>(T response, string reason) where T : BaseResponse
    //{
    //    response.IsError = true;
    //    response.ErrorReason = reason;
    //    return response;
    //}
    private static T Error<T>(T response, string reason)
    {
        dynamic r = response;
        r.IsError = true;
        r.ErrorReason = reason;
        return response;
    }
    private static ChangePasswordResponse SetChangePasswordError(ChangePasswordResponse r, string reason)
    {
        r.IsError = true;
        r.IsSuccess = false;
        r.ErrorReason = reason;
        return r;
    }

    private static SecurityAnswerResponse ErrorAnswer(
        SecurityAnswerResponse r, string reason, bool answersCorrect = false)
    {
        r.IsError = true;
        r.ErrorReason = reason;
        r.IsAnswersCorrect = answersCorrect;
        return r;
    }

    private static ResetPasswordResponse Error(ResetPasswordResponse r, string reason)
    {
        r.IsError = true;
        r.ErrorReason = reason;
        return r;
    }

    #endregion
}