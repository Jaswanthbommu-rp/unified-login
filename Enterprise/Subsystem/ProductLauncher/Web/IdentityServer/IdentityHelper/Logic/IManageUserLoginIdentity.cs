using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Logic
{
    /// <summary>
    /// Interface for IManageUserLoginIdentity
    /// </summary>
    public interface IManageUserLoginIdentity
    {
        /// <summary>
        /// Authenticate User
        /// </summary>
        //AuthenticateUserResponse GetAuthenticatedUser(AuthUserDetails authUserDetails);

        AuthenticateUserResponse GetAuthenticatedUser(AuthUserDetails authUserDetails, bool validateUsingPassword);

        OrganizationStatus GetUserOrganizationStatus(long userId, DateTime? lastLogin, long companyPartyId, bool getPrimaryOrg);
        /// <summary>
        /// Get User Statues
        /// </summary>
        //IList<UserStatus> GetUserStatuses(Guid realPageId);
        /// <summary>
        /// Authenticat External User
        /// </summary>
        //AuthenticateUserResponse GetAuthenticatedExternalUser(AuthUserDetails authUserDetails);

    }
}