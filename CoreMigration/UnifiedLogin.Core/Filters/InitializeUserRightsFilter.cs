using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.Core.Filters
{
    /// <summary>
    /// Action filter to initialize user rights before action execution
    /// </summary>
    public class InitializeUserRightsFilter : IActionFilter
    {
        private readonly IUserClaimsAccessor _userClaimsAccessor;

        public InitializeUserRightsFilter(IUserClaimsAccessor userClaimsAccessor)
        {
            _userClaimsAccessor = userClaimsAccessor;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.User?.Identity?.IsAuthenticated == true)
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var userRights = BaseUserRights.GetUserRightsBy(context.HttpContext.User, userClaim);

                if (userRights != null && userRights.Count > 0)
                {
                    var identity = (ClaimsIdentity)context.HttpContext.User.Identity;

                    // Check if rights are already added to avoid duplicates
                    if (!identity.Claims.Any(c => c.Type.Equals("right", StringComparison.OrdinalIgnoreCase)))
                    {
                        identity.AddClaims(userRights.Select(a => new Claim("right", a)).ToList());
                    }
                }

                // Ensure the rights are set in the user claims
                if (userClaim.Rights == null || userClaim.Rights.Count == 0)
                {
                    userClaim.Rights = userRights;
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No action needed after execution
        }
    }
}