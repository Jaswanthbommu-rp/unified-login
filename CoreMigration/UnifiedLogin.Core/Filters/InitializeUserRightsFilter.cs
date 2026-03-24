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
      //  private readonly IBaseUserRightsAsync _baseUserRights;

        public InitializeUserRightsFilter(
            IUserClaimsAccessor userClaimsAccessor)
        {
            _userClaimsAccessor = userClaimsAccessor;
            //_baseUserRights = baseUserRights;
        }
        //public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        //{
        //    if (context.HttpContext.User?.Identity?.IsAuthenticated == true)
        //    {
        //        var userClaim = _userClaimsAccessor.GetUserClaim();
        //        var identity = (ClaimsIdentity)context.HttpContext.User.Identity!;

        //        // Only load rights when they have not yet been stamped on the identity
        //        if (!identity.Claims.Any(c => c.Type.Equals("right", StringComparison.OrdinalIgnoreCase)))
        //        {
        //            var userRights = await _baseUserRights.GetUserRightsAsync(
        //                context.HttpContext.User, userClaim, context.HttpContext.RequestAborted);

        //            if (userRights is { Count: > 0 })
        //            {
        //                identity.AddClaims(userRights.Select(r => new Claim("right", r)));
        //                userClaim.Rights = userRights;
        //            }
        //        }
        //        else if (userClaim.Rights == null || userClaim.Rights.Count == 0)
        //        {
        //            // Claims already stamped — sync the Rights list on the claim object
        //            userClaim.Rights = identity.Claims
        //                .Where(c => c.Type.Equals("right", StringComparison.OrdinalIgnoreCase))
        //                .Select(c => c.Value)
        //                .ToList();
        //        }
        //    }

        //    await next();
        //}
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