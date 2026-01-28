using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.ResponseObject;

namespace UnifiedLogin.LandingAPIEnterprise.Helpers
{
    /// <summary>
    /// Helper class for validating user-related DTOs and data
    /// </summary>
    public class UserValidationHelper
    {
        private readonly ErrorResponse _errorResponse;

        public UserValidationHelper(ErrorResponse errorResponse = null)
        {
            _errorResponse = errorResponse ?? new ErrorResponse { Errors = new List<Error>() };
        }

        /// <summary>
        /// Validates if request is null
        /// </summary>
        public bool ValidateRequestNotNull<T>(T request, string errorMessage = "Null request received.") where T : class
        {
            if (request == null)
            {
                _errorResponse.AddError(errorMessage);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates if realpage user ID is provided (not empty guid)
        /// </summary>
        public bool ValidateUserIdProvided(Guid userId, string fieldName = "UnityRealPageUserId")
        {
            if (userId == Guid.Empty)
            {
                _errorResponse.AddError($"{fieldName} not supplied.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates email and login name match for regular users
        /// </summary>
        public bool ValidateRegularUserEmailLogin(UserTypeDto userType, string email, string loginName)
        {
            if (userType != UserTypeDto.Regular)
                return true;

            if (string.IsNullOrEmpty(email))
            {
                _errorResponse.AddError("Email is required for Regular user type.", 
                    UserControllerConstants.ValidationErrorTitle);
                return false;
            }

            if (!email.Equals(loginName, StringComparison.OrdinalIgnoreCase))
            {
                _errorResponse.AddError("Email and loginName should be same for Regular user type.",
                    UserControllerConstants.ValidationErrorTitle);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates date range (effective date should be before expiration date)
        /// </summary>
        public bool ValidateDateRange(DateTime? effectiveDate, DateTime? expirationDate)
        {
            if (!effectiveDate.HasValue || !expirationDate.HasValue)
                return true;

            if (expirationDate.Value < effectiveDate.Value)
            {
                _errorResponse.AddError("UserExpirationDate should be greater than UserEffectiveDate.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates pagination parameters
        /// </summary>
        public bool ValidatePaginationParameters(int rowsPerPage, int pageNumber)
        {
            if (rowsPerPage <= 0)
            {
                _errorResponse.AddError("rowsPerPage must be 1 or greater.");
                return false;
            }

            if (pageNumber <= 0)
            {
                _errorResponse.AddError("pageNumber must be 1 or greater.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if user type is NoEmail but password is missing
        /// </summary>
        public bool ValidateNoEmailUserHasPassword(UserTypeDto userType, string password)
        {
            if (userType == UserTypeDto.NoEmail && string.IsNullOrEmpty(password))
            {
                _errorResponse.AddError("Password field is required for User type with NoEmail.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if user is in external company
        /// </summary>
        public bool ValidateNotExternalCompany(Guid organizationRealPageId)
        {
            if (organizationRealPageId == UserControllerConstants.ExternalCompanyId)
            {
                _errorResponse.AddError("Cannot create new user in External User company.",
                    UserControllerConstants.ValidationErrorTitle);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates organization access
        /// </summary>
        public bool ValidateOrganizationAccess(long userOrgPartyId, long personaOrgPartyId, bool isImpersonated = false)
        {
            if (userOrgPartyId == 0)
                return true; // Internal API can bypass org check

            if (!isImpersonated && userOrgPartyId != personaOrgPartyId)
            {
                _errorResponse.AddError("Invalid company id");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets all accumulated errors
        /// </summary>
        public ErrorResponse GetErrors()
        {
            return _errorResponse;
        }

        /// <summary>
        /// Checks if any errors have been accumulated
        /// </summary>
        public bool HasErrors()
        {
            return _errorResponse.Errors?.Any() ?? false;
        }

        /// <summary>
        /// Clears all errors
        /// </summary>
        public void ClearErrors()
        {
            _errorResponse.Errors.Clear();
        }

        /// <summary>
        /// Merges validation results into error response
        /// </summary>
        public void AddValidationResults(IEnumerable<ValidationResult> validationResults)
        {
            _errorResponse.AddErrors(
                validationResults.Select(r => r.ToString()),
                UserControllerConstants.ValidationErrorTitle);
        }
    }
}
