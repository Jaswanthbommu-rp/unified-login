using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.TwoFactor;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    public class MultiFactorAuthController : BaseApiController
    {
        private ITwoFactorLogic _twoFactorLogic;
        
        public MultiFactorAuthController()
        {
            // DONT USE USERCLAIM IN BASE, IT IS NULL AT THIS POINT. MOVE TO Initialize FUNCTION
        }
        
        public MultiFactorAuthController(IRepository repository)
        {
            _twoFactorLogic = new TwoFactorLogic(_userClaims, repository);
        }
        
        /// <summary>
        /// Used to initialize DI classes with userclaim
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            _twoFactorLogic = new TwoFactorLogic(_userClaims, null);
        }

        /// <summary>
        /// Used to update information for the AppAuth user
        /// </summary>
        /// <param name="realPageId">The id of the user being edited</param>
        /// <param name="appAuthUser">The settings to add/update</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "AppAuth User Updated")]
        [SwaggerOperation("UpdateUserAppAuth")]
        [Route("multifactorauth/appauth/user/{realPageId}")]
        [HttpPut]
        [AuthorizeScope("rplandingapi")]
        public HttpResponseMessage UpdateUserAppAuth(Guid realPageId, [FromBody] AppAuthUser appAuthUser )
        {
            int result = _twoFactorLogic.UpdateUserTwoFactorStatus(realPageId, appAuthUser.Status );
            if (result == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No records updated");
            }
        
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Used to delete a users App Auth token setup
        /// </summary>
        /// <param name="realPageId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "App Auth token deleted")]
        [SwaggerOperation("DeleteUserAppAuthToken")]
        [Route("multifactorauth/appauth/user/{realPageId}/token")]
        [HttpDelete]
        [AuthorizeScope("rplandingapi")]
        public HttpResponseMessage DeleteUserAppAuthToken(Guid realPageId)
        {
        
            int result = _twoFactorLogic.DeleteUserAppAuthToken(realPageId);
            if (result == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No records deleted");
            }

            //Make sure to reset authentication two factor state to pending
            result = _twoFactorLogic.UpdateUserTwoFactorStatus(realPageId, 2);
            if (result == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No records updated");
            }

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}