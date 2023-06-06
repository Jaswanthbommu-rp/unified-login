using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.MarketingCenter;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// Used to manage Marketing Center users
	/// </summary>
	public class ProductMarketingCenterController : BaseApiController
    {
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
        [Route("products/marketingcenter/roles")]
        [Authorize]
        [HttpGet]
        public ListResponse GetMarketingCenterRoles(long editorPersonaId, long userPersonaId, [FromUri]RequestParameter datafilter)
        {
            ManageProductMarketingCenter mg = new ManageProductMarketingCenter(base._userClaims);
            ListResponse response = mg.GetRoles(editorPersonaId, userPersonaId, datafilter);
            return response;
        }


        /// <summary>
        /// Used to get a list of roles 
        /// </summary>
        /// <remarks>A datafilter can be used to filter the roles using name</remarks>
        /// <param name="editorPersonaId"></param>                
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [Route("products/marketingcenter/rolescount")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetRolesCount(long editorPersonaId)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
            ManageProductMarketingCenter manageProductMarketingCenter = new ManageProductMarketingCenter(base._userClaims);
            ListResponse response = manageProductMarketingCenter.GetRolesCount(editorPersonaId);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Used to get a list of rights 
        /// </summary>
        /// <remarks></remarks>
        /// <param name="editorPersonaId"></param>       
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of rights for the given company", Type = typeof(object))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [Route("products/marketingcenter/rights")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetRights(long editorPersonaId)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
            ManageProductMarketingCenter manageProductMarketingCenter = new ManageProductMarketingCenter(base._userClaims);
            ListResponse response = manageProductMarketingCenter.GetRights(editorPersonaId);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Used to get a list of properties
        /// </summary>
        /// <remarks>For now filtering and sorting will be done on the UI side</remarks>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter">A datafilter used to filter the properties. RightDescription or CenterName can be used</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of properties for the given company", Type = typeof(ProductProperty))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(ProductProperty), typeof(ProductProperty.PropertyExample))]
        [Route("products/marketingcenter/properties")]
        [Authorize]
        [HttpGet]
        public ListResponse GetMarketingCenterProperties(long editorPersonaId, long userPersonaId, [FromUri]RequestParameter datafilter)
        {
            ManageProductMarketingCenter mg = new ManageProductMarketingCenter(base._userClaims);
            ListResponse response = mg.GetProperties(editorPersonaId, userPersonaId, datafilter);
            return response;
        }

        /// <summary>
        /// Used to create a new account for the given GreenBook user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId">The persona to use to find the login to update</param>
        /// <param name="rolepropList">Used to update the list of roles and properties given to a user</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.Created, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request (when role or property data has invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/marketingcenter/user")]
        [Authorize]
        [HttpPost]
        public HttpResponseMessage CreateMarketingCenterUser(long editorPersonaId, long userPersonaId, MarketingCenterRoleAndPropertyList rolepropList)
        {
            if (editorPersonaId == 0 || editorPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            if (rolepropList == null)
            {
                rolepropList = new MarketingCenterRoleAndPropertyList();
            }
            ManageProductMarketingCenter mg = new ManageProductMarketingCenter(base._userClaims);

            string result = mg.ManageMarketingCenterUser(editorPersonaId, userPersonaId, rolepropList.RoleList, rolepropList.PropertyList, rolepropList.IsAssignedNewPropertyByDefault);
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.Created);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, result);
        }

        /// <summary>
        /// Used to update an existing account for the given GreenBook user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId">The persona to use to find the login to update</param>
        /// <param name="rolepropList">Used to update the list of roles and properties given to a user</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.Created, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request (when role or property data has invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/marketingcenter/user")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateMarketingCenterUser(long editorPersonaId, long userPersonaId, MarketingCenterRoleAndPropertyList rolepropList)
        {
            if (editorPersonaId == 0 || editorPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            if (rolepropList == null)
            {
                rolepropList = new MarketingCenterRoleAndPropertyList();
            }
            ManageProductMarketingCenter mg = new ManageProductMarketingCenter(base._userClaims);

            string result = mg.ManageMarketingCenterUser(editorPersonaId, userPersonaId, rolepropList.RoleList, rolepropList.PropertyList, rolepropList.IsAssignedNewPropertyByDefault);
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, result);
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
        [Route("products/marketingcenter/user/MT/status")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateMarketingCenterUserStatus(ProductUser produtUser)
        {
            var manageProductMarketingCenter = new ManageProductMarketingCenter(base._userClaims);
            if (!manageProductMarketingCenter.ChangeUserStatus(_personaId, produtUser.UserName, produtUser.UserId.ToString()))
            {
                if(produtUser.IsAssigned)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Activate MarketingCenter user failed.");
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Deactivate MarketingCenter user failed.");
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Successfully disabled product user.");
        }

        /// <summary>
        /// Disable the resident portal user.
        /// </summary>
        /// <param name="editorPersonaId">The editorPersonaId.</param>
        /// <param name="roleId">The roleId.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Role Deleted Successfully", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad Request")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/marketingcenter/role")]
        [Authorize]
        [HttpDelete]
        public ListResponse DeleteMarketingCenterRole(long editorPersonaId, int roleId)
        {
            if (editorPersonaId == 0 || editorPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            var manageProductMarketingCenter = new ManageProductMarketingCenter(base._userClaims);
            ListResponse response = manageProductMarketingCenter.DeleteRole(editorPersonaId, roleId);
            return response;
        }

        /// <summary>
        /// Disable the resident portal user.
        /// </summary>
        /// <param name="roleId">The roleId.</param>
        /// <param name="isActive">The isActive.</param>
        /// <param name="editorPersonaId">The editorPersonaId.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Role Deleted Successfully", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad Request")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/marketingcenter/role/status")]
        [Authorize]
        [HttpPost]
        public ListResponse UpdateMarketingCenterRoleStatus(long editorPersonaId, int roleId, bool isActive)
        {
            if (editorPersonaId == 0 || editorPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            var manageProductMarketingCenter = new ManageProductMarketingCenter(base._userClaims);
            ListResponse response = manageProductMarketingCenter.UpdateRoleStatus(editorPersonaId, roleId, isActive);
            return response;
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
        [Route("products/marketingcenter/migration-users")]
        [Authorize] // Todo: Need to implement Resource Scope Based Authorization
        [HttpGet]
        public HttpResponseMessage ListMarketingCenterMigrationUsers(long editorPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            ManagePersona managePersona = new ManagePersona();
            var persona = managePersona.GetPersona(editorPersonaId);
            if (persona == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not found.");

	        base._userClaims.UserRealPageGuid = persona.RealPageId;
            var manageProductMarketingCenter = new ManageProductMarketingCenter(base._userClaims);

            var result = manageProductMarketingCenter.GetMigrationUsers(editorPersonaId, datafilter);
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
        [SwaggerResponse(HttpStatusCode.OK, Description = "Mark Marketing Center users to migrated", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/marketingcenter/migrate-users")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateUsersMigrationStatus(IList<MigrateUser> migrateUsers)
        {
			var manageProductMarketingCenter = new ManageProductMarketingCenter(base._userClaims);
            return Request.CreateResponse(HttpStatusCode.OK, manageProductMarketingCenter.UpdateUsersMigrationStatus(_personaId, migrateUsers));
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
