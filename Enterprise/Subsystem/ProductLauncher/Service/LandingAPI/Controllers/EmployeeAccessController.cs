using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    public class EmployeeAccessController : BaseApiController
    {
        private IManageEmployeeAccess _manageEmployeeAccess;

        public EmployeeAccessController() : base() { }

        public EmployeeAccessController(IManageEmployeeAccess manageEmployeeAccess)
        {
            _manageEmployeeAccess = manageEmployeeAccess;

            base.Request = new HttpRequestMessage();
            base.Request.SetConfiguration(new HttpConfiguration());
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            _manageEmployeeAccess = new ManageEmployeeAccess(_userClaims);
        }

        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List of roles by partyid", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("employeeaccess/companies")]
        [HttpGet]
        public HttpResponseMessage GetCompanies(long editorPersonaId, string filter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            return Request.CreateResponse(HttpStatusCode.OK, _manageEmployeeAccess.GetCompanies(editorPersonaId, filter));
        }

        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List of roles by partyid", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("employeeaccess/users")]
        [HttpGet]
        public HttpResponseMessage GetUsers(long editorPersonaId, string filter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            return Request.CreateResponse(HttpStatusCode.OK, _manageEmployeeAccess.GetUsers(editorPersonaId, filter));
        }

        [Route("employeeaccess/company/{companyRealPageId}/persona")]
        [HttpGet]
        public HttpResponseMessage GetEmployeePersonaId(Guid companyRealPageId)
        {
            //return personaId and userRelapgeId (_realpageUserId)
            if (companyRealPageId == Guid.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Company ID not supplied.");
            
            return Request.CreateResponse(HttpStatusCode.OK, _manageEmployeeAccess.GetOrCreateEmployeePersonaId(companyRealPageId, _userClaims.LoginName));
        }

    }

}



