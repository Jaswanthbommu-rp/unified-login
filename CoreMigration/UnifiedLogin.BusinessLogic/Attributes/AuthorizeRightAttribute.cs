using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.Landing;
using Serilog;
using Serilog.Events;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace UnifiedLogin.BusinessLogic.Attributes
{
    /// <summary>
    /// Check if user has right for a particular functionality
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeRightAttribute : AuthorizeAttribute
    {
        #region Private Variables Ctor

        private readonly string[] _rightToCheck;

        #endregion

        /// <summary>
        /// AuthorizeRight Attribute
        /// </summary>
        public AuthorizeRightAttribute(params string[] right)
        {
            this._rightToCheck = right;
        }

        #region Public / Protected methods

        /// <summary>
        /// Returns if request is authorized or not
        /// </summary>
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            bool isAuthorized = base.IsAuthorized(actionContext);
            return isAuthorized;
        }

        /// <summary>
        /// Triggers in authorization
        /// </summary>
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var claimDetails = GetClaims();

            //Perform your logic here
            if (base.IsAuthorized(actionContext))
            {
                try
                {
                    if (claimDetails == null || claimDetails.OrganizationPartyId <= 0 || string.IsNullOrEmpty(claimDetails.Roles)) return;
                    if (_rightToCheck == null || !_rightToCheck.Any()) return;
                    int rightMatchedCnt = 0;
                    foreach (var right in _rightToCheck)
                    {
                        // Check user has access to right
                        if (CheckUserRight.CheckUserHasAccess(claimDetails.Rights, right))
                        {
                            rightMatchedCnt++;
                        }
                    }

                    if (rightMatchedCnt > 0)
                    {
                        Log.Write(LogEventLevel.Debug, "{ActionName} - {state}", propertyValue0: "OnAuthorization", propertyValue1: $"User right has been verified. Roles - {claimDetails.Roles} Right - {ConvertStringArrayToStringJoin(_rightToCheck)}");
                    }
                    else
                    {
                        Log.Write(LogEventLevel.Debug, "{ActionName} - {state}", propertyValue0: "OnAuthorization", propertyValue1: $"User right has not verified. Roles - {claimDetails.Roles} Right - {ConvertStringArrayToStringJoin(_rightToCheck)}");

                        // handle unauthorized request
                        this.HandleUnauthorizedRequest(actionContext);
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        Log.Write(LogEventLevel.Error, ex, "{ActionName} - {state}", propertyValue0: "OnAuthorization", propertyValue1: $"Error while evaluating user rights. Roles - {claimDetails?.Roles} Right - {ConvertStringArrayToStringJoin(_rightToCheck)}");

                        this.HandleUnauthorizedRequest(actionContext);
                    }
                    finally
                    {
                    }
                }
            }
            else
            {
                base.OnAuthorization(actionContext);
            }
        }

        /// <summary>
        /// 
        /// </summary> 
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            if (base.IsAuthorized(actionContext))
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden);
                actionContext.Response.ReasonPhrase = "The server understood the request but refuses to authorize it.(No Right)";
            }
            else
            {
                base.HandleUnauthorizedRequest(actionContext);
            }
        }

        #endregion

        #region Private Methods

        // Get claims for a user
        private DefaultUserClaim GetClaims()
        {
            if (HttpContext.Current.User.Identity != null)
            {
                return new DefaultUserClaim(ClaimsPrincipal.Current);
            }

            return null;
        }

        private static string ConvertStringArrayToStringJoin(string[] array)
        {
            // Use string Join to concatenate the string elements.
            string result = string.Join(",", array);
            return result;
        }
        #endregion
    }

}