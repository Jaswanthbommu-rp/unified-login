using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPIEnterprise.Helpers;
using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.LandingAPIEnterprise.Services
{
    /// <summary>
    /// Service for validating SuperUser constraints
    /// </summary>
    public interface ISuperUserValidationService
    {
        /// <summary>
        /// Validates if a new super user can be created based on SMB settings
        /// </summary>
        /// <returns>Error message if validation fails, null if valid</returns>
        string ValidateSuperUserCreation(Guid organizationRealPageGuid, long organizationPartyId, 
            IManageUnifiedSettings settingsService, IUserRepository userRepository);

        /// <summary>
        /// Validates if a user can be promoted to super user based on SMB settings
        /// </summary>
        string ValidateSuperUserPromotion(Guid organizationRealPageGuid, long organizationPartyId,
            int? currentUserTypeId, int newUserTypeId, IManageUnifiedSettings settingsService, 
            IUserRepository userRepository);
    }

    /// <summary>
    /// Implementation of ISuperUserValidationService
    /// </summary>
    public class SuperUserValidationService : ISuperUserValidationService
    {
        /// <summary>
        /// Validates if a new super user can be created based on SMB settings
        /// </summary>
        public string ValidateSuperUserCreation(Guid organizationRealPageGuid, long organizationPartyId,
            IManageUnifiedSettings settingsService, IUserRepository userRepository)
        {
            if (organizationRealPageGuid == Guid.Empty)
                return null;

            if (!IsSmbEnabled(organizationRealPageGuid, settingsService))
                return null;

            var superUserCount = userRepository.GetSuperUserCountByOrganizationAsync(organizationPartyId);
            if (superUserCount >= UserControllerConstants.MaxSuperUsersPerOrganization)
            {
                return "You have reached the maximum number of System Administrators " +
                       $"({UserControllerConstants.MaxSuperUsersPerOrganization}). " +
                       "Please update the User Relationship of the user.";
            }

            return null;
        }

        /// <summary>
        /// Validates if a user can be promoted to super user based on SMB settings
        /// </summary>
        public string ValidateSuperUserPromotion(Guid organizationRealPageGuid, long organizationPartyId,
            int? currentUserTypeId, int newUserTypeId, IManageUnifiedSettings settingsService,
            IUserRepository userRepository)
        {
            // Only validate if user is being promoted to super user
            if (newUserTypeId != (int)UserRoleType.SuperUser)
                return null;

            // If already a super user, no restriction
            if (currentUserTypeId == (int)UserRoleType.SuperUser)
                return null;

            return ValidateSuperUserCreation(organizationRealPageGuid, organizationPartyId, 
                settingsService, userRepository);
        }

        /// <summary>
        /// Checks if SMB setting is enabled for the organization
        /// </summary>
        private bool IsSmbEnabled(Guid organizationRealPageGuid, IManageUnifiedSettings settingsService)
        {
            var data = settingsService.GetCompanyInternalSettings(organizationRealPageGuid, "UPFM", "SMB");
            var isEnableSmb = data?.Keys?.FirstOrDefault(k => 
                k.Name.Equals(UserControllerConstants.SmbSettingName, StringComparison.OrdinalIgnoreCase));
            
            return isEnableSmb?.Value == UserControllerConstants.SmbEnabledValue;
        }
    }
}
