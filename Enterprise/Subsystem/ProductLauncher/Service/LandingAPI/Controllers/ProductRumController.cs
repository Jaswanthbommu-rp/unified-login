using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	public class ProductRumController : BaseApiController
    {
        /// <summary>
        /// Returns Roles (User Access Groups in On Site)
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <param name="datafilter">A datafilter used to filter the roles.</param>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/rum/roles")]
        [HttpGet]
        public HttpResponseMessage GetRoles(long editorPersonaId, long userPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            if (_realpageUserId == Guid.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

            var manageProductRum = new ManageProductRum(base._userClaims);
            var result = manageProductRum.GetRoles(editorPersonaId, userPersonaId, datafilter);

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
        [Route("products/rum/properties")]
        [HttpGet]
        public HttpResponseMessage GetProperties(long editorPersonaId, long userPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            if (_realpageUserId == Guid.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");


            var manageProductRum = new ManageProductRum(base._userClaims);
            var result = manageProductRum.GetProperties(editorPersonaId, userPersonaId, datafilter);

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
        [Route("products/rum/regions")]
        [HttpGet]
        [Obsolete]
        public HttpResponseMessage GetRegionss(long editorPersonaId, long userPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            if (_realpageUserId == Guid.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");


            var manageProductRum = new ManageProductRum(base._userClaims);
            var result = manageProductRum.GetRegions(editorPersonaId, userPersonaId, datafilter);

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
        [Route("products/rum/propertygroups")]
        [HttpGet]
        public HttpResponseMessage GetPropertyGroups(long editorPersonaId, long userPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            if (_realpageUserId == Guid.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");


            var manageProductRum = new ManageProductRum(base._userClaims);
            var result = manageProductRum.GetPropertyGroups(editorPersonaId, userPersonaId, datafilter);

            //if(result.IsError)
            //    Request.CreateResponse(HttpStatusCode.Forbidden, result);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        #region Migration API

        /// <summary>
        /// Returns product users of an organization for given user.
        /// </summary>
        /// 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List RUM users", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/rum/migration-users")]
        [Authorize] // Todo: Need to implement Resource Scope Based Authorization
        [HttpGet]
        public HttpResponseMessage ListRUMMigrationUsers(long editorPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            ManagePersona managePersona = new ManagePersona();
            var persona = managePersona.GetPersona(editorPersonaId);
            if (persona == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not found.");

	        base._userClaims.UserRealPageGuid = persona.RealPageId;
            var manageProductRum = new ManageProductRum(base._userClaims);

            var result = manageProductRum.GetMigrationUsers(editorPersonaId, datafilter);
            if (!result.IsError)
                return Request.CreateResponse(HttpStatusCode.OK, result);
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden, result);
        }

        /// <summary>
        /// Returns product users of an organization for given user.
        /// </summary>
        /// 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Mark RUM user to migrated", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/rum/migrate-users")]
        [Authorize] 
        [HttpPut]
        public HttpResponseMessage UpdateUsersMigrationStatus(List<MigrateUser> migratedUsers)
        {
            var manageProductRum = new ManageProductRum(base._userClaims);
            return Request.CreateResponse(HttpStatusCode.OK, manageProductRum.UpdateUsersMigrationStatus(_personaId, migratedUsers));
        }

        #region User-Status

        /// <summary>
        /// Disables the Utility Management user.
        /// </summary>
        /// <param name="productUser">The product user.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "User Disabled Successfully", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad Request")]
        [Route("products/rum/user/MT/status")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateRUMUserStatus(ProductUser productUser)
        {
            var manageProductRum = new ManageProductRum(base._userClaims);
            if (!manageProductRum.ChangeUserStatus(_personaId, productUser.UserId.ToString()))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Disabling Utility Management user failed.");
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Successfully disabled product user.");
        }

        #endregion
        #endregion
    }
}
