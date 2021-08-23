using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Security;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.Security;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    /// <summary>
    /// Controller to provide necessary api's for access rights
    /// </summary>
    [Authorize]
    public class AccessController : BaseApiController
    {
        #region Ctor
        /// <summary>
        /// 
        /// </summary>
        public AccessController() { }

        #endregion

        /// <summary>
        /// List of right a user can have per page
        /// </summary>
        /// <param name="routeId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when route id have invalid values)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List of right a user can have per page", Type = typeof(RouteSecurity))]
        [SwaggerResponseExamples(typeof(RouteSecurity), typeof(RouteSecurityExample))]
        [HttpGet]
        [Route("{routeId}/rights")]
        public HttpResponseMessage GetRights(string routeId)
        {
            IManageSecurity _manangeSecurityLogic = new ManageSecurity(_userClaims);
            var personaId = _personaId;
            var output = _manangeSecurityLogic.GetPersonaRightsAndActionsByRoute(personaId, routeId);

            if (_userClaims.ImpersonatedBy != Guid.Empty && routeId.ToLower().Equals("userslist"))
            {
                ManagePersona mp = new ManagePersona();
                Persona impersonateUserPersona = mp.GetActivePersonaWithoutRights(_userClaims.ImpersonatedBy);
                output.obj.ImpersonatedRights = _manangeSecurityLogic.GetPersonaRightsAndActionsByRoute(impersonateUserPersona.PersonaId, routeId).obj;
            }

            return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        #region Swagger Example
        /// <summary>
        /// Example for list of right a user can have per page
        /// </summary>
        public class RouteSecurityExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>List of right a user can have per page</returns>
            public object GetExamples()
            {
                var example = new RouteSecurity()
                {
                    RouteId = "Userslist",
                    Rights = new[] { "Create User", "Lock/Unlock User", "View User", "Edit User" }
                };
                return example;
            }
        }

        #endregion
    }
}
