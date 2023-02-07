using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using Swashbuckle.Swagger.Annotations;
using System;
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
    /// <summary>
    /// Vendor Credentialing Controller
    /// </summary>
    public class ProductVendorServicesController : BaseApiController
    {
        private IManageProductVendorServices _manageProductVendorServices;

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public ProductVendorServicesController() : base() { }

        /// <summary>
        /// Used for unit testing
        /// </summary> 
        public ProductVendorServicesController(IManageProductVendorServices manageProductVendorServices)
        {
            _manageProductVendorServices = manageProductVendorServices;
            base.Request = new HttpRequestMessage();
            base.Request.SetConfiguration(new HttpConfiguration());
        }

        /// <summary>
        /// Used to initialize the base class  
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            _manageProductVendorServices = new ManageProductVendorServices(base._userClaims);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Returns Divisions, Regions and ownership Groups  
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId"></param> 
        /// <param name="datafilter">A datafilter used to filter the roles.</param>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/vendorcompliance/propertygroups")]
        [HttpGet]
        public HttpResponseMessage GetPropertyGroups(long editorPersonaId, long userPersonaId, [FromUri] RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            if (_realpageUserId == Guid.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

            var result = _manageProductVendorServices.GetPropertyGroups(editorPersonaId, userPersonaId, datafilter);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Returns Roles (User Access Groups in Vendor Credentialing)
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <param name="accessType">Acess type - Property or Client</param>
        /// <param name="datafilter">A datafilter used to filter the roles.</param>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/vendorcompliance/roles")]
        [HttpGet]
        public HttpResponseMessage GetRoles(long editorPersonaId, long userPersonaId, AccessType accessType, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            if (_realpageUserId == Guid.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");


            var result = _manageProductVendorServices.GetRoles(editorPersonaId, userPersonaId, accessType, datafilter);

            //if(result.IsError)
            //    Request.CreateResponse(HttpStatusCode.Forbidden, result);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Returns Properties  
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param> 
        /// <param name="datafilter">A datafilter used to filter the properties.</param>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/vendorcompliance/properties")]
        [HttpGet]
        public HttpResponseMessage GetProperties(long editorPersonaId, long userPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            if (_realpageUserId == Guid.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

            var result = _manageProductVendorServices.GetProperties(editorPersonaId, userPersonaId, datafilter);

            //if(result.IsError)
            //    Request.CreateResponse(HttpStatusCode.Forbidden, result);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Get Notification Settings 
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param> 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/vendorcompliance/notifications")]
        [HttpGet]
        public HttpResponseMessage GetNotification(long editorPersonaId, long userPersonaId)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            if (_realpageUserId == Guid.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

            var result = _manageProductVendorServices.GetNotificationSettings(editorPersonaId, userPersonaId);

            //if(result.IsError)
            //    Request.CreateResponse(HttpStatusCode.Forbidden, result);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        #endregion

        #region User-Status

        /// <summary>
        /// Disable vendor compliance user
        /// </summary>
        /// <param name="productUser"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "User Deleted Successfully", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad Request")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/vendorcompliance/user/MT/status")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateVendorComplianceUserStatus(ProductUser productUser)
        {
            if (!_manageProductVendorServices.ChangeUserStatus(_personaId, productUser.UserName, productUser.UserId.ToString()))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Deactivate VendorCompliance user failed.");
                //return Request.CreateResponse(HttpStatusCode.BadRequest, "Activate VendorCompliance user failed.");
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
        [Route("products/vendorcompliance/migration-users")]
        [Authorize] // Todo: Need to implement Resource Scope Based Authorization
        [HttpGet]
        public HttpResponseMessage ListVendorServiceMigrationUsers(long editorPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            ManagePersona managePersona = new ManagePersona();
            var persona = managePersona.GetPersona(editorPersonaId);
            if (persona == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not found.");

	        base._userClaims.UserRealPageGuid = persona.RealPageId;
            var manageProductVendorServices = new ManageProductVendorServices(base._userClaims);

            var result = manageProductVendorServices.GetMigrationUsers(editorPersonaId, datafilter);
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
        [SwaggerResponse(HttpStatusCode.OK, Description = "Mark Vendor Compliance users to migrated", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/vendorcompliance/migrate-users")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateUsersMigrationStatus(IList<MigrateUser> migrateUsers)
        {
            return Request.CreateResponse(HttpStatusCode.OK, _manageProductVendorServices.UpdateUsersMigrationStatus(_personaId, migrateUsers));
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