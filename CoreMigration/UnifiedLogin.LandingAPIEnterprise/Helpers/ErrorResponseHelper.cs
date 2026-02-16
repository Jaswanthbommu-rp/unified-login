using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.SharedObjects.ResponseObject;
using Microsoft.AspNetCore.Mvc;

namespace UnifiedLogin.LandingAPIEnterprise.Helpers
{
    /// <summary>
    /// Helper class for creating standardized error responses
    /// </summary>
    public static class ErrorResponseHelper
    {
        /// <summary>
        /// Creates a single error response
        /// </summary>
        public static ErrorResponse CreateError(string detail, string title = UserControllerConstants.ErrorTitle, 
            string source = UserControllerConstants.ErrorSource)
        {
            return new ErrorResponse
            {
                Errors = new List<Error>
                {
                    new Error
                    {
                        Title = title,
                        Source = source,
                        Detail = detail,
                        StatusCode = ""
                    }
                }
            };
        }

        /// <summary>
        /// Creates a validation error response
        /// </summary>
        public static ErrorResponse CreateValidationError(string detail, string source = UserControllerConstants.ErrorSource)
        {
            return CreateError(detail, UserControllerConstants.ValidationErrorTitle, source);
        }

        /// <summary>
        /// Creates a multi-error response
        /// </summary>
        public static ErrorResponse CreateErrors(IEnumerable<string> details, string title = UserControllerConstants.ErrorTitle,
            string source = UserControllerConstants.ErrorSource)
        {
            return new ErrorResponse
            {
                Errors = details
                    .Select(d => new Error
                    {
                        Title = title,
                        Source = source,
                        Detail = d,
                        StatusCode = ""
                    })
                    .ToList()
            };
        }

        /// <summary>
        /// Creates a multi-error response from validation results
        /// </summary>
        public static ErrorResponse CreateValidationErrors(IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> validationResults,
            string source = UserControllerConstants.ErrorSource)
        {
            return new ErrorResponse
            {
                Errors = validationResults
                    .Select(r => new Error
                    {
                        Title = UserControllerConstants.ValidationErrorTitle,
                        Source = source,
                        Detail = r.ToString(),
                        StatusCode = ""
                    })
                    .ToList()
            };
        }

        /// <summary>
        /// Appends an error to existing error response
        /// </summary>
        public static void AddError(this ErrorResponse response, string detail, string title = UserControllerConstants.ErrorTitle,
            string source = UserControllerConstants.ErrorSource)
        {
            response.Errors.Add(new Error
            {
                Title = title,
                Source = source,
                Detail = detail,
                StatusCode = ""
            });
        }

        /// <summary>
        /// Appends multiple errors to existing error response
        /// </summary>
        public static void AddErrors(this ErrorResponse response, IEnumerable<string> details,
            string title = UserControllerConstants.ErrorTitle, string source = UserControllerConstants.ErrorSource)
        {
            foreach (var detail in details)
            {
                response.AddError(detail, title, source);
            }
        }

        /// <summary>
        /// Creates a BadRequest result with error response
        /// </summary>
        public static IActionResult ToBadRequest(this ErrorResponse response)
        {
            return new BadRequestObjectResult(response);
        }
    }
}
