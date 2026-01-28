using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.User;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.User.Models;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.ResponseObject;

namespace UnifiedLogin.LandingAPIEnterprise.Services
{
    /// <summary>
    /// Service for user management operations (create, update, activate/deactivate)
    /// </summary>
    public interface IUserManagementService
    {
        Task<ObjectResponse> CreateUserAsync(UserProductDetailsDto userProductDetailsDto, DefaultUserClaim userClaims);
        Task<ObjectResponse> UpdateUserAsync(UserProductDetailsDto userProductDetailsDto, DefaultUserClaim userClaims);
        Task<ObjectResponse> ChangeUserStatusAsync(Guid unityRealPageUserId, EntApiUserStatus userStatusToChange, DefaultUserClaim userClaims);
        Task<ChangeCompanyResult> ChangeCompanyAsync(long personaId, DefaultUserClaim userClaims);
        Task<string> DeleteSamlUserProductInfoAndStatusAsync(ProductUserAccountDetails productUser);
        Task<string> UpdateProductUserAccountDetailsAsync(ProductUserAccountDetails productUser);
    }

    public class UserManagementService : IUserManagementService
    {
        private readonly IUserValidationService _validationService;
        private readonly IUserProfileService _profileService;
        private readonly IManagePersona _managePersona;
        private readonly IManageProduct _manageProduct;
        private readonly IManageOrganization _manageOrganization;
        private readonly IManageUnifiedSettings _manageSettings;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IManageUserLogin _userLoginLogic;
        private readonly IManageProductPanel _manageProductPanel;
        private readonly IManageProductUser _manageProductUser;
        private readonly UserManagement _userManagement;

        public UserManagementService(
            IUserValidationService validationService,
            IUserProfileService profileService,
            IManagePersona managePersona,
            IManageProduct manageProduct,
            IManageOrganization manageOrganization,
            IManageUnifiedSettings manageSettings,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IManageUserLogin userLoginLogic,
            IManageProductPanel manageProductPanel,
            IManageProductUser manageProductUser,
            UserManagement userManagement)
        {
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _manageProduct = manageProduct ?? throw new ArgumentNullException(nameof(manageProduct));
            _manageOrganization = manageOrganization ?? throw new ArgumentNullException(nameof(manageOrganization));
            _manageSettings = manageSettings ?? throw new ArgumentNullException(nameof(manageSettings));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userLoginLogic = userLoginLogic ?? throw new ArgumentNullException(nameof(userLoginLogic));
            _manageProductPanel = manageProductPanel ?? throw new ArgumentNullException(nameof(manageProductPanel));
            _manageProductUser = manageProductUser ?? throw new ArgumentNullException(nameof(manageProductUser));
            _userManagement = userManagement ?? throw new ArgumentNullException(nameof(userManagement));
        }

        public async Task<ObjectResponse> CreateUserAsync(UserProductDetailsDto userProductDetailsDto, DefaultUserClaim userClaims)
        {
            // Validate SuperUser count if applicable
            await ValidateSuperUserCountAsync(userProductDetailsDto, userClaims, isUpdate: false);

            // Set default dates if not supplied
            userProductDetailsDto.UserProfileDetails.UserEffectiveDate ??= DateTime.UtcNow;
            userProductDetailsDto.UserProfileDetails.UserExpirationDate ??= new DateTime(9999, 12, 31);

            // Validate product data
            var userProductDetails = _profileService.GetUserBusinessObject(userProductDetailsDto, userClaims);
            var productErrors = _userManagement.ValidateProductData(userProductDetails.ProductList);
            if (productErrors.Any())
            {
                throw new ValidationException(string.Join("; ", productErrors));
            }

            // Check if User type is NoEmail and no password
            if (GetGbUserType(userProductDetailsDto.UserProfileDetails.UserType) == UserTypeConstants.RegularUserNoEmail 
                && string.IsNullOrEmpty(userProductDetailsDto.UserProfileDetails.Password))
            {
                throw new ValidationException("Password field is required for User type with NoEmail.");
            }

            // Check if user is available in other company
            await ValidateUserTypeConsistencyAsync(userProductDetailsDto.UserProfileDetails.LoginName, userProductDetailsDto.UserProfileDetails.UserType);

            // Validate and assign custom fields
            var (customFields, customFieldError) = ValidateCustomFields(userProductDetailsDto.UserProfileDetails.CustomFields, null);
            if (!string.IsNullOrEmpty(customFieldError))
            {
                throw new ValidationException(customFieldError);
            }

            // Build profile
            var profile = _profileService.BuildProfileFromDto(
                userProductDetailsDto, 
                customFields, 
                userClaims, 
                _manageOrganization, 
                _managePersona, 
               // _userLoginLogic, 
                _productRepository, 
                _manageProductPanel);

            // Create default persona
            var userPersona = await CreateDefaultPersonaAsync(profile, userClaims);
            profile.Persona = userPersona;

            if (profile.organization.Count == 0)
            {
                var persona = _managePersona.GetFirstAvailablePersonaByCompany(userClaims.UserRealPageGuid, userClaims.OrganizationPartyId);
                profile.organization.Add(persona.Organization);
            }

            // Validate domain
            if (!_userLoginLogic.IsUserEmailDomainValid(profile.userLogin.LoginName))
            {
                throw new ValidationException("Email domain is not allowed.");
            }

            // Create user
            var manageUser = new ManageUser(userClaims);
            var response = manageUser.CreateUser(profile, userPersona, true);

            if (!response.Status.Success)
            {
                throw new InvalidOperationException($"{response?.Status?.ErrorMsg} {response?.Status?.ErrorData}");
            }

            return new ObjectResponse 
            { 
                Data = response.UserRealPageGuid, 
                IsError = false, 
                ErrorReason = null 
            };
        }

        public async Task<ObjectResponse> UpdateUserAsync(UserProductDetailsDto userProductDetailsDto, DefaultUserClaim userClaims)
        {
            // Validate product data
            var userProductDetails = _profileService.GetUserBusinessObject(userProductDetailsDto, userClaims);
            var productErrors = _userManagement.ValidateProductData(userProductDetails.ProductList);
            if (productErrors.Any())
            {
                throw new ValidationException(string.Join("; ", productErrors));
            }

            // Validate and assign custom fields
            var (customFields, customFieldError) = ValidateCustomFields(userProductDetailsDto.UserProfileDetails.CustomFields, null);
            if (!string.IsNullOrEmpty(customFieldError))
            {
                throw new ValidationException(customFieldError);
            }

            // Build profile
            var profile = _profileService.BuildProfileFromDto(
                userProductDetailsDto, 
                customFields, 
                userClaims, 
                _manageOrganization, 
                _managePersona, 
               // _userLoginLogic, 
                _productRepository, 
                _manageProductPanel);

            var userDetails = _userRepository.GetUserDetails(userClaims.PersonaId, null);

            // Validate SuperUser count if changing to SuperUser
            await ValidateSuperUserCountAsync(userProductDetailsDto, userClaims, isUpdate: true, profile.UserTypeId);

            // Validate domain
            if (!_userLoginLogic.IsUserEmailDomainValid(profile.userLogin.LoginName))
            {
                throw new ValidationException("Email domain is not allowed.");
            }

            // Update user
            var manageUser = new ManageUser(userClaims);
            var response = manageUser.UpdateUser(userDetails.UserRealPageId, profile);

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                throw new InvalidOperationException(response.ErrorMessage);
            }

            return new ObjectResponse 
            { 
                Data = response.RealPageId, 
                IsError = false, 
                ErrorReason = null 
            };
        }

        public async Task<ObjectResponse> ChangeUserStatusAsync(Guid unityRealPageUserId, EntApiUserStatus userStatusToChange, DefaultUserClaim userClaims)
        {
            var statusTypeName = userStatusToChange switch
            {
                EntApiUserStatus.Activate => UserUiStatusType.Active,
                EntApiUserStatus.Deactivate => UserUiStatusType.Disabled,
                _ => throw new ArgumentException("Invalid parameter: Incorrect userStatusToChange supplied.")
            };

            if (unityRealPageUserId == userClaims.UserRealPageGuid)
            {
                throw new InvalidOperationException("Invalid parameter: Cannot update API logged-in user's status.");
            }

            if (unityRealPageUserId == Guid.Empty)
            {
                throw new ArgumentException("Invalid parameter: unityRealPageUserId.");
            }

            var response = _userManagement.ActivateDeactivateUser(unityRealPageUserId, statusTypeName);

            if (!string.IsNullOrEmpty(response.ErrorReason))
            {
                throw new InvalidOperationException(response.ErrorReason);
            }

            return new ObjectResponse { Data = response.Data };
        }

        public async Task<ChangeCompanyResult> ChangeCompanyAsync(long personaId, DefaultUserClaim userClaims)
        {
            // Get current user's persona
            var currentPersona = _managePersona.GetPersona(userClaims.PersonaId);
            
            if (currentPersona == null)
            {
                return new ChangeCompanyResult 
                { 
                    IsSuccess = false, 
                    ErrorMessage = "Current persona not found." 
                };
            }

            // Get list of active personas for the user
            var personaList = _managePersona.ListActivePersona(currentPersona.RealPageId, false);
            
            // Validate that the target persona belongs to the current user
            if (!personaList.Any(p => p.PersonaId == personaId))
            {
                throw new UnauthorizedAccessException("The specified persona does not belong to the current user.");
            }

            // Trigger company change notification
            var notificationResult = _managePersona.ChangeCompanyNotification(personaId);
            
            if (notificationResult == Guid.Empty)
            {
                return new ChangeCompanyResult 
                { 
                    IsSuccess = false, 
                    ErrorMessage = "Failed to change company. Notification could not be sent." 
                };
            }

            return new ChangeCompanyResult 
            { 
                IsSuccess = true, 
                NotificationId = notificationResult 
            };
        }

        public async Task<string> DeleteSamlUserProductInfoAndStatusAsync(ProductUserAccountDetails productUser)
        {
            if (productUser == null)
            {
                throw new ArgumentNullException(nameof(productUser), "ProductUser cannot be null.");
            }

            if (productUser.ProductId <= 0)
            {
                throw new ArgumentException("ProductId must be greater than 0.", nameof(productUser));
            }

            // Delete SAML user product information
            var result = _manageProductUser.DeleteSamlUserProductInfoAndStatus(productUser, true);

            // Return success message if result is empty
            return string.IsNullOrEmpty(result) ? "Success" : result;
        }

        public async Task<string> UpdateProductUserAccountDetailsAsync(ProductUserAccountDetails productUser)
        {
            if (productUser == null)
            {
                throw new ArgumentNullException(nameof(productUser), "ProductUser cannot be null.");
            }

            if (productUser.ProductId <= 0)
            {
                throw new ArgumentException("ProductId must be greater than 0.", nameof(productUser));
            }

            // Update product user account details
            var result = _manageProductUser.UpdateProductUserAccountDetails(productUser,  true);

            // Return success message if result is empty
            return string.IsNullOrEmpty(result) ? "Success" : result;
        }

        #region Private Helper Methods

        private async Task ValidateSuperUserCountAsync(UserProductDetailsDto userProductDetailsDto, DefaultUserClaim userClaims, bool isUpdate, int? currentUserTypeId = null)
        {
            if (userProductDetailsDto.UserProfileDetails.UserType != UserTypeDto.SuperUser || userClaims.OrganizationRealPageGuid == Guid.Empty)
                return;

            var data = _manageSettings.GetCompanyInternalSettings(userClaims.OrganizationRealPageGuid, "UPFM", "SMB");
            var isEnableSmb = data?.Keys?.FirstOrDefault(k => k.Name.Equals("enablesmb", StringComparison.OrdinalIgnoreCase));

            if (isEnableSmb?.Value != "1")
                return;

            bool isUserTypeChangedToSuperUser = false;
            if (isUpdate && currentUserTypeId.HasValue)
            {
                var existingUserDetails = _userRepository.GetUserDetails(null, userProductDetailsDto.UserProfileDetails.UnityRealPageUserId.ToString());
                isUserTypeChangedToSuperUser = existingUserDetails != null 
                    && existingUserDetails.UserRoleTypeId != (int)UserRoleType.SuperUser 
                    && currentUserTypeId.Value == (int)UserRoleType.SuperUser;
            }

            if (!isUpdate || isUserTypeChangedToSuperUser)
            {
                var superUserCount = _userRepository.GetSuperUserCountByOrganizationAsync(userClaims.OrganizationPartyId);
                if (superUserCount >= 2)
                {
                    throw new ValidationException("You have reached the maximum number of System Administrators (2). Please update the User Relationship of the user.");
                }
            }
        }

        private async Task ValidateUserTypeConsistencyAsync(string loginName, UserTypeDto userType)
        {
            var roleTypeLogic = new ManageRoleType(new RoleTypeRepository());
            var roleTypeList = (List<RoleType>)roleTypeLogic.GetRoleType(
                roleTypeName: "user role", 
                partyId: null, 
                orgMasterId: null, 
                loginName: loginName);
            
            var userTypeId = GetGbUserType(userType);
            if (!roleTypeList.Any(x => x.PartyRoleTypeId == userTypeId))
            {
                throw new ValidationException("User type with Regular already exists. Please create user type as an External.");
            }
        }

        private (IList<CustomFieldValue> customFields, string error) ValidateCustomFields(Dictionary<string, string> customFieldsDict, long? userLoginPersonaId)
        {
            IList<CustomFieldValue> userCustomFields = null;
            string customFieldsData = _userManagement.ValidateAndAssignCustomFieldValues(
                null, 
                customFieldsDict, 
                out userCustomFields);
            
            return (userCustomFields, customFieldsData);
        }

        private async Task<List<Persona>> CreateDefaultPersonaAsync(ProfileDetail profile, DefaultUserClaim userClaims)
        {
            var personaEnvironment = _managePersona.GetPersonaEnvironmentType();
            var personaEnv = personaEnvironment.SingleOrDefault(p => p.Name == "Production");
            
            if (personaEnv == null)
            {
                throw new InvalidOperationException("Persona environment is missing.");
            }

            var userPersona = new List<Persona>
            {
                new Persona
                {
                    Name = profile.UserTypeId == (int)UserRoleType.SuperUser ? "System Administrator" : "Primary",
                    PersonaEnvironmentTypeId = (int)personaEnv.PersonaEnvironmentTypeId,
                    FromDate = DateTime.UtcNow,
                    ThruDate = null
                }
            };

            return userPersona;
        }

        private int GetGbUserType(UserTypeDto userTypeDto)
        {
            return userTypeDto switch
            {
                UserTypeDto.Regular => (int)UserRoleType.User,
                UserTypeDto.NoEmail => (int)UserRoleType.UserNoEmail,
                UserTypeDto.Employee => (int)UserRoleType.RealPageEmployee,
                UserTypeDto.External => (int)UserRoleType.ExternalUser,
                UserTypeDto.SuperUser => (int)UserRoleType.SuperUser,
                _ => throw new ArgumentOutOfRangeException(nameof(userTypeDto), userTypeDto, null)
            };
        }

        #endregion
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }

    /// <summary>
    /// Result object for company change operation
    /// </summary>
    public class ChangeCompanyResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public Guid? NotificationId { get; set; }
    }
}
