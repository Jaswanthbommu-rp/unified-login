using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	public class ProductLead2LeaseController : BaseApiController
    {
        IManageProductLead2Lease _manageProductLead2Lease;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ProductLead2LeaseController() { }

        /// <summary>
        /// Used for unit testing
        /// </summary>
        /// <param name="manageProductLead2Lease"></param>
        public ProductLead2LeaseController(IManageProductLead2Lease manageProductLead2Lease)
        {
            _manageProductLead2Lease = manageProductLead2Lease;
            base.Request = new HttpRequestMessage();
            base.Request.SetConfiguration(new HttpConfiguration());
        }

        /// <summary>
        /// Used to initialize the base class before the controller to get the users persona
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            //When API is NOT called from test class
            _manageProductLead2Lease = new ManageProductLead2Lease(base._userClaims);
        }

        /// <summary>
        /// Used to get a list of roles
        /// </summary>
        /// <remarks>For now filtering and sorting will be done on the UI side</remarks>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter">A datafilter used to filter the roles.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles for the given company", Type = typeof(ProductRole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(ProductRole), typeof(ProductRole.RoleExample))]
        [Route("products/lead2lease/roles")]
        [Authorize]
        [HttpGet]
        public ListResponse GetLead2LeaseRoles(long editorPersonaId, long userPersonaId, [FromUri]RequestParameter datafilter)
        {
            ListResponse response = _manageProductLead2Lease.GetRoles(editorPersonaId, userPersonaId, datafilter);
            return response;
        }

        /// <summary>
        /// Used to get a list of properties
        /// </summary>
        /// <remarks>For now filtering and sorting will be done on the UI side</remarks>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of properties for the given company", Type = typeof(ProductProperty))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(ProductProperty), typeof(ProductProperty.PropertyExample))]
        [Route("products/lead2lease/properties")]
        [Authorize]
        [HttpGet]
        public ListResponse GetLead2LeaseProperties(long editorPersonaId, long userPersonaId, [FromUri]RequestParameter datafilter)
        {
            ListResponse response = _manageProductLead2Lease.GetProperties(editorPersonaId, userPersonaId, datafilter);
            return response;
        }

        #region User-Status

        /// <summary>
        /// Disable the resident portal user.
        /// </summary>
        /// <param name="produtUser">The produt user.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "User Deleted Successfully", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad Request")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/lead2lease/user/MT/status")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateLead2LeaseUserStatus(ProductUser produtUser)
        {
            if (!_manageProductLead2Lease.ChangeUserStatus(_personaId, produtUser.UserName, produtUser.UserId.ToString()))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Deactivate Lead2Lease user failed.");
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Successfully disabled product user.");
        }

        #endregion


        #region Migration API
        /// <summary>
        /// Returns product users of an organization for given user.
        /// </summary>
        /// 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List Marketing Center users", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/lead2lease/migration-users")]
        [Authorize] // Todo: Need to implement Resource Scope Based Authorization
        [HttpGet]
        public HttpResponseMessage ListLead2LeaseMigrationUsers(long editorPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            ManagePersona managePersona = new ManagePersona();
            var persona = managePersona.GetPersona(editorPersonaId);
            if (persona == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not found.");

	        base._userClaims.UserRealPageGuid = persona.RealPageId;
            var manageProductLead2Lease = new ManageProductLead2Lease(base._userClaims);

            var result = manageProductLead2Lease.GetMigrationUsers(editorPersonaId, datafilter);
            if (!result.IsError)
                return Request.CreateResponse(HttpStatusCode.OK, result);
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden, result);
        }

        /// <summary>
        /// Update migration status of users.
        /// </summary>
        /// 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Mark Lead2Lease users to migrated", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/lead2lease/migrate-users")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateUsersMigrationStatus(IList<MigrateUser> migrateUsers)
        {
            return Request.CreateResponse(HttpStatusCode.OK, _manageProductLead2Lease.UpdateUsersMigrationStatus(_personaId, migrateUsers));
        }
        #endregion

        #region Get Examples

        /// <summary>
        /// Used to document examples of the Response webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class ResponseExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Response example</returns>
            public object GetExamples()
            {
                HttpResponseMessage example = new HttpResponseMessage(HttpStatusCode.OK);

                return example;
            }
        }
        #endregion
    }
}
