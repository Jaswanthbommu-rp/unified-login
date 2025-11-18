using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace UnifiedLogin.LandingAPI.Attributes
{
    /// <summary>
    /// Used to secure a controller using scopes
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AuthorizeScopeAttribute : TypeFilterAttribute
    {
        public AuthorizeScopeAttribute(params string[] scopes) : base(typeof(AuthorizeScopeFilter))
        {
            Arguments = new object[] { scopes };
        }

        private class AuthorizeScopeFilter : IAuthorizationFilter
        {
            private readonly string[] _scopesToCheck;

            public AuthorizeScopeFilter(string[] scopes)
            {
                _scopesToCheck = scopes;
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

                // Check scopes
                var userClaims = context.HttpContext.User.Claims.ToList();
                bool hasAllowedScope = false;

                foreach (var scope in _scopesToCheck)
                {
                    if (userClaims.Any(c => c.Type.Equals("Scope", StringComparison.OrdinalIgnoreCase) &&
                                           c.Value.Equals(scope, StringComparison.OrdinalIgnoreCase)))
                    {
                        hasAllowedScope = true;
                        break;
                    }
                }

                if (!hasAllowedScope)
                {
                    context.Result = new ObjectResult("The server understood the request but refuses to authorize it. (No Scope)")
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
            }
        }
    }
}
