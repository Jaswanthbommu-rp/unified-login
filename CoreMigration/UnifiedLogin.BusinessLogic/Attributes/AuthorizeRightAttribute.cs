using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.Landing;
using Serilog;
using Serilog.Events;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace UnifiedLogin.BusinessLogic.Attributes
{
    /// <summary>
    /// Checks that the authenticated user possesses at least one of the specified rights.
    /// Returns 401 if not authenticated, 403 if authenticated but lacks required rights.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeRightAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _rightsToCheck;

        public AuthorizeRightAttribute(params string[] rights)
        {
            _rightsToCheck = rights ?? Array.Empty<string>();
        }

        /// <summary>
        /// Core authorization logic executed early in the MVC pipeline.
        /// </summary>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            // Skip if endpoint allows anonymous access
            if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
            {
                return;
            }

            var user = context.HttpContext.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // If no rights specified, treat as authenticated-only requirement.
            if (_rightsToCheck.Length == 0)
            {
                return;
            }

            // Retrieve IUserClaimsAccessor from DI to get claims with rights loaded from database
            var userClaimsAccessor = context.HttpContext.RequestServices.GetService<IUserClaimsAccessor>();
            DefaultUserClaim claimDetails = userClaimsAccessor?.GetUserClaim();
            
            if (claimDetails == null || claimDetails.OrganizationPartyId <= 0 || string.IsNullOrEmpty(claimDetails.Roles))
            {
                // Not enough claim data -> forbid.
                context.Result = new ForbidResult();
                return;
            }

            var userRights = claimDetails.Rights ?? new System.Collections.Generic.List<string>();
            int matchedCount = 0;
            foreach (var right in _rightsToCheck)
            {
                try
                {
                    if (CheckUserRight.CheckUserHasAccess(userRights, right))
                    {
                        matchedCount++;
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(LogEventLevel.Error, ex, "{ActionName} - {state}", propertyValue0: "AuthorizeRight", propertyValue1: $"Error while evaluating right '{right}'. Roles={claimDetails.Roles}");
                }
            }

            if (matchedCount > 0)
            {
                Log.Write(LogEventLevel.Debug, "{ActionName} - {state}", propertyValue0: "AuthorizeRight", propertyValue1: $"User right verified. Roles={claimDetails.Roles} Rights={ConvertStringArrayToStringJoin(_rightsToCheck)} Matched={matchedCount}");
                return; // success
            }

            Log.Write(LogEventLevel.Debug, "{ActionName} - {state}", propertyValue0: "AuthorizeRight", propertyValue1: $"User right NOT verified. Roles={claimDetails.Roles} Rights={ConvertStringArrayToStringJoin(_rightsToCheck)}");
            context.Result = new ForbidResult();
        }

        private static string ConvertStringArrayToStringJoin(string[] array) => string.Join(",", array ?? Array.Empty<string>());
    }
}