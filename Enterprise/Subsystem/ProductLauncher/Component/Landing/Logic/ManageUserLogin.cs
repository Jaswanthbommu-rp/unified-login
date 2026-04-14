using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Messaging;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Kafka;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.EmployeeAccess;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Web;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    /// <summary>
    /// Manage UserLogin repository calls
    /// </summary>
    public class ManageUserLogin : IManageUserLogin
    {
        #region Private Variables
        private readonly IUserLoginRepository _userLoginRepository;
        private readonly ICredentialRepository _credentialRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IRoleTypeRepository _roleTypeRepository;
        private readonly IPersonRepository _personRepository;
        private readonly DefaultUserClaim _defaultUserClaim;
        private readonly IManagePersona _managePersona;

        private static readonly Guid EmployeeCompanyRealPageId = new Guid("0D018E46-C20E-477D-ADED-4E5A35FB8F99");
        #endregion

        #region Constructors
        /// <summary>
        /// Unit test constructor v2
        /// </summary>
        public ManageUserLogin(IRepository repository, DefaultUserClaim userClaim, HttpMessageHandler messageHandler)
        {
            _userLoginRepository = new UserLoginRepository(repository);
            _credentialRepository = new CredentialRepository(repository);
            _userRepository = new UserRepository(repository, userClaim, messageHandler);
            _personRepository = new PersonRepository(repository);
            _roleTypeRepository = new RoleTypeRepository(repository);
            _organizationRepository = new OrganizationRepository(repository);
            _managePersona = new ManagePersona(repository, userClaim, messageHandler);
            _defaultUserClaim = userClaim;
        }

        /// <summary>
        /// Create a basic instance of the ManageUser Controller class
        /// </summary>
        public ManageUserLogin()
        {
            _userLoginRepository = new UserLoginRepository();
            _credentialRepository = new CredentialRepository();
            _userRepository = new UserRepository();
            _personRepository = new PersonRepository();
            _roleTypeRepository = new RoleTypeRepository();
            _organizationRepository = new OrganizationRepository();
            _managePersona = new ManagePersona();
        }

        /// <summary>
        /// Create a basic instance of the ManageUser Controller class
        /// </summary>
        /// <param name="userClaim"></param>
        public ManageUserLogin(DefaultUserClaim userClaim)
        {
            _userLoginRepository = new UserLoginRepository();
            _credentialRepository = new CredentialRepository();
            _userRepository = new UserRepository(userClaim);
            _personRepository = new PersonRepository();
            _roleTypeRepository = new RoleTypeRepository();
            _organizationRepository = new OrganizationRepository(userClaim);
            _defaultUserClaim = userClaim;
        }

        #endregion

        #region Public ManageUserLogin methods
        /// <summary>
        /// Create a Person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="userLogin">UserLogin object of the parameter values</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse CreateUserLogin(Guid realPageId, IUserLogin userLogin)
        {
            if (userLogin == null)
            {
                throw new ArgumentNullException(nameof(userLogin), "Null UserLogin.");
            }

            if (realPageId == Guid.Empty)
            {
                throw new Exception("Invalid parameter realPageId.");
            }
            // if password exist then get hash & salt
            if (!string.IsNullOrEmpty(userLogin.Password))
            {
                var pwd = userLogin.Password.PasswordHash();
                userLogin.PasswordHash = pwd.PasswordHash;
                userLogin.PasswordSalt = pwd.PasswordSalt;
            }
            return _userLoginRepository.CreateUserLogin(realPageId, userLogin);
        }

        /// <summary>
        /// GetUserLogin
        /// </summary>
        /// <param name="realPageId"></param>
        /// <param name="orgPartyId"></param>
        /// <returns>UserLogin with statuses</returns>
        public UserLogin GetUserLogin(Guid realPageId, long orgPartyId)
        {
            if (realPageId == Guid.Empty)
            {
                throw new Exception("Missing RealPage Id.");
            }

            return GetUserLogin(realPageId, orgPartyId, null, null);
        }

        /// <summary>
        /// GetUserLogin
        /// </summary>
        /// <param name="realPageId"></param>
        /// <returns>UserLogin without statuses</returns>
        public UserLoginOnly GetUserLoginOnly(Guid realPageId)
        {
            if (realPageId == Guid.Empty)
            {
                throw new Exception("Missing user realpage id.");
            }
            return _userLoginRepository.GetUserLoginOnly(realPageId);
        }

        /// <summary>
        /// GetUserLogin
        /// </summary>
        /// <param name="enterpriseUserName"></param>
        /// <returns>UserLogin without statuses</returns>
        public UserLoginOnly GetUserLoginOnly(string enterpriseUserName)
        {
            if (string.IsNullOrEmpty(enterpriseUserName))
            {
                throw new Exception("Missing login name.");
            }

            return _userLoginRepository.GetUserLoginOnly(enterpriseUserName); ;
        }

        /// <summary>
        /// GetUserLogin
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>UserLogin without statuses</returns>
        public UserLoginOnly GetUserLoginOnly(long userId)
        {
            if (userId == 0)
            {
                throw new Exception("Missing user Id.");
            }

            return _userLoginRepository.GetUserLoginOnly(userId); ;
        }

        /// <summary>
        /// GetUserLogin
        /// </summary>
        /// <param name="userLogin"></param>
        /// <param name="orgPartyId"></param>
        /// <param name="userStatuses"></param>
        /// <returns>UserLogin with statuses</returns>
        public UserLogin GetUserLogin(UserLogin userLogin, long orgPartyId, IList<UserStatus> userStatuses)
        {
            if (userLogin == null)
            {
                throw new Exception("Missing user login.");
            }

            if (userStatuses == null)
            {
                throw new Exception("Missing user statuses.");
            }

            return GetUserLogin(Guid.Empty, orgPartyId, userLogin, userStatuses);
        }

        /// <summary>
        /// GetUserLogin
        /// </summary>
        /// <param name="userLogin"></param>
        /// <param name="orgPartyId"></param>
        /// <returns>UserLogin with statuses</returns>
        public UserLogin GetUserLogin(UserLogin userLogin, long orgPartyId)
        {
            if (userLogin == null)
            {
                throw new Exception("Missing user login.");
            }

            return GetUserLogin(Guid.Empty, orgPartyId, userLogin, null);
        }

        /// <summary>
        /// Get UserLogin
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="orgPartyId">The company being used</param>
        /// <param name="userLogin"></param>
        /// <param name="userStatuses"></param>
        /// <returns>UserLogin object including statuses</returns>
        private UserLogin GetUserLogin(Guid realPageId, long orgPartyId, UserLogin userLogin = null, IList<UserStatus> userStatuses = null)
        {
            // TODO figure out how to fix this
            if (realPageId == Guid.Empty && userLogin == null && userStatuses == null)
            {
                throw new Exception("Missing RealPage Id.");
            }

            if (realPageId == Guid.Empty && userLogin == null)
            {
                throw new Exception("Missing user login.");
            }

            if (realPageId != Guid.Empty)
            {
                userLogin = _userLoginRepository.GetUserLogin(realPageId, orgPartyId);
            }
            if (userLogin != null)
            {
                if (realPageId != Guid.Empty && userLogin.UserRoleType == null && userLogin.RealPageId != Guid.Empty)
                {
                    // the caller didn't already get the userroletype, so go get it
                    IList<Organization> organizationList = _userLoginRepository.ListOrganizationByEnterpriseUserId(userLogin.RealPageId, "User Type");
                    Organization orgUserType = organizationList.FirstOrDefault(o => o.PartyId == orgPartyId && o.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase));
                    if (orgUserType != null)
                    {
                        string userType = orgUserType.RoleNameFrom;
                        userType = Regex.Replace(userType, @"[^A-Za-z0-9]+", "").ToUpper();
                        UserRoleType userRoleType;

                        if (Enum.TryParse(userType, true, out userRoleType))
                        {
                            userLogin.UserRoleType = userRoleType;
                        }
                    }
                    else
                    {
                        Log.Write(LogEventLevel.Error, "{ActionName} - {state}", new object[] { "GetUserLogin", $"No user type for UserId={userLogin.UserId}" });
                    }
                }

                userLogin.IsSuperUser = (userLogin.UserRoleType == UserRoleType.SuperUser) ? true : false;
                if (realPageId == Guid.Empty && userLogin != null)
                {
                    userLogin = UserLoginStatus.SetUserLoginStatus(userLogin);
                }
            }
            return userLogin;
        }

        /// <summary>
        /// Update an existing UserLogin
        /// </summary>
        /// <param name="username">Username</param>        
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse UpdateLastLogin(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new Exception("Invalid parameter username.");
            }

            return _userLoginRepository.UpdateLastLogin(username);
        }

        /// <summary>
        /// Create or Update User status  
        /// </summary>
        public bool CreateUpdateUserStatus(Guid realPageId, UserUiStatusType uiStatusTypeName)//, DateTime fromUtcDateTime,DateTime? thruUtcDateTime)
        {
            if (realPageId == null)
            {
                throw new ArgumentNullException(nameof(realPageId), "Null realPageId.");
            }

            DateTime fromUtcDateTime = DateTime.UtcNow;
            DateTime? thruUtcDateTime = null; // default for AccountCreationSuccessful; Unlocked; Active
            int statusTypeId = (int)MapUiStatusToDb(uiStatusTypeName);
            bool? isNotified = null;
            bool newUserWithFeatureDate = false;
            bool isUserExpired = false;
            bool newUserwithActiveStatus = false;
            bool sendUserStatusEvent = false;
            UserDetails userDetailsInfo = new UserDetails();
            UserLoginOnly userLoginOnly = null;
            OrganizationStatus orgStatus = new OrganizationStatus();

            // get NewUserRegistration activity exp time
            var newUserRegistrationActivity = GetActivities(_defaultUserClaim.OrganizationPartyId);
            if (newUserRegistrationActivity != null)
                thruUtcDateTime = fromUtcDateTime.AddMinutes(newUserRegistrationActivity.ActivityTokenExpirationMinutes);

            // set max date if inputted status is locked (from UI) 
            if (uiStatusTypeName == UserUiStatusType.Locked)
            {
                thruUtcDateTime = DateTime.MaxValue;
            }

            if (uiStatusTypeName == UserUiStatusType.Unlocked)
            {
                thruUtcDateTime = null;
                statusTypeId = (int)UserUiStatusType.Active;
            }
            // set thruDate less than current date if status is disabled or Account Creation Expired
            if (uiStatusTypeName == UserUiStatusType.Expired)
            {
                thruUtcDateTime = DateTime.UtcNow.AddMinutes(-1);
                statusTypeId = (int)UserUiStatusType.Expired;
            }
            if (uiStatusTypeName == UserUiStatusType.Disabled)
            {
                thruUtcDateTime = null;
                statusTypeId = (int)UserUiStatusType.Disabled;
                sendUserStatusEvent = true;
            }
            //If Disabled user activated by admin from user list page set thrudate to null
            if (uiStatusTypeName == UserUiStatusType.Active)
            {
                userLoginOnly = _userLoginRepository.GetUserLoginOnly(realPageId);
                var userLogin = GetUserLogin(realPageId, _defaultUserClaim.OrganizationPartyId); // keep for now
                sendUserStatusEvent = true;
                //TODO - Need to register audit activity with previous thrudate and reason why we are setting null for disabled to active status
                if (userLoginOnly != null)
                {
                    orgStatus = _userLoginRepository.GetUserOrganizationWithStatus(userLoginOnly.UserId, userLoginOnly.LastLogin, _defaultUserClaim.OrganizationPartyId, false);

                    if (orgStatus.ThruDate != null)
                    {
                        if (DateTime.UtcNow > orgStatus.ThruDate)
                        {
                            isUserExpired = true;
                        }
                    }
                    if (orgStatus.StatusThruDate != null)
                    {
                        if (DateTime.UtcNow > orgStatus.StatusThruDate)
                        {
                            isUserExpired = true;
                        }
                    }

                    if (userLoginOnly.LastLogin == null && userLoginOnly.PasswordModifiedDate != null && !isUserExpired)
                        newUserwithActiveStatus = true;


                    fromUtcDateTime = orgStatus.FromDate;
                    orgStatus.ThruDate = new DateTime(9999, 12, 31);
                    if (orgStatus.FromDate > DateTime.UtcNow)
                    {
                        DateTime fromDate = DateTime.UtcNow;
                        orgStatus.FromDate = fromDate;
                        newUserWithFeatureDate = true;
                    }

                    UserLogin ul = new UserLogin() { LoginName = userLoginOnly.LoginName, PasswordHash = userLoginOnly.PasswordHash, PasswordSalt = userLoginOnly.PasswordSalt, FromDate = orgStatus.FromDate, ThruDate = orgStatus.ThruDate };

                    RepositoryResponse response = _userLoginRepository.UpdateUserLogin(realPageId, ul, _defaultUserClaim.OrganizationPartyId);
                    //user changed to feature from date or user never logged in and status changing from disabled to active and not previously locked
                    if (orgStatus.PrimaryOrganization && (newUserWithFeatureDate || (userLoginOnly.LastLogin == null && !userLoginOnly.Is3rdPartyIDP && orgStatus.Status != UserUiStatusType.Locked)) && !newUserwithActiveStatus)
                    {
                        IManageUserRegistrationEmail manageUserRegistrationEmail = new ManageUserRegistrationEmail(_defaultUserClaim);
                        isNotified = manageUserRegistrationEmail.SendNewUserRegistrationEmail(userLoginOnly, orgStatus.Name, (int)userLogin.UserRoleType, orgStatus.PartyId);
                        statusTypeId = (int)UserUiStatusType.Pending;
                    }
                    else
                    {
                        statusTypeId = (int)UserUiStatusType.Active;
                        thruUtcDateTime = null;
                    }

                    IList<UserLoginOnly> userLoginOnlyList = new List<UserLoginOnly>();
                    userLoginOnlyList.Add(userLoginOnly);

                    _userRepository.ActivateSalesForceUser(_defaultUserClaim.UserRealPageGuid, _defaultUserClaim.PersonaId, userLoginOnlyList, true);
                }
            }

            if (uiStatusTypeName == UserUiStatusType.Unlocked)
            {
                // Get Userlogin to pass LoginName
                var userLogin = _userLoginRepository.GetUserLoginOnly(realPageId);
                // reset login attempt count
                _credentialRepository.UpdateUserActivityAttempts(userLogin.LoginName, ActivityType.UnlockUser, null, _defaultUserClaim.OrganizationPartyId, null);
            }

            _userRepository.UpdateUserStatusByCompany(realPageId, _defaultUserClaim.OrganizationPartyId, statusTypeId, fromUtcDateTime, thruUtcDateTime);

            if (uiStatusTypeName == UserUiStatusType.Active || uiStatusTypeName == UserUiStatusType.Disabled || uiStatusTypeName == UserUiStatusType.Locked || uiStatusTypeName == UserUiStatusType.Unlocked)
            {
                var userLogin = _userLoginRepository.GetUserLoginOnly(realPageId);
                AddActivityLog(userLogin, uiStatusTypeName.ToString(), ProductEnum.UnifiedPlatform.ToEnumDescription(), _defaultUserClaim);

                if (orgStatus.PrimaryOrganization && (newUserWithFeatureDate || (userLoginOnly != null && (userLoginOnly.LastLogin == null && !userLoginOnly.Is3rdPartyIDP) && orgStatus.Status != UserUiStatusType.Locked)) && !newUserwithActiveStatus)
                {
                    string message = string.Empty;
                    var userRepository = new UserRepository(_defaultUserClaim);
                    userDetailsInfo = userRepository.GetUserDetails(userRealPageId: realPageId.ToString());
                    IProfileDetail profile = new ProfileDetail();
                    profile.FirstName = userDetailsInfo.FirstName;
                    profile.LastName = userDetailsInfo.LastName;
                    profile.userLogin.LoginName = userDetailsInfo.LoginName;
                    profile.userLogin.UserId = userDetailsInfo.UserId;
                    profile.userLogin.RealPageId = userDetailsInfo.UserRealPageId;
                    if (isNotified == true)
                    {
                        message = "Welcome Email sent to user {0} {1} by user {2}.";
                        LogAuditActivity(LogActivityTypeConstants.EMAIL_SENT, LogActivityCategoryType.Email, message, "UpdateUser", profile);
                    }
                    else if (isNotified == false)
                    {
                        message = "Unable to Resend Welcome Email to user {0} {1} by user {2}.";
                        LogAuditActivity(LogActivityTypeConstants.EMAIL_RESENT, LogActivityCategoryType.Email, message, "UpdateUser", profile);
                    }
                }
            }

            if (sendUserStatusEvent)
            {
                if (userDetailsInfo != null && string.IsNullOrEmpty(userDetailsInfo.LoginName))
                {          
                    Persona persona = _managePersona.ListPersona(realPageId).Where(c => c.OrganizationPartyId == _defaultUserClaim.OrganizationPartyId ).FirstOrDefault();
                    userDetailsInfo = _userRepository.GetUserDetails(personaId: persona != null ? persona.PersonaId: 0 );
                }
                if (userDetailsInfo != null)
                {
                    IUserLoginPersonaRepository userLoginPersonaRepository = new UserLoginPersonaRepository();
                    IList<UserLoginPersona> userLoginPersonaList = userLoginPersonaRepository.ListUserLoginPersona(userLoginPersonaId: null, userLoginId: userDetailsInfo.UserId, organizationPartyId: userDetailsInfo.OrganizationPartyId);
                    var primaryOrgPersona = userLoginPersonaList.Where(x => x.PrimaryOrganization == true).FirstOrDefault();
                    if (primaryOrgPersona != null && userDetailsInfo != null
                        && userDetailsInfo.UserRoleTypeId != UserTypeConstants.RegularUserNoEmail
                        && !userDetailsInfo.IsRPEmployee
                        && !userDetailsInfo.LoginName.Equals($"{userDetailsInfo.BooksMasterId}admin@realpage.com", StringComparison.OrdinalIgnoreCase))

                    {
                        var kafkaProducer = KafkaProducerServiceFactory.Instance;
                        kafkaProducer.PublishUserStatusChangeEventAsync(new UnifiedLoginUserStatusEvent
                        {
                            persona_id = userDetailsInfo.PersonaId,
                            user_login_name = userDetailsInfo.LoginName,
                            is_active = uiStatusTypeName == UserUiStatusType.Active,
                            user_activation_deactivation_date = DateTime.UtcNow
                        }).ConfigureAwait(false);
                    }
                }
            }
            return true;
        }

        /// <summary>
        ///Active user Update User status  
        /// </summary>
        public bool UpdateActiveUserStatus(Guid realPageId, UserUiStatusType uiStatusTypeName)
        {
            if (realPageId == null)
            {
                throw new ArgumentNullException(nameof(realPageId), "Null realPageId.");
            }

            if (uiStatusTypeName == UserUiStatusType.Active)
            {
                //Get User login

                var userLoginlogic = new ManageUserLogin(_defaultUserClaim);
                var userLogin = userLoginlogic.GetUserLoginOnly(realPageId);

                if (userLogin != null)
                {
                    // TODO refactor this call
                    IList<OrganizationStatus> orgList = _userLoginRepository.ListOrganizationWithoutStatusByUserId(userLogin.UserId);
                    OrganizationStatus currentOrg = orgList.FirstOrDefault(p => p.RealPageId == _defaultUserClaim.OrganizationRealPageGuid);

                    if (currentOrg == null)
                    {
                        // the call may be anonymous, so take the first company found in the list as default
                        currentOrg = orgList[0];
                        if (_defaultUserClaim.PersonaId == 0)
                        {
                            // get the default persona id for this company for this user as well
                            ManagePersona mp = new ManagePersona();
                            var persona = mp.GetFirstAvailablePersonaByCompany(realPageId, currentOrg.PartyId);
                            _defaultUserClaim.PersonaId = persona.PersonaId;
                        }
                    }
                    IList<UserLoginOnly> ul = new List<UserLoginOnly> { userLogin };

                    _userRepository.UpdateUserStatusByCompany(userLogin.RealPageId, currentOrg.PartyId, (int)UserUiStatusType.Active, currentOrg.FromDate, null);
                    if (_defaultUserClaim.PersonaId != 0)
                    {
                        _userRepository.ActivateSalesForceUser(_defaultUserClaim.UserRealPageGuid, _defaultUserClaim.PersonaId, ul, true);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>		
        /// <param name="userLogins"></param> 	
        /// <param name="isCalledFromService"></param> 
        /// <returns></returns>
        public bool ResendInvitation(IList<UserLogin> userLogins, bool isCalledFromService = false)
        {
            try
            {
                DefaultUserClaim defaultUserClaim = GetDefaultUserClaim();
                foreach (var user in userLogins)
                {
                    //Get User Profile
                    var message = "";
                    var profileLogic = new ManageProfile(_defaultUserClaim);
                    IProfileDetail profileDetail = new ProfileDetail();
                    profileDetail = profileLogic.GetProfileDetail(user.RealPageId, defaultUserClaim.OrganizationPartyId);
                    // Get Userlogin to pass Data
                    var userLogin = profileDetail.userLogin;

                    if (userLogin != null && userLogin.UserRoleType != UserRoleType.UserNoEmail && (userLogin.IsExpired.Value || userLogin.IsPending.Value || isCalledFromService))
                    {
                        int statusTypeId = (int)MapUiStatusToDb(UserUiStatusType.Pending);

                        var userFromDate = userLogin.FromDate.Value;
                        DateTime thruUtcDateTime = userFromDate.AddHours(72);
                        var newUserRegistrationActivity = GetActivities(_defaultUserClaim.OrganizationPartyId);
                        thruUtcDateTime = newUserRegistrationActivity != null ? DateTime.UtcNow.Date.AddMinutes(newUserRegistrationActivity.ActivityTokenExpirationMinutes) : thruUtcDateTime;

                        //update user login
                        RepositoryResponse response = _userLoginRepository.UpdateUserLogin(userLogin.RealPageId, userLogin, defaultUserClaim.OrganizationPartyId);
                        //Send email notification
                        IManageUserRegistrationEmail manageUserRegistrationEmail = new ManageUserRegistrationEmail(_defaultUserClaim);
                        bool isNotified = manageUserRegistrationEmail.SendNewUserRegistrationEmail(profileDetail);

                        var userName = _defaultUserClaim.LoginName?.Length == 0 ? " notification service" : (!string.IsNullOrEmpty(_defaultUserClaim.ImpersonatedByName) ? " RealPage Access (" + _defaultUserClaim.ImpersonatedByName + ") " : _defaultUserClaim.FirstName + " " + _defaultUserClaim.LastName);

                        if (isNotified)
                        {
                            if (isCalledFromService)
                            {
                                message = $"Welcome Email send to user {profileDetail.userLogin.LoginName} by user {userName}.";
                            }
                            else
                            {
                                message = $"Resent Welcome Email to user {profileDetail.userLogin.LoginName} by user {userName}.";
                            }
                        }
                        else
                        {
                            message = $"Unable to send Welcome Email to user {profileDetail.userLogin.LoginName} by user {userName}.";
                        }
                        //Log Activity
                        LogActivity.WriteActivity(new ActivityDetails
                        {
                            LogActivityTypeName = isCalledFromService == true ? LogActivityTypeConstants.EMAIL_SENT : LogActivityTypeConstants.EMAIL_RESENT,
                            LogCategoryName = LogActivityCategoryType.Email.ToString(),
                            CorrelationId = defaultUserClaim.CorrelationId.ToString(),
                            BooksMasterOrganizationId = defaultUserClaim.OrganizationMasterId,
                            OrganizationPartyId = defaultUserClaim.OrganizationPartyId,
                            Message = message,

                            FromUserLoginName = userName,
                            FromUserLoginId = defaultUserClaim.UserId,
                            FromUserRealpageId = _defaultUserClaim.UserRealPageGuid.ToString(),
                            FromUserFirstName = _defaultUserClaim.FirstName,
                            FromUserLastName = _defaultUserClaim.LastName,

                            ToUserLoginName = profileDetail.userLogin.LoginName,
                            ToUserLoginId = profileDetail.userLogin.UserId,
                            ToUserFirstName = profileDetail.FirstName,
                            ToUserLastName = profileDetail.LastName,
                            ToUserRealpageId = profileDetail.userLogin.RealPageId.ToString()
                        });
                        //set to pending status
                        //DateTime fromDate = DateTime.UtcNow.Date;
                        _userRepository.UpdateUserStatusByCompany(userLogin.RealPageId, defaultUserClaim.OrganizationPartyId, statusTypeId, userFromDate, thruUtcDateTime);
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(LogEventLevel.Error, ex, "{ActionName} - {state}", new object[] { "ResendInvitation", ex.Message });
                return false;
            }
        }

        /// <summary>
        /// Used to reset the password and send an email to the request user with the given RealPageId
        /// </summary>
        /// <param name="realPageId"></param>
        /// <returns></returns>
        public bool ClearPasswordAndQuestions(Guid realPageId)
        {
            var manageProfile = new ManageProfile(_defaultUserClaim);
            var profileDetail = (ProfileDetail)manageProfile.GetProfileDetail(realPageId, _defaultUserClaim.OrganizationPartyId);
            var userLogin = profileDetail.userLogin;
            bool emailSentSuccessfully = false;

            if (userLogin != null && userLogin.UserRoleType != UserRoleType.UserNoEmail)
            {
                // clear the user password and security questions
                var spResponse = _credentialRepository.ResetEnterpriseUserCredential(realPageId, null, null, _defaultUserClaim.OrganizationPartyId);
                if (spResponse.Id == 0)
                {
                    LogResetPasswordActivity(false, profileDetail);
                    return false;
                }
                DateTime thruUtcDateTime = DateTime.UtcNow.Date.AddHours(72);
                var newUserRegistrationActivity = GetActivities(_defaultUserClaim.OrganizationPartyId);
                thruUtcDateTime = newUserRegistrationActivity != null ? DateTime.UtcNow.Date.AddMinutes(newUserRegistrationActivity.ActivityTokenExpirationMinutes) : thruUtcDateTime;
                userLogin.PasswordHash = null;
                userLogin.PasswordSalt = null;

                //update user login
                RepositoryResponse response = _userLoginRepository.UpdateUserLogin(realPageId, userLogin, _defaultUserClaim.OrganizationPartyId);
                //Send email notification
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    LogResetPasswordActivity(false, profileDetail);
                    return false;
                }
                var manageUserRegistrationEmail = new ManageUserRegistrationEmail(_defaultUserClaim);
                emailSentSuccessfully = manageUserRegistrationEmail.SendPasswordResetEmail(profileDetail);

                if (emailSentSuccessfully)
                {
                    //set to pending status
                    int statusTypeId = (int)MapUiStatusToDb(UserUiStatusType.Pending);

                    _userRepository.UpdateUserStatusByCompany(userLogin.RealPageId, _defaultUserClaim.OrganizationPartyId, statusTypeId, userLogin.FromDate.Value, thruUtcDateTime);
                }
            }

            LogResetPasswordActivity(emailSentSuccessfully, profileDetail);
            return emailSentSuccessfully;
        }

        private void LogResetPasswordActivity(bool isSuccess, ProfileDetail profileDetail)
        {
            string message;
            string editorName = (_defaultUserClaim.ImpersonatedBy == Guid.Empty) ? $"{_defaultUserClaim.FirstName} {_defaultUserClaim.LastName}" : " RealPage Access (" + _defaultUserClaim.ImpersonatedByName + ") ";

            message = isSuccess
                ? $"{editorName} successfully initiated password reset for {profileDetail.FirstName} {profileDetail.LastName}."
                : $"An exception occurred when {editorName} was updating the password for {profileDetail.FirstName} {profileDetail.LastName}.";

            //Log Activity
            LogActivity.WriteActivity(new ActivityDetails
            {
                LogActivityTypeName = LogActivityTypeConstants.EMAIL_RESETPASSWORDSENT,
                LogCategoryName = LogActivityCategoryType.Email.ToString(),
                CorrelationId = _defaultUserClaim.CorrelationId.ToString(),
                BooksMasterOrganizationId = _defaultUserClaim.OrganizationMasterId,
                OrganizationPartyId = _defaultUserClaim.OrganizationPartyId,
                Message = message,
                BooksProductCode = "UPFM",

                FromUserLoginName = _defaultUserClaim.LoginName,
                FromUserLoginId = _defaultUserClaim.UserId,
                FromUserRealpageId = _defaultUserClaim.UserRealPageGuid.ToString(),
                FromUserFirstName = _defaultUserClaim.FirstName,
                FromUserLastName = _defaultUserClaim.LastName,

                ToUserLoginName = profileDetail.userLogin.LoginName,
                ToUserLoginId = profileDetail.userLogin.UserId,
                ToUserFirstName = profileDetail.FirstName,
                ToUserLastName = profileDetail.LastName,
                ToUserRealpageId = profileDetail.userLogin.RealPageId.ToString()
            });
        }

        /// <summary>
        /// Used to fix the primary org for a user if it needs to be activated based on the status type and dates
        /// </summary>
        /// <param name="userLogin"></param>
        /// <param name="lastLoginDate"></param>
        /// <param name="userTypeId"></param>
        /// <param name="manageProfile"></param>
        /// <returns></returns>
        private OrganizationStatus CheckPrimaryOrganizationStatus(UserLoginOnly userLogin, DateTime? lastLoginDate, int userTypeId, ManageProfile manageProfile, DefaultUserClaim adminUserClaim)
        {
            var logData = new Dictionary<string, object>();
            var primaryOrgStatus = _userLoginRepository.GetUserOrganizationWithStatus(userLogin.UserId, lastLoginDate, 0, true);
            var organization = _organizationRepository.GetOrganization(realPageId: primaryOrgStatus.RealPageId);

            logData = new Dictionary<string, object> { { "userLogin", userLogin } };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logData, null, new object[] { "ManageUserLogin.CheckPrimaryOrganizationStatus", "Beginning of method for user with json" });

            if ((primaryOrgStatus.StatusTypeId == (int)UserUiStatusType.Pending || primaryOrgStatus.StatusTypeId == (int)UserUiStatusType.ForceResetPassword) &&
                primaryOrgStatus.StatusThruDate != null &&
                primaryOrgStatus.StatusThruDate < DateTime.UtcNow &&
                !userLogin.Is3rdPartyIDP)
            {
                _userRepository.UpdateUserStatusByCompany(userLogin.RealPageId, primaryOrgStatus.PartyId, (int)UserUiStatusType.Expired, primaryOrgStatus.FromDate, null);
                DefaultUserClaim currentUserClaim = GetCurrentUserClaim(manageProfile, organization);
                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "ManageUserLogin.CheckPrimaryOrganizationStatus", $"Calling AddActivityLog - pending users who are not activated before status thru date then update status as expire - status type - {UserUiStatusType.Expired.ToString()}, user {userLogin.LoginName} ,PrimaryOrgStatus.StatusThruDate : {primaryOrgStatus.StatusThruDate}, Now : {DateTime.UtcNow}, PrimaryOrgStatus.PartyId : {primaryOrgStatus.PartyId}" });
                AddActivityLog(userLogin, UserUiStatusType.Expired.ToString(), ProductEnum.UnifiedPlatform.ToEnumDescription(), currentUserClaim);
            }

            if (primaryOrgStatus.StatusTypeId == (int)UserUiStatusType.Disabled && DateTimeOffset.UtcNow.Date == primaryOrgStatus.FromDate.Date && lastLoginDate == null)
            {
                int statusTypeId = (int)UserUiStatusType.Pending;
                string statusType = UserUiStatusType.Pending.ToString();
                DateTime? thruUtcDateTime = primaryOrgStatus.FromDate.AddHours(72);
                var userFromDate = primaryOrgStatus.FromDate;
                var userFromDateCST = TimeZoneInfo.ConvertTime(Convert.ToDateTime(userFromDate), TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
                var newUserRegistrationActivity = GetActivities(organization.PartyId);
                thruUtcDateTime = newUserRegistrationActivity != null ? DateTime.UtcNow.Date.AddMinutes(newUserRegistrationActivity.ActivityTokenExpirationMinutes) : thruUtcDateTime;

                if (userLogin.Is3rdPartyIDP || primaryOrgStatus.IsActive.Value)
                {
                    statusTypeId = (int)UserUiStatusType.Active;
                    statusType = UserUiStatusType.Active.ToString();
                    thruUtcDateTime = null;
                }
                else if (userTypeId != UserTypeConstants.RegularUserNoEmail)
                {
                    IManageUserRegistrationEmail manageUserRegistrationEmail = new ManageUserRegistrationEmail(_defaultUserClaim);
                    bool isNotified = manageUserRegistrationEmail.SendNewUserRegistrationEmail(userLoginOnly: userLogin, companyName: organization.Name, userTypeId: userTypeId, organizationPartyId: organization.PartyId);
                    var message = "";
                    var userName = _defaultUserClaim.LoginName?.Length == 0 ? " notification service" : userLogin.LoginName;
                    if (isNotified)
                    {
                        message = $"Welcome Email sent to user {userLogin.LoginName} by automated system.";
                    }
                    else
                    {
                        message = $"Unable to send Welcome Email to user {userLogin.LoginName} by automated system.";
                    }
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "ManageUserLogin.CheckPrimaryOrganizationStatus", $"Email notification - {message}" });
                    if (organization.RealPageId != DefaultUserClaim.ExternalCompanyRealPageId)
                    {
                        LogActivity.WriteActivity(new ActivityDetails
                        {
                            LogActivityTypeName = LogActivityTypeConstants.EMAIL_SENT,
                            LogCategoryName = LogActivityCategoryType.Email.ToString(),
                            CorrelationId = _defaultUserClaim.CorrelationId.ToString(),
                            BooksMasterOrganizationId = organization.BooksMasterId,
                            OrganizationPartyId = organization.PartyId,
                            Message = message,
                            FromUserLoginName = "automatedsystem",
                            FromUserLoginId = adminUserClaim.UserId,
                            ToUserLoginName = userLogin.LoginName,
                            ToUserLoginId = userLogin.UserId
                        });
                    }
                }

                _userRepository.UpdateUserStatusByCompany(userLogin.RealPageId, primaryOrgStatus.PartyId, statusTypeId, userFromDate, thruUtcDateTime);
                if (organization.RealPageId != DefaultUserClaim.ExternalCompanyRealPageId)
                {
                    DefaultUserClaim currentUserClaim = GetCurrentUserClaim(manageProfile, organization);
                    string message = "{0} {1} was activated by the system due to the scheduled User Effective date. | " + userFromDateCST.ToShortDateString() + "/ " + userFromDateCST.ToShortTimeString() + " CST";
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "ManageUserLogin.CheckPrimaryOrganizationStatus", $"Calling AddActivityLog - Future user and user never logged in for status type - {statusType}" });
                    AddActivityLog(userLogin, statusType, ProductEnum.UnifiedPlatform.ToEnumDescription(), currentUserClaim, message);
                }
                primaryOrgStatus = _userLoginRepository.GetUserOrganizationWithStatus(userLogin.UserId, lastLoginDate, 0, true);
            }

            return primaryOrgStatus;
        }

        /// <summary>
        /// Gets claims by GUID, for user that requested to resend an activation link
        /// </summary>
        /// <param name="userRealPageId"></param>
        /// <returns></returns>
        public DefaultUserClaim GetUserClaimsFromNonUser(Guid userRealPageId)
        {
            DefaultUserClaim currentUserClaim = GetDefaultUserClaim();
            //IManagePerson _managePerson = new ManagePerson();
            var profileLogic = new ManageProfile(_defaultUserClaim);

            // Get Userlogin to pass Data
            var userLogin = _userLoginRepository.GetUserLoginOnly(userRealPageId);
            var orgWithoutStatus = _userLoginRepository.GetPrimaryOrgWithoutStatusByUserId(userLogin.UserId);

            if (orgWithoutStatus == null)
            {
                return null;
            }
            var org = _organizationRepository.GetOrganization(realPageId: orgWithoutStatus.RealPageId);

            if (org == null)
            {
                return null;
            }

            var orgWithStatus = _userLoginRepository.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, orgWithoutStatus.PartyId, false);

            if (orgWithStatus == null)
            {
                return null;
            }
            var orgList = _userLoginRepository.ListOrganizationByEnterpriseUserId(userRealPageId, null);

            //Get User Profile			
            var profileDetail = profileLogic.GetProfileDetail(userRealPageId, orgWithStatus.PartyId);

            //since windows service doesn't have editor persona,Get RealPageEmployeeAccessID to use in to get editor persona
            currentUserClaim = GetCurrentUserClaim(profileLogic, org);

            return currentUserClaim;

        }

        /// <summary>
        /// 
        /// </summary>		
        /// <param name="userLogins"></param> 			
        /// <returns></returns>
        public bool ProcessFutureUserLogins(IList<ProcessUserLogin> userLogins)
        {
            try
            {
                var logData = new Dictionary<string, object>();
                DefaultUserClaim currentUserClaim = GetDefaultUserClaim();
                //IManagePerson _managePerson = new ManagePerson();
                var profileLogic = new ManageProfile(_defaultUserClaim);
                logData = new Dictionary<string, object> { { "userLogins", userLogins } };
                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logData, null, new object[] { "ManageUserLogin.ProcessFutureUserLogins", "Beginning of method for user with json" });
                foreach (var user in userLogins)
                {
                    // Get Userlogin to pass Data
                    var userLogin = _userLoginRepository.GetUserLoginOnly(user.UserRealPageId);
                    var org = _organizationRepository.GetOrganization(realPageId: user.OrganizationRealPageId);

                    var orgStatus = _userLoginRepository.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, org.PartyId, false);
                    var orgList = _userLoginRepository.ListOrganizationByEnterpriseUserId(user.UserRealPageId, null);

                    logData = new Dictionary<string, object> { { "userLoginOnly", userLogin }, { "userOrganizationList", orgList } };
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logData, null, new object[] { "ManageUserLogin.ProcessFutureUserLogins", $"Getting info for process future user with login name {userLogin.LoginName} and user realpageId {user.UserRealPageId}" });

                    if (orgStatus != null)
                    {
                        //Get User Profile			
                        var profileDetail = profileLogic.GetProfileDetail(user.UserRealPageId, org.PartyId);

                        //since windows service doesn't have editor persona,Get RealPageEmployeeAccessID to use in to get editor persona
                        currentUserClaim = GetCurrentUserClaim(profileLogic, org);

                        //check primary Org status
                        var primaryOrgStatus = CheckPrimaryOrganizationStatus(userLogin, userLogin.LastLogin, profileDetail.UserTypeId, profileLogic, currentUserClaim);

                        if (userLogin.LoginName != null && org.PartyId != primaryOrgStatus.PartyId)
                        {
                            //pending users who are not activated before status thru date,then expire them
                            if ((orgStatus.StatusTypeId == (int)UserUiStatusType.Pending || orgStatus.StatusTypeId == (int)UserUiStatusType.ForceResetPassword) &&
                                orgStatus.StatusThruDate != null &&
                                orgStatus.StatusThruDate < DateTime.UtcNow &&
                                !userLogin.Is3rdPartyIDP)
                            {
                                _userRepository.UpdateUserStatusByCompany(userLogin.RealPageId, orgStatus.PartyId, (int)UserUiStatusType.Expired, orgStatus.FromDate, null);
                                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "ManageUserLogin.ProcessFutureUserLogins", $"Calling AddActivityLog - pending users who are not activated before status thru date then update status as expire - status type - {UserUiStatusType.Expired.ToString()}" });
                                AddActivityLog(userLogin, UserUiStatusType.Expired.ToString(), ProductEnum.UnifiedPlatform.ToEnumDescription(), currentUserClaim);
                            }

                            //Feature user (with disabled state) and user never logged in
                            if (orgStatus.StatusTypeId == (int)UserUiStatusType.Disabled && orgStatus.FromDate.Subtract(DateTime.UtcNow).TotalMinutes <= 15 && userLogin.LastLogin == null)
                            {
                                int statusTypeId = (int)UserUiStatusType.Pending;
                                string statusType = UserUiStatusType.Pending.ToString();
                                DateTime? thruUtcDateTime = orgStatus.FromDate.AddHours(72);
                                var userFromDate = orgStatus.FromDate;
                                var userFromDateCST = TimeZoneInfo.ConvertTime(Convert.ToDateTime(userFromDate), TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
                                var newUserRegistrationActivity = GetActivities(org.PartyId);
                                thruUtcDateTime = newUserRegistrationActivity != null ? DateTime.UtcNow.Date.AddMinutes(newUserRegistrationActivity.ActivityTokenExpirationMinutes) : thruUtcDateTime;

                                if (userLogin.Is3rdPartyIDP)
                                {
                                    statusTypeId = (int)UserUiStatusType.Active;
                                    statusType = UserUiStatusType.Active.ToString();
                                    thruUtcDateTime = null;
                                }
                                else if (profileDetail.UserTypeId != UserTypeConstants.RegularUserNoEmail)
                                {
                                    if (primaryOrgStatus.IsActive.Value)
                                    {
                                        statusTypeId = (int)UserUiStatusType.Active;
                                        statusType = UserUiStatusType.Active.ToString();
                                        thruUtcDateTime = null;
                                    }

                                    IManageUserRegistrationEmail manageUserRegistrationEmail = new ManageUserRegistrationEmail(_defaultUserClaim);
                                    bool isNotified = manageUserRegistrationEmail.SendNewUserRegistrationEmail(userLoginOnly: userLogin, companyName: profileDetail.organization[0].Name, userTypeId: profileDetail.UserTypeId, organizationPartyId: profileDetail.organization[0].PartyId);
                                    var message = "";
                                    var userName = _defaultUserClaim.LoginName?.Length == 0 ? " notification service" : userLogin.LoginName;
                                    if (isNotified)
                                    {
                                        message = $"Welcome Email sent to user {profileDetail.userLogin.LoginName} by automated system.";
                                    }
                                    else
                                    {
                                        message = $"Unable to send Welcome Email to user {profileDetail.userLogin.LoginName} by automated system.";
                                    }
                                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "ManageUserLogin.ProcessFutureUserLogins", $"Email notification - {message}" });
                                    LogActivity.WriteActivity(new ActivityDetails
                                    {
                                        LogActivityTypeName = LogActivityTypeConstants.EMAIL_SENT,
                                        LogCategoryName = LogActivityCategoryType.Email.ToString(),
                                        CorrelationId = _defaultUserClaim.CorrelationId.ToString(),
                                        BooksMasterOrganizationId = org.BooksMasterId,
                                        OrganizationPartyId = org.PartyId,
                                        Message = message,
                                        FromUserLoginName = "automatedsystem",
                                        FromUserLoginId = currentUserClaim.UserId,
                                        ToUserLoginName = profileDetail.userLogin.LoginName,
                                        ToUserLoginId = profileDetail.userLogin.UserId
                                    });
                                }
                                _userRepository.UpdateUserStatusByCompany(userLogin.RealPageId, org.PartyId, statusTypeId, userFromDate, thruUtcDateTime);
                                if (statusTypeId == (int)UserUiStatusType.Active || statusTypeId == (int)UserUiStatusType.Disabled)
                                {
                                    UserDetails userDetailsInfo = new UserDetails();                               
                                    IManagePersona managePersona = new ManagePersona();
                                    Persona persona = managePersona.ListPersona(userLogin.RealPageId).Where(c => c.OrganizationPartyId == _defaultUserClaim.OrganizationPartyId).FirstOrDefault();
                                    userDetailsInfo = _userRepository.GetUserDetails(personaId: persona.PersonaId);

                                    IUserLoginPersonaRepository userLoginPersonaRepository = new UserLoginPersonaRepository();
                                    IList<UserLoginPersona> userLoginPersonaList = userLoginPersonaRepository.ListUserLoginPersona(userLoginPersonaId: null, userLoginId: userDetailsInfo.UserId, organizationPartyId: userDetailsInfo.OrganizationPartyId);
                                    var primaryOrgPersona = userLoginPersonaList.Where(x => x.PrimaryOrganization == true).FirstOrDefault();
                                    if (primaryOrgPersona != null && userDetailsInfo != null
                                        && userDetailsInfo.UserRoleTypeId != UserTypeConstants.RegularUserNoEmail
                                        && !userDetailsInfo.IsRPEmployee
                                        && !userDetailsInfo.LoginName.Equals($"{userDetailsInfo.BooksMasterId}admin@realpage.com", StringComparison.OrdinalIgnoreCase))

                                    {
                                        var kafkaProducer = KafkaProducerServiceFactory.Instance;
                                        kafkaProducer.PublishUserStatusChangeEventAsync(new UnifiedLoginUserStatusEvent
                                        {
                                            persona_id = userDetailsInfo.PersonaId,
                                            user_login_name = userDetailsInfo.LoginName,
                                            is_active = statusTypeId == (int)UserUiStatusType.Active,
                                            user_activation_deactivation_date = DateTime.UtcNow
                                        }).ConfigureAwait(false);
                                    }
                                }


                                string activityMessage = "{0} {1} was activated by the system due to the scheduled User Effective date. | " + userFromDateCST.ToShortDateString() + "/ " + userFromDateCST.ToShortTimeString() + " CST";
                                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "ManageUserLogin.ProcessFutureUserLogins", $"Calling AddActivityLog - Future user and user never logged in for status type - {statusType}" });
                                AddActivityLog(userLogin, statusType, ProductEnum.UnifiedPlatform.ToEnumDescription(), currentUserClaim, activityMessage);
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Write(LogEventLevel.Error, ex, "{ActionName} - {state}", new object[] { "ResendInvitation", ex.Message });
                return false;
            }
        }

        private DefaultUserClaim GetCurrentUserClaim(ManageProfile profileLogic, Organization org)
        {
            DefaultUserClaim currentUserClaim;
            Guid realPageEmployeeAccessID = _organizationRepository.GetOrganizationAdminUserRealPageId(org.RealPageId);

            if (realPageEmployeeAccessID != Guid.Empty)
            {
                var adminUserLogin = _userLoginRepository.GetUserLoginOnly(realPageEmployeeAccessID);
                var adminProfileDetail = profileLogic.GetProfileDetail(realPageEmployeeAccessID, org.PartyId);

                currentUserClaim = new DefaultUserClaim()
                {
                    OrganizationMasterId = org.BooksMasterId,
                    OrganizationPartyId = org.PartyId,
                    FirstName = adminProfileDetail.FirstName,
                    LastName = adminProfileDetail.LastName,
                    UserId = Convert.ToInt32(adminUserLogin.UserId),
                    LoginName = adminUserLogin.LoginName,
                    UserRealPageGuid = adminUserLogin.RealPageId,
                    CorrelationId = _defaultUserClaim.CorrelationId
                };
            }
            else
            {
                currentUserClaim = new DefaultUserClaim()
                {
                    OrganizationMasterId = org.BooksMasterId,
                    OrganizationPartyId = org.PartyId,
                    FirstName = "Automated",
                    LastName = "System",
                    UserId = 1,
                    LoginName = "automatedsystem",
                    UserRealPageGuid = Guid.Empty,
                    CorrelationId = _defaultUserClaim.CorrelationId
                };
            }

            return currentUserClaim;
        }

        private Activity GetActivities(long organizationPartyId)
        {
            var actvityDetail = _credentialRepository.GetActivities(organizationPartyId);
            var activity = actvityDetail.FirstOrDefault(x => x.ActivityTypeId == (int)ActivityType.NewUserRegistration);
            return activity;
        }

        /// <summary>
        /// Check if username is already existing
        /// </summary>
        /// <param name="realPageId">User RealPageId</param>
        /// <param name="enterpriseUsername">Username</param>
        public bool ValidateUsername(Guid realPageId, string enterpriseUsername)
        {
            if (realPageId == null)
            {
                throw new ArgumentNullException(nameof(realPageId), "RealPageId is required.");
            }

            if (string.IsNullOrWhiteSpace(enterpriseUsername))
            {
                throw new ArgumentException(nameof(realPageId), "Username is required.");
            }

            var currentUserLogin = _userLoginRepository.GetUserLoginOnly(realPageId);

            if (!enterpriseUsername.Equals(currentUserLogin.LoginName, StringComparison.OrdinalIgnoreCase))
            {
                var checkLoginName = _userLoginRepository.GetUserLoginOnly(enterpriseUsername);
                if (checkLoginName != null)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// list of Organization By Enterprise User Id
        /// </summary>
        /// <param name="realPageId">Unique Identifier - EnterpriseUserId</param>
        /// <param name="relationshipType">Parties Relationship type name (Optional)</param>
        /// <returns>List of Organization</returns>
        public IList<Organization> ListOrganizationByEnterpriseUserId(Guid realPageId, string relationshipType = null)
        {
            if (realPageId == Guid.Empty)
            {
                throw new Exception("Invalid parameter realPageId.");
            }

            return _userLoginRepository.ListOrganizationByEnterpriseUserId(realPageId, relationshipType);
        }

        private UserDbStatusType MapUiStatusToDb(UserUiStatusType uiStatusTypeName)
        {
            UserDbStatusType dbStatusType = 0;
            switch (uiStatusTypeName.ToString())
            {
                case "AccountCreationPending":
                case "Pending":
                case "Expired":
                case "AccountCreationExpired":
                case "AccountCreationSuccessful":
                    dbStatusType = UserDbStatusType.Pending;
                    break;
                case "Active":
                case "Disabled":
                    dbStatusType = UserDbStatusType.Active;
                    break;
                case "Locked":
                case "Unlocked":
                    dbStatusType = UserDbStatusType.Locked;
                    break;
                case "ForceResetPassword":
                    dbStatusType = UserDbStatusType.ForceResetPassword;
                    break;
            }

            return dbStatusType;
        }

        /// <summary>
        /// Update an existing UserLogin
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="userLogin">UserLogin object of the parameter values</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse UpdateUserLogin(Guid realPageId, IUserLogin userLogin)
        {
            if (userLogin == null)
            {
                throw new ArgumentNullException(nameof(userLogin), "Null UserLogin.");
            }

            if (realPageId == Guid.Empty)
            {
                throw new Exception("Invalid parameter realPageId.");
            }
            // if password exist then get hash & salt
            if (!string.IsNullOrEmpty(userLogin.Password))
            {
                var pwd = userLogin.Password.PasswordHash();
                userLogin.PasswordHash = pwd.PasswordHash;
                userLogin.PasswordSalt = pwd.PasswordSalt;
            }

            // update status for a user
            UserUiStatusType userLoginStatusType = UserUiStatusType.Active;

            if (userLogin.IsActive.HasValue && userLogin.IsActive.Value == true)
            {
                userLoginStatusType = UserUiStatusType.Active;
            }
            else if (userLogin.IsActive.HasValue && userLogin.IsActive.Value == false)
            {
                userLoginStatusType = UserUiStatusType.Disabled;
            }
            else if (userLogin.IsActive.HasValue && userLogin.IsLocked.Value == true)
            {
                userLoginStatusType = UserUiStatusType.Locked;
            }
            else if (userLogin.IsActive.HasValue && userLogin.IsLocked.Value == false)
            {
                userLoginStatusType = UserUiStatusType.Unlocked;
            }

            if (userLogin.RealPageId != Guid.Empty)
            {
                CreateUpdateUserStatus(userLogin.RealPageId, userLoginStatusType);
            }

            return _userLoginRepository.UpdateUserLogin(realPageId, userLogin, _defaultUserClaim.OrganizationPartyId);
        }

        /// <summary>
        /// 
        /// </summary>		
        /// <param name="userLogins"></param> 		
        /// <param name="userLoginStatusType"></param>
        /// <returns></returns>
        public IList<RepositoryResponse> UpdateUserLogins(IList<UserLogin> userLogins, UserUiStatusType userLoginStatusType)
        {
            if (userLogins == null)
            {
                throw new ArgumentNullException(nameof(userLogins), "Null userLogins.");
            }

            IList<RepositoryResponse> responses = new List<RepositoryResponse>();

            foreach (UserLogin userLogin in userLogins)
            {
                // update status for each user
                bool result = CreateUpdateUserStatus(userLogin.RealPageId, userLoginStatusType);

                if (result)
                {
                    RepositoryResponse resp = new RepositoryResponse();
                    resp.Id = userLogin.UserId;
                    responses.Add(resp);
                }
            }

            return responses;
        }


        /// <summary>
        /// UpdateBulkUserLogins
        /// </summary>		
        /// <param name="userLogins"></param> 		
        /// <param name="userLoginStatusType"></param>
        /// <returns></returns>
        public RepositoryResponse UpdateBulkUserLogins(IList<UserLoginOnly> userLogins, UserUiStatusType userLoginStatusType)
        {
            IUserLoginPersonaRepository userLoginPersonaRepository = new UserLoginPersonaRepository();
            RepositoryResponse response = new RepositoryResponse();
            if (userLogins == null)
            {
                throw new ArgumentNullException(nameof(userLogins), "Null userLogins.");
            }

            DateTime fromUtcDateTime = DateTime.UtcNow;
            DateTime? thruUtcDateTime = null; // default for AccountCreationSuccessful; Unlocked; Active
            OrganizationStatus orgStatus = new OrganizationStatus();
            bool newUserWithFeatureDate = false;
            bool isUserExpired = false;
            bool newUserwithActiveStatus = false;
            int statusTypeId = 0;
            if (userLogins.Count > 0)
            {
                // set max date if inputted status is locked (from UI) 
                if (userLoginStatusType == UserUiStatusType.Locked)
                {
                    thruUtcDateTime = DateTime.MaxValue;
                    statusTypeId = (int)UserUiStatusType.Locked;
                }

                if (userLoginStatusType == UserUiStatusType.Unlocked || userLoginStatusType == UserUiStatusType.Active)
                {
                    thruUtcDateTime = null;
                    statusTypeId = (int)UserUiStatusType.Active;
                }

                if (userLoginStatusType == UserUiStatusType.Disabled)
                {
                    thruUtcDateTime = null;
                    statusTypeId = (int)UserUiStatusType.Disabled;
                }
            }

            if (statusTypeId > 0)
            {
                IList<Guid> userRealPageIdList = new List<Guid>();
                foreach (UserLoginOnly userLogin in userLogins)
                {
                    userRealPageIdList.Add(userLogin.RealPageId);
                }

                int result = _userLoginRepository.UpdateBulkUserStatus(userRealPageIdList, statusTypeId, fromUtcDateTime, thruUtcDateTime, _defaultUserClaim.OrganizationPartyId);

                if (result == -1)
                {
                    response.ErrorMessage = "Error while updating bulk statuses.";
                }
                else
                {
                    IList<UserLoginOnly> ul = new List<UserLoginOnly>();
                    foreach (UserLoginOnly userLogin in userLogins)
                    {
                        ul.Add(new UserLoginOnly() { RealPageId = userLogin.RealPageId });
                    }

                    bool isAssigned = true;
                    if (userLoginStatusType == UserUiStatusType.Active)
                    {
                        _userRepository.ActivateSalesForceUser(_defaultUserClaim.UserRealPageGuid, _defaultUserClaim.PersonaId, ul, isAssigned);
                        foreach (UserLoginOnly userLogin in userLogins)
                        {
                            UserLoginOnly userLoginOnly = _userLoginRepository.GetUserLoginOnly(userLogin.RealPageId);
                            var userLoginInfo = GetUserLogin(userLogin.RealPageId, _defaultUserClaim.OrganizationPartyId); // keep for now
                            orgStatus = _userLoginRepository.GetUserOrganizationWithStatus(userLoginOnly.UserId, userLoginOnly.LastLogin, _defaultUserClaim.OrganizationPartyId, false);
                            if (orgStatus.ThruDate != null)
                            {
                                if (DateTime.UtcNow > orgStatus.ThruDate)
                                {
                                    isUserExpired = true;
                                }
                            }
                            if (orgStatus.StatusThruDate != null)
                            {
                                if (DateTime.UtcNow > orgStatus.StatusThruDate)
                                {
                                    isUserExpired = true;
                                }
                            }

                            if (userLoginOnly.LastLogin == null && userLoginOnly.PasswordModifiedDate != null && !isUserExpired)
                                newUserwithActiveStatus = true;

                            fromUtcDateTime = orgStatus.FromDate;
                            orgStatus.ThruDate = new DateTime(9999, 12, 31);
                            if (orgStatus.FromDate > DateTime.UtcNow)
                            {
                                DateTime fromDate = DateTime.UtcNow;
                                orgStatus.FromDate = fromDate;
                                newUserWithFeatureDate = true;
                            }
                            if (orgStatus.PrimaryOrganization && (newUserWithFeatureDate || (userLoginOnly.LastLogin == null && !userLoginOnly.Is3rdPartyIDP && orgStatus.Status != UserUiStatusType.Locked)) && !newUserwithActiveStatus)
                            {
                                string message = string.Empty;
                                bool? isNotified = null;
                                IManageUserRegistrationEmail manageUserRegistrationEmail = new ManageUserRegistrationEmail(_defaultUserClaim);
                                isNotified = manageUserRegistrationEmail.SendNewUserRegistrationEmail(userLoginOnly, orgStatus.Name, (int)userLoginInfo.UserRoleType, orgStatus.PartyId);
                                statusTypeId = (int)UserUiStatusType.Pending;
                                var userDetailsInfo = _userRepository.GetUserDetails(userRealPageId: userLogin.RealPageId.ToString());
                                IProfileDetail profile = new ProfileDetail();
                                profile.FirstName = userDetailsInfo.FirstName;
                                profile.LastName = userDetailsInfo.LastName;
                                profile.userLogin.LoginName = userDetailsInfo.LoginName;
                                profile.userLogin.UserId = userDetailsInfo.UserId;
                                profile.userLogin.RealPageId = userDetailsInfo.UserRealPageId;
                                if (isNotified == true)
                                {
                                    message = "Welcome Email sent to user {0} {1} by user {2}.";
                                    LogAuditActivity(LogActivityTypeConstants.EMAIL_SENT, LogActivityCategoryType.Email, message, "UpdateUser", profile);
                                }
                                else if (isNotified == false)
                                {
                                    message = "Unable to Resend Welcome Email to user {0} {1} by user {2}.";
                                    LogAuditActivity(LogActivityTypeConstants.EMAIL_RESENT, LogActivityCategoryType.Email, message, "UpdateUser", profile);
                                }
                            }
                        }
                    }
                    foreach (UserLoginOnly userLogin in userLogins)
                    {
                        AddActivityLog(userLogin, userLoginStatusType.ToString(), ProductEnum.UnifiedPlatform.ToEnumDescription(), _defaultUserClaim);
                    }

                    if (userLoginStatusType == UserUiStatusType.Disabled || userLoginStatusType == UserUiStatusType.Active)
                    {
                        IManagePersona managePersona = new ManagePersona();

                        foreach (UserLoginOnly userLogin in userLogins)
                        {                        
                            Persona persona = managePersona.ListPersona(userLogin.RealPageId).Where(c => c.OrganizationPartyId == _defaultUserClaim.OrganizationPartyId).FirstOrDefault();
                            var userDetailsInfo = _userRepository.GetUserDetails(personaId: persona.PersonaId);

                            IList<UserLoginPersona> userLoginPersonaList = userLoginPersonaRepository.ListUserLoginPersona(userLoginPersonaId: null, userLoginId: userDetailsInfo.UserId, organizationPartyId: userDetailsInfo.OrganizationPartyId);
                            var primaryOrgPersona = userLoginPersonaList.Where(x => x.PrimaryOrganization).FirstOrDefault();
                            if (primaryOrgPersona != null && userDetailsInfo != null
                                && userDetailsInfo.UserRoleTypeId != UserTypeConstants.RegularUserNoEmail
                                && !userDetailsInfo.IsRPEmployee
                                && !userDetailsInfo.LoginName.Equals($"{userDetailsInfo.BooksMasterId}admin@realpage.com", StringComparison.OrdinalIgnoreCase))

                            {
                                var kafkaProducer = KafkaProducerServiceFactory.Instance;
                                kafkaProducer.PublishUserStatusChangeEventAsync(new UnifiedLoginUserStatusEvent
                                {
                                    persona_id = userDetailsInfo.PersonaId,
                                    user_login_name = userDetailsInfo.LoginName,
                                    is_active = userLoginStatusType == UserUiStatusType.Active,
                                    user_activation_deactivation_date = DateTime.UtcNow
                                }).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }

            return response;
        }

        private DefaultUserClaim GetDefaultUserClaim()
        {
            return new DefaultUserClaim(ClaimsPrincipal.Current);
        }

        private void AddActivityLog(UserLoginOnly userLogin, string activityTypeName, string booksProductCode, DefaultUserClaim defaultUserClaim, string activityMessage = "")
        {
            var person = _personRepository.GetPerson(userLogin.RealPageId);
            var userLoginTo = GetUserLoginOnly(userLogin.RealPageId);

            string message = string.Empty;
            string activity = string.Empty;
            List<string> logActivityTypeName = new List<string>();

            switch (activityTypeName.ToLower())
            {
                case "active":
                    activity = "Activated";
                    logActivityTypeName.Add(LogActivityTypeConstants.LOGIN_ENABLED);
                    break;
                case "disabled":
                    activity = "Deactivated";
                    logActivityTypeName.Add(LogActivityTypeConstants.LOGIN_DISABLED);
                    break;
                case "locked":
                    activity = "Locked";
                    logActivityTypeName.Add(LogActivityTypeConstants.USER_LOCKED);
                    break;
                case "unlocked":
                    activity = "Unlocked";
                    logActivityTypeName.Add(LogActivityTypeConstants.USER_UNLOCKED);
                    break;
                case "expired":
                    activity = "Expired";
                    logActivityTypeName.Add(LogActivityTypeConstants.USER_EXPIRED);
                    break;
                case "pending":
                    activity = "Pending";
                    logActivityTypeName.Add(LogActivityTypeConstants.LOGIN_ENABLED);
                    break;
                default:
                    break;
            }

            if (string.IsNullOrEmpty(activityMessage))
            {
                message = (defaultUserClaim.ImpersonatedBy == Guid.Empty) ? $"{defaultUserClaim.FirstName} {defaultUserClaim.LastName} {activity} user {person.FirstName} {person.LastName}." : $"RealPage Access ({defaultUserClaim.ImpersonatedByName}) {activity} user {person.FirstName} {person.LastName}.";
            }
            else
            {
                message = string.Format(activityMessage, person.FirstName, person.LastName);
            }
            if (!string.IsNullOrEmpty(activity))
            {
                foreach (string logType in logActivityTypeName)
                {

                    LogActivity.WriteActivity(new ActivityDetails
                    {
                        LogActivityTypeName = logType,
                        LogCategoryName = LogActivityCategoryType.User.ToString(),
                        CorrelationId = defaultUserClaim.CorrelationId.ToString(),
                        BooksMasterOrganizationId = defaultUserClaim.OrganizationMasterId,
                        OrganizationPartyId = defaultUserClaim.OrganizationPartyId,
                        Message = message,

                        FromUserLoginName = defaultUserClaim.LoginName,
                        FromUserLoginId = defaultUserClaim.UserId,
                        FromUserFirstName = defaultUserClaim.FirstName,
                        FromUserLastName = defaultUserClaim.LastName,
                        FromUserRealpageId = defaultUserClaim.UserRealPageGuid.ToString(),

                        ToUserLoginId = userLoginTo.UserId,
                        ToUserLoginName = userLoginTo.LoginName,
                        ToUserFirstName = person.FirstName,
                        ToUserLastName = person.LastName,
                        ToUserRealpageId = userLoginTo.RealPageId.ToString(),

                        BooksProductCode = booksProductCode
                    });
                }
            }
        }

        /// <summary>
        /// Get the computed expiration days and auto logout interval
        /// </summary>
        /// <param name="realPageId"></param>
        /// <param name="orgPartyId"></param>
        /// <returns></returns>
        public LogOutIntervalResponse GetLogOutInterval(Guid realPageId, long orgPartyId)
        {
            var response = new LogOutIntervalResponse();

            if (realPageId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(realPageId), "Null realPageId.");
            }

            var userLogin = GetUserLoginOnly(realPageId);

            if (userLogin != null)
            {
                // TODO should this be by login company or current company status?
                OrganizationStatus orgStatus = _userLoginRepository.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, orgPartyId, false);

                if (orgStatus.ThruDate != null)
                {
                    var clientLocalDatetime = ClientTimezone.GetClientDatetime(HttpContext.Current.Request);
                    var clientLocalDatetimeExpiration = orgStatus.ThruDate.Value.ToLocalTime();
                    var daysLeft = (int)Math.Floor(clientLocalDatetimeExpiration.Date.Subtract(clientLocalDatetime.Date).TotalDays);

                    if (daysLeft < 0)
                    {
                        response.IsError = true;
                        response.ErrorReason = "User account has expired.";
                        response.SeverityLevel = SeverityLevelType.Critical;
                    }
                    else
                    {
                        response.DaysToExpire = daysLeft;

                        if (daysLeft <= 10)
                        {
                            response.SeverityLevel = SeverityLevelType.Critical;
                        }
                        else if (daysLeft <= 20)
                        {
                            response.SeverityLevel = SeverityLevelType.Warning;
                        }
                        else if (daysLeft <= 30)
                        {
                            response.SeverityLevel = SeverityLevelType.Information;
                        }

                        if (daysLeft <= 24)
                        {
                            var expireInMilliSec = 0;
                            var tsTms = new TimeSpan(23, 59, 0).TotalMilliseconds;

                            if (daysLeft > 0)
                                expireInMilliSec = daysLeft * 24 * 60 * 60 * 1000 + (int)tsTms;
                            else
                                expireInMilliSec = (int)tsTms - (int)clientLocalDatetime.TimeOfDay.TotalMilliseconds;

                            response.LogOutSetInterval = int.Parse(expireInMilliSec.ToString());
                            response.Remaining = new TimeSpan(0, 0, 0, 0, response.LogOutSetInterval).ToString(@"dd\.hh\:mm\:ss");

                        }
                        else
                        {
                            response.LogOutSetInterval = -1;
                        }
                    }
                }
                else
                {
                    response.LogOutSetInterval = -1;
                }
            }
            else
            {
                response.IsError = true;
                response.ErrorReason = "User does not exist.";
            }

            return response;
        }

        /// <summary>
        /// Link Identity Provider to a UserLogin
        /// </summary>
        /// <param name="PersonaId">PersonaId</param>
        /// <param name="UserId">UserLogin unique Id</param>
        /// <param name="ContactMechanismId">Contact Mechanism Id</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse LinkIdentityProviderToUserLogin(long PersonaId, long UserId, int ContactMechanismId)
        {
            if (PersonaId <= 0)
            {
                throw new Exception("Missing Persona Id.");
            }

            if (UserId <= 0)
            {
                throw new Exception("Missing UserLogin Id.");
            }

            if (ContactMechanismId <= 0)
            {
                throw new Exception("Missing Contact Mechanism Id.");
            }

            return _userLoginRepository.LinkIdentityProviderToUserLogin(PersonaId, UserId, ContactMechanismId);
        }

        /// <summary>
        /// User Exists? User Exists in this Organization?
        /// </summary>
        /// <param name="loginName">User LoginName</param>
        /// <param name="organizationRealPageId">Unique Identifier - OrganizationRealPageId</param>
        /// <param name="userRealPageId">The id of the user if editing</param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="isFromExport"></param>
        /// <param name="userType"></param>
        /// <returns>UserOrganizationExists object</returns>
        public UserOrganizationExists IsLoginNameExists(string loginName, Guid organizationRealPageId, Guid userRealPageId,string firstName=null, string lastName=null, int userType = 0, bool isFromExport = false)
        {
            if (string.IsNullOrWhiteSpace(loginName))
            {
                throw new Exception("Invalid parameter loginName.");
            }

            bool isAdminUser = false;
            bool isRegularUser = false;
            //Remove all leading and Trailing white-space characters
            loginName = loginName.Trim();

            if (organizationRealPageId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(organizationRealPageId), "Null realPageId.");
            }

            Organization orgDetails = _organizationRepository.GetOrganization(realPageId: organizationRealPageId);
            UserOrganizationExists userOrganizationExists = new UserOrganizationExists();
            IList<UserOrganization> userPersonaOrganizationList = null;
            IList<UserOrganization> userPersonaOrganizationWithOrgIdList = null;
            UserLogin userLogin = new UserLogin();
            userPersonaOrganizationList = GetUserPersonaOrganization(loginName);

            if (isFromExport && userPersonaOrganizationList.Any() && !userType.Equals((int)UserRoleType.UserNoEmail))
            {
                var userExistingReleationShipTypes = userPersonaOrganizationList.Select(m => m.PartyRoleTypeId);
                if (!userExistingReleationShipTypes.Any(m => m == (int)UserRoleType.UserNoEmail))
                {
                    userPersonaOrganizationWithOrgIdList = GetUserPersonaOrganization(loginName, organizationRealPageId);
                    if (userPersonaOrganizationWithOrgIdList.Count == 0)
                    {
                        bool isExternalEveryWhere = userPersonaOrganizationList.All(x => x.PartyRoleTypeId.Equals((int)UserRoleType.ExternalUser));
                        if (userType.Equals((int)UserRoleType.ExternalUser) || (isExternalEveryWhere && userType.Equals((int)UserRoleType.User)))
                        {
                            userPersonaOrganizationList = userPersonaOrganizationWithOrgIdList;
                        }
                    }
                }
            }
            userOrganizationExists.IsValidDomainUsername = IsUserEmailDomainValid(loginName, firstName, lastName, userRealPageId);
            userOrganizationExists.UserExistsAsAdminInOtherDomain = false;
            userOrganizationExists.OrgIsRealpageEmployee = (orgDetails.RealPageId == EmployeeCompanyRealPageId);

            userOrganizationExists.UserExists = (userPersonaOrganizationList != null && userPersonaOrganizationList.Count > 0);
            userOrganizationExists.UserExistsInThisOrganization = (userPersonaOrganizationList != null && userPersonaOrganizationList.Count >= 0 && userPersonaOrganizationList.Any(a => a.OrganizationRealPageId == organizationRealPageId));
            userOrganizationExists.UserExistsAsNoEmail = userPersonaOrganizationList != null && userPersonaOrganizationList.Count > 0 && userPersonaOrganizationList.Any(p => (p.PartyRoleTypeId == (int)UserRoleType.UserNoEmail));

            if (userPersonaOrganizationList.Count > 0)
            {
                userOrganizationExists.UserIsExternalEverywhere = userPersonaOrganizationList.All(x => x.PartyRoleTypeId.Equals((int)UserRoleType.ExternalUser));

                var thisOrgStatus = userPersonaOrganizationList.FirstOrDefault(p => p.OrganizationRealPageId == organizationRealPageId);
                if (thisOrgStatus != null && !thisOrgStatus.PrimaryOrganization)
                {
                    userOrganizationExists.Restricted = new Dictionary<string, List<string>>();
                    List<string> restrictedList = new List<string>
                    {
                        {"FirstName"},
                        {"MiddleName"},
                        {"LastName"},
                        {"LoginName"}
                    };
                    userOrganizationExists.Restricted.Add("Fields", restrictedList);

                    restrictedList = new List<string>
                    {
                        {"resetPassword"},
                        {"securityQuestions"}
                    };
                    userOrganizationExists.Restricted.Add("Tabs", restrictedList);
                }
            }

            if (userOrganizationExists.UserExists)
            {
                var ulo = GetUserLoginOnly(loginName);
                userOrganizationExists.Person = new Person() { RealPageId = ulo.RealPageId };

                var userPrimaryOrg = userPersonaOrganizationList.FirstOrDefault(c => c.PrimaryOrganization);
                var superVisorInfo = _userRepository.GetSuperVisorInformation(ulo.UserId, _defaultUserClaim.OrganizationPartyId);
                userOrganizationExists.SuperVisor = (superVisorInfo != null) ? superVisorInfo : new UserInfoLite();
                userOrganizationExists.SuperVisor.UserId = ulo.UserId;
                userOrganizationExists.SuperVisor.IsReadOnly = (userPrimaryOrg != null && _defaultUserClaim.OrganizationPartyId != userPrimaryOrg.OrganizationPartyId);

                //Find the Primary Organization
                UserOrganization userOrganization = userPersonaOrganizationList.FirstOrDefault(m => m.PrimaryOrganization.Equals(true));
                if (userOrganization != null)
                {
                    if (userOrganization.OrganizationRealPageId != DefaultUserClaim.EmployeeCompanyRealPageId && userOrganization.OrganizationRealPageId != DefaultUserClaim.ExternalCompanyRealPageId && userOrganization.OrganizationRealPageId != DefaultUserClaim.ContractCompanyRealPageId)
                    {
                        userOrganizationExists.PrimaryCompanyName = userOrganization.OrganizationName;
                    }

                    //Get user details (includes the status)
                    userLogin = GetUserLogin(realPageId: ulo.RealPageId, orgPartyId: userOrganization.OrganizationPartyId, userLogin: null, userStatuses: null);
                    if (userLogin != null)
                    {
                        userOrganizationExists.UserIsDisabledInPrimaryCompany = userLogin.StatusId.Equals((int)UserUiStatusType.Disabled);
                    }
                }

                if (userOrganizationExists.OrgIsRealpageEmployee && !userOrganizationExists.UserExistsInThisOrganization)
                {
                    userOrganizationExists.Person = _personRepository.GetPerson(ulo.RealPageId);
                }
                // get the companies current roles and make sure External user type exists
                // use the organization id of the person creating the user
                IList<RoleType> userRoles = _roleTypeRepository.GetRoleType("User Role", _defaultUserClaim.OrganizationPartyId);
                if (userRoles.All(c => c.PartyRoleTypeId != (int)UserRoleType.ExternalUser))
                {
                    userOrganizationExists.UserExistsNotAvailable = true;
                    return userOrganizationExists;
                }

                var p = _personRepository.GetPerson(ulo.RealPageId);
                if (p != null)
                {
                    userOrganizationExists.Person = p;
                }
            }

            if (userOrganizationExists.UserExists && !userOrganizationExists.UserExistsInThisOrganization)
            {
                UserOrganization userOrganization = userPersonaOrganizationList.FirstOrDefault(m => m.PrimaryOrganization.Equals(true));
                isAdminUser = userOrganization != null && userOrganization.PartyRoleTypeId == (int)UserRoleType.SuperUser;
                isRegularUser = userOrganization != null && userOrganization.PartyRoleTypeId == (int)UserRoleType.User;
                if (userOrganization != null && (isAdminUser || isRegularUser) && userOrganization.BooksCustomerMasterId == orgDetails.BooksCustomerMasterId)
                {
                    var orgDomains = _organizationRepository.GetOrganizationListByBooksCustomerMasterId(orgDetails.BooksCustomerMasterId);
                    if (orgDomains.Count > 1)
                    {
                        userOrganizationExists.UserExists = true;
                        userOrganizationExists.UserExistsAsAdminInOtherDomain = isAdminUser;
                        userOrganizationExists.UserExistsAsRegularUserInOtherDomain = isRegularUser;
                    }
                }
            }
            return userOrganizationExists;
        }

        /// <summary>
        /// Gets a list of organizations for the given login name
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="organizationRealPageId"></param>
        /// <returns></returns>
        public IList<UserOrganization> GetUserPersonaOrganization(string loginName, Guid? organizationRealPageId = null)
        {
            return _userLoginRepository.ListOrganizationByLoginName(loginName, organizationRealPageId);
        }

        /// <summary>
        /// checks user name domain valid or not
        /// </summary>
        public bool IsUserEmailDomainValid(string loginName,string firstName = null, string lastName = null, Guid? userRealPageId = null)
        {
            var BlacklistedDomains = GetBlacklistedDomains();
            var userDomain = loginName.Split('@').LastOrDefault();
            bool isUserEmailDomainValid = !BlacklistedDomains.Contains(userDomain);
            if (!isUserEmailDomainValid)
            {
                string userRealpageIdString = (userRealPageId.HasValue && userRealPageId.Value != Guid.Empty) ? userRealPageId.Value.ToString() : null;
                var userDetailsInfo = _userRepository.GetUserDetails(null, userRealpageIdString);
                UserDetails impersonatorUserInfo = _defaultUserClaim.ImpersonatedBy == Guid.Empty ? null : _userRepository.GetUserDetails(null, _defaultUserClaim.ImpersonatedBy.ToString());
                var message = (string.IsNullOrEmpty(_defaultUserClaim.ImpersonatedByName) || impersonatorUserInfo == null)
                ? $"{_defaultUserClaim.FirstName} {_defaultUserClaim.LastName} ({_defaultUserClaim.LoginName}) acknowledged an Unauthorized Access warning when attempting to create a user for {firstName} {lastName} ({loginName})."
                : $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName} ({impersonatorUserInfo.LoginName})) acknowledged an Unauthorized Access warning when attempting to create a user for {firstName} {lastName} ({loginName}).";
                LogActivity.WriteActivity(new ActivityDetails
                {
                    LogActivityTypeName = LogActivityTypeConstants.UPDATE_USER,
                    LogCategoryName = LogActivityCategoryType.User.ToString(),
                    CorrelationId = _defaultUserClaim.CorrelationId.ToString(),
                    BooksMasterOrganizationId = _defaultUserClaim.OrganizationMasterId,
                    OrganizationPartyId = _defaultUserClaim.OrganizationPartyId,
                    Message = message,

                    FromUserLoginName = _defaultUserClaim.LoginName,
                    FromUserLoginId = _defaultUserClaim.UserId,
                    FromUserRealpageId = _defaultUserClaim.UserRealPageGuid.ToString(),
                    FromUserFirstName = _defaultUserClaim.FirstName,
                    FromUserLastName = _defaultUserClaim.LastName,

                    ToUserLoginName = loginName,
                    ToUserLoginId = userDetailsInfo?.UserId ?? 0,
                    ToUserFirstName = firstName,
                    ToUserLastName = lastName,
                    ToUserRealpageId = userRealpageIdString
                });
            }
            return isUserEmailDomainValid;
        }

        /// <summary>
        /// Gets all blacklisted domains
        /// </summary>
        public IList<string> GetBlacklistedDomains()
        {
            return _userLoginRepository.GetBlacklistedDomains();
        }

        /// <summary>
        /// Used to get the specified organization for the given user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="lastLogin"></param>
        /// <param name="orgPartyId"></param>
        /// <param name="getPrimaryOrg"></param>
        /// <returns>Organization status</returns>
        public OrganizationStatus GetUserOrganizationWithStatus(long userId, DateTime lastLogin, long orgPartyId, bool getPrimaryOrg)
        {
            return _userLoginRepository.GetUserOrganizationWithStatus(userId, lastLogin, orgPartyId, getPrimaryOrg);
        }

        /// <summary>
        /// Log that an existing user requested to resend an email link
        /// </summary>
        /// <param name="userRealPageId"></param>
        public void LogUserRequestedEmailLinkResent(Guid userRealPageId)
        {
            var profileLogic = new ManageProfile(_defaultUserClaim);

            // Get Userlogin to pass Data
            var userLogin = _userLoginRepository.GetUserLoginOnly(userRealPageId);
            var orgWithoutStatus = _userLoginRepository.GetPrimaryOrgWithoutStatusByUserId(userLogin.UserId);
            var orgWithStatus = _userLoginRepository.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, orgWithoutStatus.PartyId, false);
            var profileDetail = profileLogic.GetProfileDetail(userRealPageId, orgWithStatus.PartyId);

            var message = $"User {profileDetail.FirstName} {profileDetail.LastName} requested a new activation link";
            LogAuditActivity(LogActivityTypeConstants.USER_REQUESTED_NEW_ACTIVATION_LINK, LogActivityCategoryType.Email, message, null, profileDetail);
        }

        #endregion

        #region Private Methods
        private void LogAuditActivity(string logActivityType, LogActivityCategoryType logActivityCategoryType, string message, string stepName, IProfileDetail profile)
        {
            string userName = string.IsNullOrEmpty(_defaultUserClaim.ImpersonatedByName) ? _defaultUserClaim.FirstName + " " + _defaultUserClaim.LastName : " RealPage Access (" + _defaultUserClaim.ImpersonatedByName + ") ";
            LogActivity.WriteActivity(new ActivityDetails
            {
                LogActivityTypeName = logActivityType,
                LogCategoryName = logActivityCategoryType.ToString(),
                CorrelationId = _defaultUserClaim.CorrelationId.ToString(),
                BooksMasterOrganizationId = _defaultUserClaim.OrganizationMasterId,
                OrganizationPartyId = _defaultUserClaim.OrganizationPartyId,
                Message = string.Format(message, profile.FirstName, profile.LastName, userName, profile.CreateUserSourceType.ToString()),

                FromUserLoginName = _defaultUserClaim.LoginName,
                FromUserLoginId = _defaultUserClaim.UserId,
                FromUserRealpageId = _defaultUserClaim.UserRealPageGuid.ToString(),
                FromUserFirstName = _defaultUserClaim.FirstName,
                FromUserLastName = _defaultUserClaim.LastName,

                ToUserLoginName = profile.userLogin.LoginName,
                ToUserLoginId = profile.userLogin.UserId,
                ToUserFirstName = profile.FirstName,
                ToUserLastName = profile.LastName,
                ToUserRealpageId = profile.userLogin.RealPageId.ToString()
            });
        }

        /// <summary>
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            string correlationId = "";
            if (_defaultUserClaim != null)
            {
                correlationId = (_defaultUserClaim.CorrelationId != Guid.Empty) ? _defaultUserClaim.CorrelationId.ToString() : "";
            }
            var logger = Log.Logger;
            if (logData?.Keys != null)
            {
                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
            }
            logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", correlationId);

            logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
        }
        #endregion
    }
}