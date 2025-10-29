using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IManageCredential
    {
        /// <summary>
        /// Change forgot Password  
        /// </summary>
        ChangePasswordResponse ForgotPassword(ChangePassword changePassword);

        /// <summary>
        /// Get Security Question
        /// </summary>
        SecurityQuestionResponse GetSecurityQuestion(string enterpriseUserName, UserDeviceDetails userDeviceDetails);

        /// <summary>
        /// Verify Security Answers
        /// </summary>
        SecurityAnswerResponse VerifySecurityAnswers(UserSecurityAnswer userSecurityAnswer, UserDeviceDetails userDeviceDetails);

        /// <summary>
        /// Reset Password
        /// </summary>
        ResetPasswordResponse ResetPassword(Guid realPageId, UserResetPassword userResetPassword);

        /// <summary>
        /// Validate Password
        /// </summary> 
        ValidatePasswordResponse ValidatePasswordForUser(string enterpriseUserName, string passwordToValidate);

        /// <summary>
        /// Validate Password
        /// </summary> 
        ValidatePasswordResponse ValidatePassword(ValidatePassword validatePassword);

        /// <summary>
        /// Check Password Expiration
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="enterpriseUserRealPageId">User unique identifier</param>
        /// <returns>Check Password Expiration Response</returns>
        CheckPasswordExpirationResponse CheckPasswordExpiration(long userId, Guid enterpriseUserRealPageId);

        /// <summary>
        /// Returns all Security Questions for user (Custom questions - TBD + System default questions)
        /// </summary>
        UserAllSecurityQuestionResponse UserAllSecurityQuestions(string enterpriseUserName);

        /// <summary>
        /// Returns User
        /// </summary>
        ListResponse GetUser(string enterpriseUserName);

        /// <summary>
        /// Set Password
        /// </summary>
        ChangePasswordResponse SetPassword(SetPassword setPassword);

        /// <summary>
        /// Set User Security Questions
        /// </summary>
        SetUserSecurityQuestionsResponse SetUserSecurityQuestions(UserSecurityAnswer userSecurityQuestionsAnswers);

        /// <summary>
        /// 
        /// </summary>
        UsersAllSecurityQuestionResponse GetUserSelectedSecurityQuestions(Guid realpageId);

        /// <summary>
        /// 
        /// </summary>
        SaveUserSelectedSecurityQuestionResponse SaveUserSelectedSecurityQuestions(Guid realpageId, IList<SecurityQuestionAnswer> securityQuestionAnswer);

        /// <summary>
        ///  Gets Identity Provider By EnterpriseUserName
        /// </summary>
        /// <param name="enterpriseUserName"></param>
        /// <returns></returns>
        IdentityProviderType GetIdentityProviderTypeByLoginName(string enterpriseUserName);

        /// <summary>
        /// Get New User Registration Verification Token Details 
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="realPageId"></param>
        /// <returns>Verification Token</returns>
        string GetNewUserRegistrationVerificationToken(long userId, Guid realPageId);

    }
}