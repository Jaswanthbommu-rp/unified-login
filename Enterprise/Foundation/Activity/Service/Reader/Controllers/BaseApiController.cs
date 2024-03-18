using System;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;
using RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Models;
using Serilog;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Controllers
{
    [AllowCors, Authorize]
    //[AllowCors, AllowAnonymous]
    public class BaseApiController : ApiController
    {
        /// <summary>
        /// Holds default user claim related information
        /// </summary>
        public DefaultUserClaim _userClaims;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseApiController()
        {

        }

        /// <summary>
        /// Used to initialize the base controller and retrieve the needed information used by the infrastructure
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            if (currentClaimPrincipal.Identity.IsAuthenticated)
            {
                _userClaims = new DefaultUserClaim(currentClaimPrincipal);
            }
        }

        /// <summary>
        /// Used to write exceptions to the elastic log
        /// </summary>
        protected void WriteToErrorLog(Exception exception = null)
        {
            Log.Error(exception: exception, messageTemplate: "{methodName} - {state}", propertyValues: new object[] { "WriteToErrorLog", $"Exception in activity reader - Reason: {exception?.Message}. User: {_userClaims.UserRealPageGuid}. PmcId: {_userClaims.OrganizationPartyId} . CorrelationId: {_userClaims.CorrelationId}" });
        }
    }
}