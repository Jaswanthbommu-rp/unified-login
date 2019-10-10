using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Logic
{
    public class ManageUserLoginIdentity : IManageUserLoginIdentity
    {
        #region Private Variables
        IUserLoginRepository _userLoginRepository;
        private IManagePersona _managePersona;
        #endregion

        #region Constructors
        /// <summary>
        /// ManageUserLogin Constructor
        /// </summary>
        /// <param name="userLoginRepository">UserLogin Repository</param>
        /// <param name="managePersona">Manage Persona</param>
        public ManageUserLoginIdentity(IUserLoginRepository userLoginRepository, IManagePersona managePersona)
        {
            _userLoginRepository = userLoginRepository;
            _managePersona = managePersona;
        }

        /// <summary>
        /// Create a basic instance of the ManageUser Controller class
        /// </summary>
        public ManageUserLoginIdentity()
        {
            _userLoginRepository = new UserLoginRepository();
            _managePersona = new ManagePersona();
        }
        #endregion

        /// <summary>
        /// Get Authenticated External User
        /// </summary>
        /// <param name="authUserDetails">Auth User Details</param>
        /// <returns>Authenticate User Response object</returns>
        //public AuthenticateUserResponse GetAuthenticatedExternalUser(AuthUserDetails authUserDetails)
        //{
        //    var authenticateUserResponse = new AuthenticateUserResponse();
        //    if (string.IsNullOrEmpty(authUserDetails.EnterpriseUserName))
        //    {
        //        throw new Exception("Invalid parameter enterpriseUserName.");
        //    }
        //
        //    UserLoginOnly user = _userLoginRepository.GetUserLoginOnly(authUserDetails.EnterpriseUserName);
        //    if (user == null || user.UserId == 0)
        //    {
        //        authenticateUserResponse.ErrorReason = "Your Username is incorrect.";
        //        authenticateUserResponse.IsError = true;
        //        return authenticateUserResponse;
        //    }
        //
        //    authenticateUserResponse.UserLogin = user;
        //
        //    // get the org id for the company the user should be logging in under
        //    long orgPartyId = 0;
        //
        //    var persona = _managePersona.GetActivePersona(user.RealPageId);
        //    user.PersonaId = persona.PersonaId;
        //
        //    IManageOrganization organizationLogic = new ManageOrganization();
        //    var organizationList = organizationLogic.ListOrganizationByEnterpriseUserId(user.RealPageId, null);
        //
        //    if (organizationList?.Count > 0)
        //    {
        //        orgPartyId = organizationList.FirstOrDefault(p => p.PrimaryOrganization).PartyId;
        //    }
        //
        //    // update activity after successful login
        //    _userLoginRepository.UpdateUserActivityAttempts(authUserDetails.EnterpriseUserName, ActivityType.LoginSuccess, authUserDetails.UserDeviceDetails, orgPartyId, null);
        //
        //    return authenticateUserResponse;
        //}

        /// <summary>
        /// Get Authenticated User
        /// </summary>
        /// <param name="authUserDetails">Auth User Details object</param>
        /// <param name="validateUsingPassword">Should the users password be used to validate login</param>
        /// <returns>Authenticate User Response object</returns>
        public AuthenticateUserResponse GetAuthenticatedUser(AuthUserDetails authUserDetails, bool validateUsingPassword)
        {
            var authenticateUserResponse = new AuthenticateUserResponse();
            //var userLoginRepository = new UserLoginRepository();

            if (string.IsNullOrEmpty(authUserDetails.EnterpriseUserName))
            {
                authenticateUserResponse.IsError = true;
                authenticateUserResponse.ErrorReason = "No Username specified.";
                return authenticateUserResponse;
            }

            if (validateUsingPassword && string.IsNullOrEmpty(authUserDetails.Password))
            {
                authenticateUserResponse.IsError = true;
                authenticateUserResponse.ErrorReason = "No password specified.";
                return authenticateUserResponse;
            }

            UserLoginOnly user = _userLoginRepository.GetUserLoginOnly(authUserDetails.EnterpriseUserName);

            if (user == null || user.UserId == 0)
            {
                // update fake user tried to login ??? Why we need it when there is no user
                authenticateUserResponse.ErrorReason = "Your Username" + (validateUsingPassword ? "/Password" : "") + " is incorrect.";
                authenticateUserResponse.IsError = true;
                return authenticateUserResponse;
            }

            //var persona = managePersona.GetActivePersona(user.RealPageId);
            //user.PersonaId = persona.PersonaId;

            //IManageOrganization organizationLogic = new ManageOrganization();
            //var organizationList = userLoginRepository.ListOrganizationByLoginName(authUserDetails.EnterpriseUserName);

            OrganizationStatus primaryOrgStatus = GetUserOrganizationStatus(user.UserId, user.LastLogin, 0, true);
            //var organizationList = _userLoginRepository.ListOrganizationStatusByUserId(user.UserId);

            //var organizationList = organizationLogic.ListOrganizationByEnterpriseUserId(user.RealPageId, null);
            //OrganizationStatus primaryOrgStatus = organizationList.FirstOrDefault(p => p.PrimaryOrganization);

            if (primaryOrgStatus == null)
            {
                authenticateUserResponse.ErrorReason = "Your account is inactive.";
                authenticateUserResponse.IsError = true;
                return authenticateUserResponse;
            }

            //primaryOrgStatus.SetOrganizationStatus(user.LastLogin != null);
            // get the org id for the company the user should be logging in under
            //long orgPartyId = primaryOrgStatus.PartyId;

            //Check for lock status
            if (primaryOrgStatus.StatusTypeId == (int) UserUiStatusType.Locked && primaryOrgStatus.StatusThruDate != null && primaryOrgStatus.StatusThruDate.Value < DateTime.UtcNow)
            {
                primaryOrgStatus.IsLocked = false;
                // reset attempt count
                // update user status back to active once lockout time ended
                _userLoginRepository.UpdateUserStatusByCompany(user.RealPageId, primaryOrgStatus.PartyId, (int) UserDbStatusType.Active, DateTime.UtcNow, null);

                _userLoginRepository.UpdateUserActivityAttempts(authUserDetails.EnterpriseUserName, ActivityType.LoginSuccess, authUserDetails.UserDeviceDetails, primaryOrgStatus.PartyId, null);
            }

            if (primaryOrgStatus.FromDate != DateTime.MinValue && primaryOrgStatus.FromDate > DateTime.UtcNow)// DateTime.UtcNow)
            {
                authenticateUserResponse.ErrorReason = "Your account is not effective and inactive.";
                authenticateUserResponse.IsError = true;
                return authenticateUserResponse;
            }

            if (primaryOrgStatus.ThruDate != null && primaryOrgStatus.ThruDate < DateTime.UtcNow)
            {
                authenticateUserResponse.ErrorReason = "You account is expired and inactive.";
                authenticateUserResponse.IsError = true;
                return authenticateUserResponse;
            }

            if (primaryOrgStatus.IsActive == false && primaryOrgStatus.Status == UserUiStatusType.Disabled)
            {
                authenticateUserResponse.ErrorReason = "The user is not active in the system.";
                authenticateUserResponse.IsError = true;
                return authenticateUserResponse;
            }

            if (primaryOrgStatus.IsExpired.HasValue && primaryOrgStatus.IsExpired.Value)
            {
                authenticateUserResponse.ErrorReason = "Your account is in expired state. Please complete account creation by getting new email invitation.";
                authenticateUserResponse.IsError = true;
                return authenticateUserResponse;
            }

            if (primaryOrgStatus.IsLocked.HasValue && primaryOrgStatus.IsLocked.Value)
            {
                authenticateUserResponse.ErrorReason = "Your account is locked.";
                authenticateUserResponse.IsError = true;
                return authenticateUserResponse;
            }

            if (primaryOrgStatus.IsTainted.HasValue && primaryOrgStatus.IsTainted.Value)
            {
                authenticateUserResponse.ErrorReason = "User account is blocked.";
                authenticateUserResponse.IsError = true;
                return authenticateUserResponse;
            }
            
            var activityAttemptExceeds = _userLoginRepository.GetActivityAttemptExceeds(primaryOrgStatus.PartyId, authUserDetails.EnterpriseUserName, (int)ActivityType.Login);
			var forceLockSettings = _userLoginRepository.GetActivityAttemptExceeds(primaryOrgStatus.PartyId, authUserDetails.EnterpriseUserName, (int)ActivityType.ForceLocked);
			if (activityAttemptExceeds != null)
            {
                // This cond occurs when user exceeds last chance to enter correct password 
                if (activityAttemptExceeds.AttemptCount >= activityAttemptExceeds.MaxActivitycount)
                {					
					// insert lock status for user; should not come to this step as user will rejected in previous isLocked condition added for additional cross check
					_userLoginRepository.UpdateUserStatusByCompany(user.RealPageId, primaryOrgStatus.PartyId, (int)UserDbStatusType.Locked, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(forceLockSettings.ActivityTokenExpirationMinutes)) ;

                    authenticateUserResponse.ErrorReason = "Max attempts to login exceeded. Your account is locked.";
                    authenticateUserResponse.IsError = true;
                    return authenticateUserResponse;
                }
            }

            if (validateUsingPassword && string.IsNullOrEmpty(user.PasswordSalt))
            {
                authenticateUserResponse.ErrorReason = "Attempt to log in user with no defined password";
                authenticateUserResponse.IsError = true;
                return authenticateUserResponse;
            }

            if (!validateUsingPassword || (user.PasswordHash == authUserDetails.Password.PasswordHashBySalt(Convert.FromBase64String(user.PasswordSalt))))
            {
                // Reset activity count
                _userLoginRepository.UpdateUserActivityAttempts(authUserDetails.EnterpriseUserName, ActivityType.LoginSuccess, authUserDetails.UserDeviceDetails, primaryOrgStatus.PartyId, null);

                // remove password hash from user object
                user.PasswordHash = "";
                user.PasswordSalt = "";
                authenticateUserResponse.IsError = false;
                authenticateUserResponse.ErrorReason = string.Empty;
                authenticateUserResponse.UserLogin = user;

                return authenticateUserResponse;
            }

            // good user but wrong password
            _userLoginRepository.UpdateUserActivityAttempts(authUserDetails.EnterpriseUserName, ActivityType.Login, authUserDetails.UserDeviceDetails, primaryOrgStatus.PartyId, null);
            
            //Get activity attempts after save
            var activityAttempts = _userLoginRepository.GetActivityAttemptExceeds(primaryOrgStatus.PartyId, authUserDetails.EnterpriseUserName, (int)ActivityType.Login);
            // After allowed failed password attempts, warn the user if the next attempt is not successful their account will be locked for {Activity Token Expiration Minutes} minutes.
            var errorMessage = "Your Username/Password is incorrect.";

            // This condition occurs when user fails last chance to enter correct password - 1
            if (activityAttempts != null && activityAttempts.AttemptCount == activityAttempts.MaxActivitycount - 1)
            {
                errorMessage = $"Your Username/Password is incorrect. If the next attempt is not successful your account will be locked for {forceLockSettings.ActivityTokenExpirationMinutes} minutes.";
            }

            if (activityAttempts != null && activityAttempts.AttemptCount == activityAttempts.MaxActivitycount)
            {
                errorMessage = "Max attempts to login exceeded. Your account is locked.";
                _userLoginRepository.UpdateUserStatusByCompany(user.RealPageId, primaryOrgStatus.PartyId, (int) UserDbStatusType.Locked, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(forceLockSettings.ActivityTokenExpirationMinutes));
            }

            authenticateUserResponse.UserLogin = null;
            authenticateUserResponse.ErrorReason = errorMessage;
            authenticateUserResponse.IsError = true;
            return authenticateUserResponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="lastLogin"></param>
        /// <param name="companyPartyId"></param>
        /// <param name="getPrimaryOrg"></param>
        /// <returns></returns>
        public OrganizationStatus GetUserOrganizationStatus(long userId, DateTime? lastLogin, long companyPartyId, bool getPrimaryOrg)
        {
            var organizationList = _userLoginRepository.ListOrganizationWithoutStatusByUserId(userId);

            //var organizationList = organizationLogic.ListOrganizationByEnterpriseUserId(user.RealPageId, null);
            OrganizationStatus primaryOrgStatus = null;
            primaryOrgStatus = getPrimaryOrg ? organizationList.FirstOrDefault(p => p.PrimaryOrganization) : organizationList.FirstOrDefault(p => p.PartyId == companyPartyId);
            //if (primaryOrgStatus == null)
            //{
            //    authenticateUserResponse.ErrorReason = "Your account is inactive.";
            //    authenticateUserResponse.IsError = true;
            //    return authenticateUserResponse;
            //}
            primaryOrgStatus?.SetOrganizationStatus(lastLogin != null);

            return primaryOrgStatus;
        }

        /// <summary>
        /// Get User Statues
        /// </summary>
        //public IList<UserStatus> GetUserStatuses(Guid realPageId)
        //{
        //    if (realPageId == Guid.Empty)
        //    {
        //        throw new Exception("Invalid parameter realPageId.");
        //    }

        //    return _userLoginRepository.GetUserStatuses(realPageId);
        //}
    }
}