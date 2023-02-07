using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using Swashbuckle.Swagger.Annotations;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using static RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers.Product.ProductOneSiteController;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    public class ProductProspectContactController : BaseApiController
    {
        #region Public methods
         
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
        [Route("products/prospectcontactcenter/properties")]
        [HttpGet]
        public HttpResponseMessage GetProperties(long editorPersonaId, long userPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            if (_realpageUserId == Guid.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

            IManageProductProspectContact manageProductProspectContact = new ManageProductProspectContact(_userClaims);
            var result = manageProductProspectContact.GetProperties(editorPersonaId, userPersonaId, datafilter);

            //if(result.IsError)
            //    Request.CreateResponse(HttpStatusCode.Forbidden, result);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        #endregion
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
        [Route("products/prospectcontactcenter/user/MT/status")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateProspectContactCenterUserStatus(ProductUser produtUser)
        {
            var manageProductProspectContact = new ManageProductProspectContact(_userClaims);
            if (!manageProductProspectContact.ChangeUserStatus(_personaId, produtUser.UserId))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Deactivate prospectcontactcenter user failed.");
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
        [Route("products/prospectcontactcenter/migration-users")]
        [Authorize] // Todo: Need to implement Resource Scope Based Authorization
        [HttpGet]
        public HttpResponseMessage ListProspectContactMigrationUsers(long editorPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            ManagePersona managePersona = new ManagePersona();
            var persona = managePersona.GetPersona(editorPersonaId);
            if (persona == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not found.");
            
            base._userClaims.UserRealPageGuid = persona.RealPageId;
            IManageProductProspectContact manageProductProspectContact = new ManageProductProspectContact(_userClaims);
            
            var result = manageProductProspectContact.GetMigrationUsers(editorPersonaId, datafilter);
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
        [SwaggerResponse(HttpStatusCode.OK, Description = "Mark Prospect Contact Center users to migrated", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/prospectcontactcenter/migrate-users")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateUsersMigrationStatus(IList<MigrateUser> migrateUsers)
        {
            IManageProductProspectContact manageProductProspectContact = new ManageProductProspectContact(_userClaims);
            return Request.CreateResponse(HttpStatusCode.OK, manageProductProspectContact.UpdateUsersMigrationStatus(_personaId, migrateUsers));
        }
        #endregion
    }
}


