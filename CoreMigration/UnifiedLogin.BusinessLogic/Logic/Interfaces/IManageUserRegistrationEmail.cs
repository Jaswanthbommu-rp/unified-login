using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
    /// <summary>
	/// Interface for ManageManageUserRegistrationEmail
	/// </summary>
    public interface IManageUserRegistrationEmail
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        bool SendNewUserRegistrationEmail(IProfileDetail profile);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userLoginOnly"></param>
        /// <param name="companyName"></param>
        /// <param name="userTypeId"></param>
        /// <param name="organizationPartyId"></param>
        /// <returns></returns>
        bool SendNewUserRegistrationEmail(UserLoginOnly userLoginOnly, string companyName, int userTypeId, long organizationPartyId);

        /// <summary>
        /// Used to send the password reset email to the given user
        /// </summary>
        /// <param name="profileDetail"></param>
        /// <returns></returns>
        bool SendPasswordResetEmail(ProfileDetail profileDetail);
    }
}