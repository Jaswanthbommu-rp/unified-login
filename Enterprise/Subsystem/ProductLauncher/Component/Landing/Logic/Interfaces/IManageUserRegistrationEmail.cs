using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
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
    }
}