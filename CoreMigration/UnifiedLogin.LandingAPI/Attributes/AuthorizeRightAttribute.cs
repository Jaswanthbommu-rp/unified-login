using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Attributes
{
    /// <summary>
    /// Check if user has right for a particular functionality
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeRightAttribute : TypeFilterAttribute
    {
        public AuthorizeRightAttribute(params string[] rights) : base(typeof(AuthorizeRightFilter))
        {
            Arguments = new object[] { rights };
        }

        private class AuthorizeRightFilter : IAuthorizationFilter
        {
            private readonly string[] _rightsToCheck;
            private readonly ILogger<AuthorizeRightFilter> _logger;

            public AuthorizeRightFilter(string[] rights, ILogger<AuthorizeRightFilter> logger)
            {
                _rightsToCheck = rights;
                _logger = logger;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                // Check if action has [AllowAnonymous]
                if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
                {
                    return;
                }

                // Check if user is authenticated
                if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // Get user claims
                var claimDetails = GetClaims(context.HttpContext.User);

                if (claimDetails == null || claimDetails.OrganizationPartyId <= 0 || string.IsNullOrEmpty(claimDetails.Roles))
                {
                    return;
                }

                if (_rightsToCheck == null || !_rightsToCheck.Any())
                {
                    return;
                }

                try
                {
                    int rightMatchedCount = 0;
                    foreach (var right in _rightsToCheck)
                    {
                        // Check user has access to right
                        if (CheckUserRight.CheckUserHasAccess(claimDetails.Rights, right))
                        {
                            rightMatchedCount++;
                        }
                    }

                    if (rightMatchedCount > 0)
                    {
                        _logger.LogDebug("User right has been verified. Roles - {Roles} Right - {Rights}",
                            claimDetails.Roles, string.Join(",", _rightsToCheck));
                    }
                    else
                    {
                        _logger.LogDebug("User right has not been verified. Roles - {Roles} Right - {Rights}",
                            claimDetails.Roles, string.Join(",", _rightsToCheck));

                        context.Result = new ObjectResult("The server understood the request but refuses to authorize it. (No Right)")
                        {
                            StatusCode = StatusCodes.Status403Forbidden
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while evaluating user rights. Roles - {Roles} Right - {Rights}",
                        claimDetails?.Roles, string.Join(",", _rightsToCheck));

                    context.Result = new ObjectResult("The server understood the request but refuses to authorize it. (No Right)")
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
            }

            private DefaultUserClaim? GetClaims(ClaimsPrincipal user)
            {
                if (user?.Identity?.IsAuthenticated == true)
                {
                    return new DefaultUserClaim(user);
                }

                return null;
            }
        }
    }
}
