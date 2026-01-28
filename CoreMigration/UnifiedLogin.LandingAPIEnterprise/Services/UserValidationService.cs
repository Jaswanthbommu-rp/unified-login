using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPIEnterprise.Helpers;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.ResponseObject;

namespace UnifiedLogin.LandingAPIEnterprise.Services
{
    /// <summary>
    /// Service for validating user-related data
    /// </summary>
    public interface IUserValidationService
    {
        ErrorResponse ValidateUserProductDetails(UserProductDetailsDto userProductDetailsDto, DefaultUserClaim userClaims);
        void ValidateDateRange(DateTime? effectiveDate, DateTime? expirationDate, ErrorResponse errorResponse);
        void ValidateEmailAndLoginName(UserDataDto userProfile, List<ValidationResult> errorList);
        void ValidateUserTypeRestrictions(UserProductDetailsDto userProductDetailsDto, DefaultUserClaim userClaims, ErrorResponse errorResponse);
        ErrorResponse ValidateSuperUserCreation(UserProductDetailsDto userProductDetailsDto, DefaultUserClaim userClaims);
        ErrorResponse ValidateSuperUserUpdate(UserProductDetailsDto userProductDetailsDto, DefaultUserClaim userClaims, int? existingUserTypeId);
    }

    public class UserValidationService : IUserValidationService
    {
        private readonly ISuperUserValidationService _superUserValidationService;
        private readonly IManageUnifiedSettings _manageSettings;
        private readonly IUserRepository _userRepository;

        public UserValidationService(
            ISuperUserValidationService superUserValidationService,
            IManageUnifiedSettings manageSettings,
            IUserRepository userRepository)
        {
            _superUserValidationService = superUserValidationService ?? throw new ArgumentNullException(nameof(superUserValidationService));
            _manageSettings = manageSettings ?? throw new ArgumentNullException(nameof(manageSettings));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public ErrorResponse ValidateUserProductDetails(UserProductDetailsDto userProductDetailsDto, DefaultUserClaim userClaims)
        {
            var errorResponse = new ErrorResponse { Errors = new List<Error>() };

            if (userProductDetailsDto == null)
            {
                errorResponse.Errors.Add(CreateError("Null request received."));
                return errorResponse;
            }

            if (userClaims.OrganizationRealPageGuid == DefaultUserClaim.ExternalCompanyRealPageId)
            {
                errorResponse.Errors.Add(CreateError("Cannot create new user in External User company."));
                return errorResponse;
            }

            var errorList = DtoValidator.ValidateObject(userProductDetailsDto.UserProfileDetails).ToList();

            if (userProductDetailsDto.ProductList != null)
            {
                foreach (var productDetailDto in userProductDetailsDto.ProductList)
                {
                    errorList.AddRange(DtoValidator.ValidateObject(productDetailDto).ToList());
                }
            }

            ValidateEmailAndLoginName(userProductDetailsDto.UserProfileDetails, errorList);
            ValidateDateRange(
                userProductDetailsDto.UserProfileDetails.UserEffectiveDate,
                userProductDetailsDto.UserProfileDetails.UserExpirationDate,
                errorResponse);

            foreach (var validationError in errorList)
            {
                errorResponse.Errors.Add(new Error
                {
                    Title = UserControllerConstants.ValidationErrorTitle,
                    Source = UserControllerConstants.ErrorSource,
                    Detail = validationError.ToString(),
                    StatusCode = ""
                });
            }

            return errorResponse;
        }

        public ErrorResponse ValidateSuperUserCreation(UserProductDetailsDto userProductDetailsDto, DefaultUserClaim userClaims)
        {
            // Only validate if user is SuperUser type
            if (userProductDetailsDto.UserProfileDetails.UserType != UserTypeDto.SuperUser)
                return null;

            var errorMessage = _superUserValidationService.ValidateSuperUserCreation(
                userClaims.OrganizationRealPageGuid,
                userClaims.OrganizationPartyId,
                _manageSettings,
                _userRepository);

            if (string.IsNullOrEmpty(errorMessage))
                return null;

            return new ErrorResponse
            {
                Errors = new List<Error>
                {
                    new Error
                    {
                        Title = CommonMessageConstants.ErrorTitle,
                        Source = CommonMessageConstants.ErrorSource,
                        Detail = errorMessage,
                        StatusCode = ""
                    }
                }
            };
        }

        public ErrorResponse ValidateSuperUserUpdate(UserProductDetailsDto userProductDetailsDto, DefaultUserClaim userClaims, int? existingUserTypeId)
        {
            // Only validate if user is being changed to SuperUser type
            if (userProductDetailsDto.UserProfileDetails.UserType != UserTypeDto.SuperUser)
                return null;

            // Get the new user type ID
            var newUserTypeId = GetGbUserType(userProductDetailsDto.UserProfileDetails.UserType);

            var errorMessage = _superUserValidationService.ValidateSuperUserPromotion(
                userClaims.OrganizationRealPageGuid,
                userClaims.OrganizationPartyId,
                existingUserTypeId,
                newUserTypeId,
                _manageSettings,
                _userRepository);

            if (string.IsNullOrEmpty(errorMessage))
                return null;

            return new ErrorResponse
            {
                Errors = new List<Error>
                {
                    new Error
                    {
                        Title = UserControllerConstants.ErrorTitle,
                        Source = UserControllerConstants.ErrorSource,
                        Detail = errorMessage,
                        StatusCode = ""
                    }
                }
            };
        }

        public void ValidateDateRange(DateTime? effectiveDate, DateTime? expirationDate, ErrorResponse errorResponse)
        {
            if (effectiveDate.HasValue && expirationDate.HasValue && expirationDate.Value < effectiveDate.Value)
            {
                errorResponse.Errors.Add(CreateError("UserExpirationDate should be greater than UserEffectiveDate."));
            }
        }

        public void ValidateEmailAndLoginName(UserDataDto userProfile, List<ValidationResult> errorList)
        {
            if (string.IsNullOrEmpty(userProfile.LoginName))
                return;

            if (userProfile.UserType != UserTypeDto.Regular)
                return;

            if (string.IsNullOrEmpty(userProfile.Email))
            {
                errorList.Add(new ValidationResult("Email is required for Regular user type."));
            }
            else if (!userProfile.Email.Equals(userProfile.LoginName, StringComparison.OrdinalIgnoreCase))
            {
                errorList.Add(new ValidationResult("Email and loginName should be same for Regular user type."));
            }
        }

        public void ValidateUserTypeRestrictions(UserProductDetailsDto userProductDetailsDto, DefaultUserClaim userClaims, ErrorResponse errorResponse)
        {
            if (userProductDetailsDto.UserProfileDetails.UserType != UserTypeDto.SuperUser || userClaims.OrganizationRealPageGuid == Guid.Empty)
                return;

            // SuperUser validation logic can be added here if needed
        }

        private static Error CreateError(string detail)
        {
            return new Error
            {
                Title = UserControllerConstants.ErrorTitle,
                Source = UserControllerConstants.ErrorSource,
                Detail = detail,
                StatusCode = ""
            };
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
    }
}
