using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.EnterpriseRole;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Foundation.DataAccess.Component;
using System.Net.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.ThirdParty;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    /// <summary>
    /// Manage User repository calls
    /// </summary>
    public class ManageUser : IManageUser
    {
        IUserRepository _userRepository;
        ICredentialRepository _credentialRepository;
        IUserLoginRepository _userLoginRepository;
        IProductRepository _productRepository;
        private IManageUserRegistrationEmail _manageUserRegistrationEmail;

        private DefaultUserClaim _userClaim;

        #region Ctor

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="userRepository"></param>
        /// <param name="credentialRepository"></param>
        /// <param name="userLoginRepository"></param>
        /// <param name="manageUserRegistrationEmail"></param>
        /// <param name="userClaim"></param>
        public ManageUser(IUserRepository userRepository, ICredentialRepository credentialRepository, IUserLoginRepository userLoginRepository, IManageUserRegistrationEmail manageUserRegistrationEmail, DefaultUserClaim userClaim)
        {
            _userRepository = userRepository;
            _credentialRepository = credentialRepository;
            _userLoginRepository = userLoginRepository;
            _manageUserRegistrationEmail = manageUserRegistrationEmail;
            _userClaim = userClaim;
        }

        /// <summary>
        /// Unit test constructor v2
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="userClaim"></param>
        /// <param name="messageHandler"></param>
        public ManageUser(IRepository repository, DefaultUserClaim userClaim, HttpMessageHandler messageHandler)
        {
            _userRepository = new UserRepository(repository, userClaim, messageHandler);
            _credentialRepository = new CredentialRepository(repository);
            _userLoginRepository = new UserLoginRepository(repository);
            _manageUserRegistrationEmail = new ManageUserRegistrationEmail(userClaim, repository);
            _userClaim = userClaim;
        }


        /// <summary>
        /// Create a basic instance of the ManageUser Controller class
        /// </summary>
        public ManageUser(DefaultUserClaim userClaim)
        {
            _userRepository = new UserRepository(userClaim);
            _credentialRepository = new CredentialRepository();
            _userLoginRepository = new UserLoginRepository();
            _manageUserRegistrationEmail = new ManageUserRegistrationEmail(userClaim);
            _productRepository = new ProductRepository();
            _userClaim = userClaim;
        }

        #endregion

        #region User Details

        /// <summary>
        /// Validate New User
        /// </summary> 
        /// <param name="enterpriseUserName">Enterprise UserName</param>
        /// <param name="newUserRegistrationToken">new User Registration Token</param>
        /// <returns>ValidateUserResponse object</returns>
        public ValidateUserResponse ValidateUser(string enterpriseUserName, string newUserRegistrationToken)
        {
            var response = new ValidateUserResponse();

            if (string.IsNullOrEmpty(enterpriseUserName))
            {
                response.IsError = true;
                response.ErrorReason = "No Username specified.";
                return response;
            }

            if (string.IsNullOrEmpty(newUserRegistrationToken))
            {
                response.IsError = true;
                response.ErrorReason = "No validation token specified.";
                return response;
            }

            response.EnterpriseUserName = enterpriseUserName;
            var userLogin = _userLoginRepository.GetUserLoginOnly(enterpriseUserName);

            if (userLogin == null)
            {
                response.IsError = true;
                response.ErrorReason = "User login information is missing.";
                return response;
            }

            //var userOrgList = 
            var organizationList = _userLoginRepository.ListOrganizationByEnterpriseUserId(userLogin.RealPageId, null);
            var primaryOrgStatus = _userLoginRepository.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, 0, true);

            TokenDetail tokenDetail = null;
            foreach (Organization org in organizationList)
            {
                tokenDetail = _credentialRepository.GetActivityToken(enterpriseUserName, newUserRegistrationToken, (int)ActivityType.NewUserRegistration, org.PartyId);
                if (tokenDetail != null)
                {
                    break;
                }
            }

            if (primaryOrgStatus.Status == UserUiStatusType.UnDefined)
            {
                response.IsError = true;
                response.ErrorReason = "Organization status could not be determined.";
                return response;
            }

            if (tokenDetail == null || tokenDetail.EnterpriseUserId <= 0 || primaryOrgStatus.IsExpired.Value == true)
            {
                response.IsError = true;
                response.ErrorReason = "This link has expired. Please contact your System Administrator.";
                return response;
            }

            //when effective date is in the future and status is pending          
            if (primaryOrgStatus.IsActive.Value == false && primaryOrgStatus.IsPending.Value == true)
            {
                response.IsError = true;
                response.ErrorReason = "Account is inactive.";
                return response;
            }

            if (primaryOrgStatus.IsPending.Value == false)
            {
                response.IsError = true;
                response.ErrorReason = "Profile already completed.";
                return response;
            }

            response.ValidateUserToken = _credentialRepository.CreateActivityToken(primaryOrgStatus.PartyId, tokenDetail.RealPageId, (int)ActivityType.NewUserRegistrationVerification);

            if (string.IsNullOrEmpty(response.ValidateUserToken))
                throw new Exception("Unable to get token"); // TODO; research on this

            return response;
        }

        /// <summary>
        /// Validate registration verification token is associated with user name
        /// </summary>
        /// <param name="enterpriseUserName">Enterprise UserName</param>
        /// <param name="verificationToken">verification Token</param>
        /// <returns>ValidateUserResponse object</returns>
        public ValidateUserResponse ValidateRegistrationVerificationToken(string enterpriseUserName, string verificationToken)
        {
            var response = new ValidateUserResponse();

            if (string.IsNullOrEmpty(enterpriseUserName))
            {
                response.IsError = true;
                response.ErrorReason = "No Username specified.";
                return response;
            }

            if (string.IsNullOrEmpty(verificationToken))
            {
                response.IsError = true;
                response.ErrorReason = "No validation token specified.";
                return response;
            }

            response.EnterpriseUserName = enterpriseUserName;

            var user = _userLoginRepository.GetUserLoginOnly(enterpriseUserName);

            long orgPartyId = _userClaim.OrganizationPartyId;
            if (orgPartyId == 0)
            {
                //IManageOrganization organizationLogic = new ManageOrganization();
                //var organizationList = organizationLogic.ListOrganizationByEnterpriseUserId(user.RealPageId, null);
                //var organizationList = _userLoginRepository.ListOrganizationByEnterpriseUserId(user.RealPageId, null);
                orgPartyId = _userLoginRepository.GetPrimaryOrgIdByUserId(user.UserId);
            }

            var tokenDetail = _credentialRepository.GetActivityToken(enterpriseUserName, verificationToken, (int)ActivityType.NewUserRegistrationVerification, orgPartyId);

            if (tokenDetail == null || tokenDetail.EnterpriseUserId <= 0)
            {
                response.IsError = true;
                response.ErrorReason = "Validation token does not match with user.";
                return response;
            }

            response.ValidateUserToken = verificationToken;

            return response;
        }

        /// <summary>
        /// Get Starter Profile Options
        /// </summary> 
        /// <param name="enterpriseUserName">Enterprise UserName</param>
        /// <returns>StarterProfileOptionsResponse object</returns>
        public StarterProfileOptionsResponse GetStarterProfileOptions(string enterpriseUserName)
        {
            if (string.IsNullOrEmpty(enterpriseUserName))
                throw new ArgumentNullException(nameof(enterpriseUserName));

            return _userRepository.GetStarterProfileOptions(enterpriseUserName);
        }

        /// <summary>
        /// Set Starter Profile  
        /// </summary> 
        /// <param name="starterProfile">StarterProfile object</param>
        /// <returns>SetStarterProfile object</returns>
        public SetStarterProfile SetStarterProfile(StarterProfile starterProfile)
        {
            if (starterProfile == null)
                throw new ArgumentNullException(nameof(starterProfile));

            return _userRepository.SetStarterProfileOptions(starterProfile);
        }

        /// <summary>
        /// Create user
        /// </summary> 
        /// <param name="profile">profiledetails of the new user</param>
        /// <param name="persona">Persona of the new user</param>
        /// <param name="createdByEnterpriseAPI">To change the activity log</param>
        /// <returns>CreateUserResponse and Error object</returns>
        public CreateUserResponse<IErrorData> CreateUser(ProfileDetail profile, IList<Persona> persona, bool createdByEnterpriseAPI = false)
        {
            long cloneUserPersonaId = 0;
            Guid cloneUserRealpageId = Guid.Empty;
            if (profile.ClonedUser)
            {
                cloneUserPersonaId = profile.Persona[0].PersonaId;
                cloneUserRealpageId = profile.Persona[0].RealPageId;
            }
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "CreateUser", $"MangeUser.CreateUser Login Name is : {profile.userLogin.LoginName}" });
            CreateUserResponse<IErrorData> response = _userRepository.CreateUser(profile, persona);
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            if (response.Status != null && response.Status.Success == true)
            {
                // Add activity message for user creation
                string auditMessage = "";
                if (profile.MigratedUser)
                {
                    profile.CreateUserSourceType = CreateUserSourceType.MigrationTool;
                }
                if (profile.CreateUserSourceType == CreateUserSourceType.UnifiedPlatform)
                {
                    if (profile.ClonedUser)
                    {
                        IManagePerson personLogic = new ManagePerson();
                        IPerson person = new Person();

                        person = personLogic.GetPerson(cloneUserRealpageId);
                        if (person != null)
                        {
                            auditMessage = "User {0} {1} originally cloned from user " + person.FirstName + " " + person.LastName + " by " + ((profile.organization[0].RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId) ? "RealPage user " : string.Empty) + "{2}.";
                        }
                    }
                    else if (profile.organization[0].RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId)
                    {
                        auditMessage = "New User {0} {1} successfully created by RealPage user {2}.";
                    }
                    else if (createdByEnterpriseAPI)
                    {
                        auditMessage = "New User {0} {1} successfully created by RealPage user {2} using enterprise API.";
                    }
                    else
                    {
                        auditMessage = "New User {0} {1} successfully created by user {2}.";
                    }
                }
                else
                {
                    auditMessage = "New User {0} {1} successfully created by {3}.";
                }

                LogAuditActivity(LogActivityTypeConstants.CREATE_USER, LogActivityCategoryType.User, auditMessage, "CreateUser", profile);

                string message = "";
                //Send Email only when time time difference between server utc time and user from date less than 15 minutes
                if (profile.userLogin.FromDate.Value.Subtract(DateTime.UtcNow).TotalMinutes <= 15 && !profile.userLogin.doNotForceChangePassword)
                {
                    UserLoginOnly userLoginOnly = _userLoginRepository.GetUserLoginOnly(profile.userLogin.LoginName);
                    bool isNotified = _manageUserRegistrationEmail.SendNewUserRegistrationEmail(userLoginOnly, profile.organization[0].Name, profile.UserTypeId, profile.organization[0].PartyId);

                    if (profile.UserTypeId != UserTypeConstants.RegularUserNoEmail && !profile.userLogin.Is3rdPartyIDP)
                    {
                        if (isNotified)
                        {
                            //Log Activity
                            message = profile.organization[0].RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId
                                ? "Welcome Email sent to user {0} {1} by RealPage user {2}."
                                : "Welcome Email sent to user {0} {1} by user {2}.";
                            LogAuditActivity(LogActivityTypeConstants.EMAIL_SENT, LogActivityCategoryType.Email, message, "CreateUser", profile);
                        }
                        else
                        {
                            //Log Activity
                            message = profile.organization[0].RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId
                                ? "Unable to Resend Welcome Email to user {0} {1} by RealPage user {2}."
                                : "Unable to Resend Welcome Email to user {0} {1} by user {2}.";
                            LogAuditActivity(LogActivityTypeConstants.EMAIL_RESENT, LogActivityCategoryType.Email, message, "CreateUser", profile);
                        }
                    }
                }

                bool isDelegateAdmin = _userRepository.GetUnifiedSettingData("delegateadministrators");
                bool newProfileDelegate = profile.IsDelegateAdmin;
                if (isDelegateAdmin)
                {
                    //Get all enterprise role Names by orgpartyid
                    ProductRepository productRepository = new ProductRepository(_userClaim);
                    List<RoleTemplate> roleTemplates = productRepository.GetRoleTemplateList(_userClaim.OrganizationPartyId);

                    if (newProfileDelegate)
                    {
                        string delegateMessage = "User admin{2}has added {0} {1} as Delegate admin";
                        LogAuditActivity(LogActivityTypeConstants.UPDATE_USER, LogActivityCategoryType.User, delegateMessage, "UpdateUser", profile);
                    }

                    var newDelegateRoles = profile.DelegateRoleTemplate?.RoleTemplateId != null ? profile.DelegateRoleTemplate.RoleTemplateId : new List<int>();
                    
                    if (newDelegateRoles.Count > 0)
                    {
                        var userEnterpriseRoles = roleTemplates.Where(r => newDelegateRoles.Contains(r.RoleTemplateId));
                        string delegateRolesMessage = "User admin{2}has added " + string.Join(", ", userEnterpriseRoles.Select(s => s.RoleTemplateName)) + " enterprise roles for Delegate admin {0} {1}";
                        LogAuditActivity(LogActivityTypeConstants.UPDATE_USER, LogActivityCategoryType.User, delegateRolesMessage, "UpdateUser", profile);
                    }
                }
            }
            return response;
        }

        /// <summary>
        /// Update New User Profile
        /// </summary> 
        /// <param name="userLogin">User Login of the New User</param>
        /// <param name="newProfile">Profile of the New User</param>
        /// <param name="partyRoleTypeId">PartyRoleTypeId of the New User</param>
        /// <param name="companyJobTitle">Job Title of the New User</param>
        /// <param name="activityToken">Activity Token for Validation</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse UpdateNewUser(string userLogin, Profile newProfile, int partyRoleTypeId, string companyJobTitle, string activityToken)
        {
            RepositoryResponse updateUserResponse = new RepositoryResponse();

            if (userLogin == null)
            {
                updateUserResponse.ErrorMessage = "Invalid parameter: userLogin";
            }
            else if (newProfile.PartyRole.RoleTypeId == 0)
            {
                updateUserResponse.ErrorMessage = "Invalid parameter: partyRoleTypeId";
            }
            else if (newProfile.TelecommunicationNumber.Count == 0)
            {
                updateUserResponse.ErrorMessage = "Invalid parameter: telecommunicationNumber";
            }
            else if (string.IsNullOrEmpty(activityToken.Trim()))
            {
                updateUserResponse.ErrorMessage = "Invalid parameter: activityToken";
            }

            //if (updateUserResponse.ErrorMessage == null)
            //impacted in defaulting empty string in property definition
            if (string.IsNullOrWhiteSpace(updateUserResponse.ErrorMessage))
            {
                updateUserResponse = _userRepository.UpdateNewUser(userLogin, newProfile, partyRoleTypeId, companyJobTitle, activityToken);
            }

            return updateUserResponse;
        }

        /// <summary>
        /// Update User Detail and Products
        /// </summary>
        /// <param name="loggedInUserRealPageId">Logged-In User unique identifier</param>
        /// <param name="profile">Edited User detail and Products</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse UpdateUser(Guid loggedInUserRealPageId, IProfileDetail profile)
        {
            RepositoryResponse repositoryResponse = new RepositoryResponse();

            if (loggedInUserRealPageId == Guid.Empty)
            {
                repositoryResponse.ErrorMessage = "Edit User: Invalid parameter realPageId.";
                return repositoryResponse;
            }

            bool sendNotification = false;

            if (profile.userLogin.Status == UserUiStatusType.Disabled && profile.userLogin.IsActive.Value)
            {
                profile.userLogin.FromDate = DateTime.UtcNow;
                sendNotification = true;
            }

            IProfileDetail oldProfile = this.GetUserProfile(profile.RealPageId, loggedInUserRealPageId, profile.Persona.First().OrganizationPartyId).obj;

            IUserLoginPersonaRepository userLoginPersonaRepository = new UserLoginPersonaRepository();
            IList<UserLoginPersona> userLoginPersonaList = userLoginPersonaRepository.ListUserLoginPersona(userLoginPersonaId: null, userLoginId: profile.Persona[0].UserId, organizationPartyId: profile.Persona[0].Organization.PartyId);

            var employeeId = this.GetUserEmployeeId(userLoginPersonaList[0].UserLoginPersonaId, profile.Persona.First().OrganizationPartyId);

            oldProfile.EmployeeId = (employeeId != null && !string.IsNullOrEmpty(employeeId.EmployeeId)) ? employeeId.EmployeeId : "";
            oldProfile.UserEmployeeId = (employeeId != null && employeeId.UserEmployeeId > 0) ? employeeId.UserEmployeeId : 0;

            repositoryResponse = _userRepository.UpdateUser(loggedInUserRealPageId, profile, oldProfile);
            if (repositoryResponse.Id > 0)
            {
                if (sendNotification)
                {
                    IManageUserRegistrationEmail manageUserRegistrationEmail = new ManageUserRegistrationEmail(_userClaim);
                    string message = "";
                    bool isNotified = manageUserRegistrationEmail.SendNewUserRegistrationEmail(profile);

                    if (isNotified)
                    {
                        //Log Activity
                        message = profile.organization[0].RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId
                                ? "Welcome Email sent to user {0} {1} by RealPage user {2}."
                                : "Welcome Email sent to user {0} {1} by user {2}.";
                        LogAuditActivity(LogActivityTypeConstants.EMAIL_SENT, LogActivityCategoryType.Email, message, "UpdateUser", profile);
                    }
                    else
                    {
                        //Log Activity
                        message = profile.organization[0].RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId
                                ? "Unable to Resend Welcome Email to user {0} {1} by RealPage user {2}."
                                : "Unable to Resend Welcome Email to user {0} {1} by user {2}.";
                        LogAuditActivity(LogActivityTypeConstants.EMAIL_RESENT, LogActivityCategoryType.Email, message, "UpdateUser", profile);
                    }
                }
            }

            return repositoryResponse;
        }

        /// <summary>
        /// Used to update the product status for a list of users
        /// </summary>
        /// <param name="editorRealPageId"></param>
        /// <param name="editorPersonaId"></param>
        /// <param name="userLogins"></param>
        /// <param name="userLoginStatusType"></param>
        /// <returns></returns>
        public RepositoryResponse UpdateUserStatus(Guid editorRealPageId, long editorPersonaId, IList<UserLoginOnly> userLogins, UserUiStatusType? userLoginStatusType)
        {
            RepositoryResponse repositoryResponse = new RepositoryResponse();
            if (userLoginStatusType == UserUiStatusType.Disabled)
            {
                _userRepository.DisableUserProduct(editorRealPageId, editorPersonaId, userLogins);
            }

            if (userLoginStatusType == UserUiStatusType.Active)
            {
                _userRepository.ActivateUserProducts(editorRealPageId, editorPersonaId, userLogins);
            }
            return repositoryResponse;
        }

        /// <summary>
        /// Used to disable the product status for a list of users
        /// </summary>	
        /// <param name="userLogins"></param>		
        /// <returns></returns>
        public RepositoryResponse DisableUsersFromProducts(IList<ProcessUserLogin> userLogins)
        {
            RepositoryResponse repositoryResponse = new RepositoryResponse();
            _userRepository.ProcessDisabledUsers(userLogins);

            return repositoryResponse;
        }

		/// <summary>
		/// Used to disable the product status for a list of users
		/// </summary>	
		/// <param name="userIds"></param>
		/// <param name="isEnabled"></param>
		/// <returns></returns>
		public RepositoryResponse ThirdPartyIdpBulkUpdate(IList<long> userIds, bool isEnabled)
		{
			var response = _userRepository.ThirdPartyIdpBulkUpdate(userIds, isEnabled);
			return response;
		}

		/// <summary>
		/// Give administrators access to missing products based on a customer company
		/// </summary>
		/// <param name="organizationRealPageId">Organization enterprise Id</param>
		/// <param name="assignUserPersonaId">Assigned to user PersonaId</param>
		public RepositoryResponse AssignProductsToAdministrators(Guid organizationRealPageId, long assignUserPersonaId = 0)
        {
            RepositoryResponse repositoryResponse = new RepositoryResponse();

            if (organizationRealPageId == Guid.Empty)
            {
                throw new Exception("Invalid parameter organization realPageId.");
            }

            repositoryResponse.Id = 1;
            repositoryResponse.RealPageId = organizationRealPageId;
            try
            {
                _userRepository.AssignProductsToAdministrators(organizationRealPageId, assignUserPersonaId);
            }
            catch (Exception exception)
            {
                repositoryResponse.Id = 0;
                repositoryResponse.ErrorMessage = exception.Message;
            }

            return repositoryResponse;
        }

        /// <summary>
        /// Check Product Right
        /// </summary>
        /// <param name="productBatch">Product Batch data</param>
        /// <returns>Boolean</returns>
        public bool CheckProductRight(ProductBatch productBatch)
        {
            bool hasAccess = false;
            // check with logged in editors rights
            List<string> editorRights = _userClaim.Rights;

            switch (productBatch.ProductId)
            {
                case (int)ProductRightEnum.ManageAccountingProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageAccountingProductAccess.ToString());
                    break;
                case (int)ProductRightEnum.ManageAssetOptimizationProductAccess:
                case (int)ProductRightEnum.AoAIRevenueManagement:
                case (int)ProductRightEnum.AoAmenityOptimization:
                case (int)ProductRightEnum.AoLeaseRentOption:
                case (int)ProductRightEnum.AoRentControl:
                case (int)ProductRightEnum.AoBusinessIntelligence:
                case (int)ProductRightEnum.AoPerformanceAnalytics:
                case (int)ProductRightEnum.AoInvestmentAnalytics:
                case (int)ProductRightEnum.AoRevenueManagement:
                case (int)ProductRightEnum.AoAxiometrics:
                case (int)ProductRightEnum.AoBenchmarking:
                case (int)ProductRightEnum.AoMarketAnalytics:
                case (int)ProductRightEnum.AoBIX:
                case (int)ProductRightEnum.AoLuminaAscent:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageAssetOptimizationProductAccess.ToString());
                    break;
                case (int)ProductRightEnum.ManageClientPortalProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageClientPortalProductAccess.ToString());
                    break;
                case (int)ProductRightEnum.ManageDocumentManagementProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageDocumentManagementProductAccess.ToString());
                    break;
                case (int)ProductRightEnum.ManageILMLeadManagemementProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageILMLeadManagemementProductAccess.ToString());
                    break;
                case (int)ProductRightEnum.ManageILMLeasingAnalyticsProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageILMLeasingAnalyticsProductAccess.ToString());
                    break;
                case (int)ProductRightEnum.ManageLead2LeaseProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageLead2LeaseProductAccess.ToString());
                    break;
                case (int)ProductRightEnum.ManageMarketingCenterProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageMarketingCenterProductAccess.ToString());
                    break;
                case (int)ProductRightEnum.ManageOneSiteProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageOneSiteProductAccess.ToString());
                    break;
                case (int)ProductRightEnum.ManageOnSiteProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageOnSiteProductAccess.ToString());
                    break;
                case (int)ProductRightEnum.ProspectContactCenterProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ProspectContactCenterProductAccess.ToString());
                    break;
                case (int)ProductRightEnum.ManageRentersInsuranceProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageRentersInsuranceProductAccess.ToString());
                    break;
                case (int)ProductRightEnum.AddEditResidentPortalUser:
                    hasAccess = editorRights.Contains(ProductRightEnum.AddEditResidentPortalUser.ToString());
                    break;
                case (int)ProductRightEnum.ManageSpendManagementProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageSpendManagementProductAccess.ToString());
                    break;
                case (int)ProductRightEnum.ManageUnifiedAmenitiesProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageUnifiedAmenitiesProductAccess.ToString());
                    break;
                case (int)ProductRightEnum.ManageUtilityManagementProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageUtilityManagementProductAccess.ToString());
                    break;
                case (int)ProductRightEnum.ManageVendorComplianceProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageVendorComplianceProductAccess.ToString());
                    break;

                case (int)ProductRightEnum.ManagePortfolioManagementProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManagePortfolioManagementProductAccess.ToString());
                    break;
                case (int)ProductRightEnum.AccessIntegrationMarketplace:
                    hasAccess = editorRights.Contains(ProductRightEnum.AccessIntegrationMarketplace.ToString());
                    break;
                case (int)ProductRightEnum.ManagePlatFormSecurity:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManagePlatFormSecurity.ToString());
                    break;
                case (int)ProductRightEnum.ManageCustomFields:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageCustomFields.ToString());
                    break;
                case (int)ProductRightEnum.ManageDepositAlternativeProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageDepositAlternativeProductAccess.ToString());
                    break;
                case (int)ProductRightEnum.ManageClickPayProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageClickPayProductAccess.ToString());
                    break;
                case (int)ProductRightEnum.ManageRenovationManager:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageRenovationManager.ToString());
                    break;
                case (int)ProductRightEnum.ManageSeniorLeadManagement:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageSeniorLeadManagement.ToString());
                    break;
                case (int)ProductRightEnum.ManageAdminSupportPortalProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageAdminSupportPortalProductAccess.ToString());
                    break;
                case (int)ProductRightEnum.ManageRealConnectProductAccess:
                    hasAccess = editorRights.Contains(ProductRightEnum.ManageRealConnectProductAccess.ToString());
                    break;
                default:
                    hasAccess = true; // Some products will have default acess - ex UnifiedLogin
                    break;
            }
            return hasAccess;
        }

        /// <summary>
        /// Get the user profile
        /// </summary>
        /// <param name="realPageId">Real page identifier</param>
        /// <param name="realpageUserId">Real page user identifier</param>
        /// <param name="orgPartyId">Organization party identifier</param>
        /// <returns>A detail of profile</returns>
        public ObjectOutput<IProfileDetail, IErrorData> GetUserProfile(Guid realPageId, Guid realpageUserId, long orgPartyId)
        {
            ObjectOutput<IProfileDetail, IErrorData> output = new ObjectOutput<IProfileDetail, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            ManageCustomFields manageCustomFields = new ManageCustomFields(_userClaim);

            realPageId = (realPageId == Guid.Empty) ? realpageUserId : realPageId;
            if (realPageId == Guid.Empty)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "User.GetProfile.1";
                errorStatus.ErrorMsg = "Get User Profile: Invalid parameter realPageId";
                output.Status = errorStatus;
                return output;
            }

            IManagePerson personLogic = new ManagePerson();
            IPerson person = personLogic.GetPerson(realPageId);
            bool isdelegateSettingEnabled = _userRepository.GetUnifiedSettingData("delegateadministrators");
            if (person != null)
            {
                //Include the UserLogin details.  IsActive and Is3rdPartyIDP are used by the Edit User
                IManageUserLogin userLoginLogic = new ManageUserLogin(_userClaim);
                //ManageUserLoginIdentity userLoginIdentity = new ManageUserLoginIdentity();

                var userLogin = userLoginLogic.GetUserLogin(realPageId, orgPartyId); // keep for now, used by ui, need to investigate how
                IManageContactMechanism contactMechanism = new ManageContactMechanism();

                IList<CommonAddress> commonAddressList = contactMechanism.ListContactMechanismForPerson(userLogin.RealPageId, "Email Notification");
                string notificationEmail = null;
                CommonAddress ca = (from a in commonAddressList
                                    where a.contactMechanismUsageType != null
                                    select a).FirstOrDefault();
                if (ca != null)
                {
                    notificationEmail = ca.AddressString;
                }

                IProfileDetail profileDetail = new ProfileDetail()
                {
                    PartyId = person.PartyId,
                    RealPageId = person.RealPageId,
                    Title = person.Title,
                    FirstName = person.FirstName,
                    MiddleName = person.MiddleName,
                    LastName = person.LastName,
                    Suffix = person.Suffix,
                    IsRealPartner = false,
                    contactMechanism = null,
                    SummaryCount = null,
                    AssignedProducts = null,
                    TelecommunicationNumber = null,
                    PartyRole = null,
                    InactivePersona = null,
                    userLogin = userLogin,
                    NotificationEmail = notificationEmail
                };

                IManagePersona managePersona = new ManagePersona(_userClaim);
                RoleTemplate roleTemplate = new RoleTemplate();
                var personaList = managePersona.ListPersona(userLogin.RealPageId);
                if (personaList.Any(p => p.OrganizationPartyId == _userClaim.OrganizationPartyId))
                {
                    var persona = managePersona.GetPersona(personaList.FirstOrDefault(p => p.OrganizationPartyId == _userClaim.OrganizationPartyId).PersonaId);
                    if (persona != null)
                    {
                        profileDetail.Persona.Add(persona);
                        profileDetail.organization.Add(persona.Organization);

                        //Add role template details for persona
                        roleTemplate = _productRepository.GetEnterpriseRoleForPersona(persona.PersonaId);
                        profileDetail.RoleTemplateId = roleTemplate?.RoleTemplateId ?? 0;
                        profileDetail.EntepriseRoleName = roleTemplate?.RoleTemplateName ?? "";
                        profileDetail.PersonaHasProductError = _productRepository.GetPersonaHasProductError(persona.PersonaId);
                    }
                }
                else
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.GetProfile.3";
                    errorStatus.ErrorMsg = "Get User Profile: User exists in a different organization.";
                    output.Status = errorStatus;
                    return output;
                }

                IManageUserLoginPersona manageUserLoginPersona = new ManageUserLoginPersona(_userClaim);
                IList<UserLoginPersona> userLoginPersonaList = manageUserLoginPersona.ListUserLoginPersona(userLoginPersonaId: null, userLoginId: profileDetail.userLogin.UserId, organizationPartyId: _userClaim.OrganizationPartyId);

                IList<CustomFieldValue> customFieldValueList = manageCustomFields.GetCustomFieldsValues(organizationPartyId: _userClaim.OrganizationPartyId, userLoginPersonaId: userLoginPersonaList[0].UserLoginPersonaId, enabled: true);
                profileDetail.CustomFields = customFieldValueList;

                var employeeId = this.GetUserEmployeeId(userLoginPersonaList[0].UserLoginPersonaId, orgPartyId);
                profileDetail.EmployeeId = (employeeId != null && !string.IsNullOrEmpty(employeeId.EmployeeId)) ? employeeId.EmployeeId : null;

                var superVisorInfo = this.GetSuperVisorInformation(profileDetail.userLogin.UserId, orgPartyId);
                profileDetail.SuperVisorUserId = (superVisorInfo != null) ? superVisorInfo.SuperVisorUserId : 0;
                profileDetail.SuperVisorUser = (superVisorInfo != null) ? superVisorInfo : new UserInfoLite();

                IManagePartyRelationship managePartyRelationship = new ManagePartyRelationship();
                PartyRelationship partyRelationship = managePartyRelationship.GetPartyRelationship(realPageId, _userClaim.OrganizationRealPageGuid, "", "", "User Type");
                if (partyRelationship != null)
                {
                    profileDetail.UserTypeId = partyRelationship.RoleTypeIdFrom;
                }

                if (FeatureFlag.GetUserCompanyAssociationFeatureFlag())
                {
                    var data = _userRepository.GetExternalUserRelationship(userLoginPersonaList[0].UserLoginPersonaId);

                    profileDetail.ExternalUserRelationship = data == null ? new ExternalUserRelationship()
                    {
                        UserLoginPersonaId = userLoginPersonaList[0].UserLoginPersonaId
                    } : data;
                }
                if (isdelegateSettingEnabled && userLoginPersonaList[0].IsDelegateAdmin)
                {
                    profileDetail.IsDelegateAdmin = userLoginPersonaList[0].IsDelegateAdmin;
                    var data = _userRepository.GetDelegateAdminRoleTemplate(userLoginPersonaList[0].UserLoginPersonaId);
                    profileDetail.DelegateRoleTemplate = new DelegateRoleTemplate()
                    {
                        RoleTemplateId = data,
                        UserLoginPersonaId = userLoginPersonaList[0].UserLoginPersonaId
                    };

                }
                profileDetail.IsRealPartner = userLoginPersonaList[0].IsRealPartner;


                output.obj = profileDetail;
                output.Status = errorStatus;
                return output;
            }

            errorStatus.Success = false;
            errorStatus.ErrorCode = "User.GetProfile.2";
            errorStatus.ErrorMsg = "Get User Profile: No data.";
            output.Status = errorStatus;

            return output;
        }

        /// <summary>
        /// Get the an UserEmployee by UserLoginPersonaId and OrganizationPartyId
        /// </summary>
        /// <param name="UserLoginPersonaId"></param>
        /// <param name="OrganizationPartyId"></param>
        public IUserEmployeeId GetUserEmployeeId(long UserLoginPersonaId, long OrganizationPartyId)
        {
            return _userRepository.GetUserEmployeeId(UserLoginPersonaId, OrganizationPartyId);
        }

        /// <summary>
        /// Get SuperVisor Information by UserId and OrganizationPartyId
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="OrganizationPartyId"></param>
        public UserInfoLite GetSuperVisorInformation(long UserId, long OrganizationPartyId)
        {
            return _userRepository.GetSuperVisorInformation(UserId, OrganizationPartyId);
        }

        #endregion

        #region Private Methods
        private void LogAuditActivity(string logActivityType, LogActivityCategoryType logActivityCategoryType,
            string message, string stepName, IProfileDetail profile)
        {
            string userName = string.IsNullOrEmpty(_userClaim.ImpersonatedByName) ? _userClaim.FirstName + " " + _userClaim.LastName : " RealPage Access (" + _userClaim.ImpersonatedByName + ") ";
            LogActivity.WriteActivity(new ActivityDetails
            {
                LogActivityTypeName = logActivityType,
                LogCategoryName = logActivityCategoryType.ToString(),
                CorrelationId = _userClaim.CorrelationId.ToString(),
                BooksMasterOrganizationId = _userClaim.OrganizationMasterId,
                OrganizationPartyId = _userClaim.OrganizationPartyId,
                Message = string.Format(message, profile.FirstName, profile.LastName, userName, profile.CreateUserSourceType.ToString()),


                FromUserLoginName = _userClaim.LoginName,
                FromUserLoginId = _userClaim.UserId,
                FromUserRealpageId = _userClaim.UserRealPageGuid.ToString(),
                FromUserFirstName = _userClaim.FirstName,
                FromUserLastName = _userClaim.LastName,

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
            if (_userClaim != null)
            {
                correlationId = (_userClaim.CorrelationId != Guid.Empty) ? _userClaim.CorrelationId.ToString() : "";
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
