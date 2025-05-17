using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

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

        /// <summary>
        /// Gets comapny persona Id, if exists else creates user in company and gets, and user realpage guid for employee as a user.
        /// </summary>
        /// <param name="companyRealPageId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Gets company persona Id, if exists else creates user in company and gets, and user realpage guid for employee as a user.", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [Route("employeeaccess/company/{companyRealPageId}/persona")]
        [HttpGet]
        public HttpResponseMessage GetEmployeePersonaId(Guid companyRealPageId)
        {
            if (companyRealPageId == Guid.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Company ID not supplied.");
            
            return Request.CreateResponse(HttpStatusCode.OK, _manageEmployeeAccess.GetOrCreateEmployeePersonaId(companyRealPageId, _userClaims));
        }

        /// <summary>
        /// Creates an employee in the given product and company if it does not exist.
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="personaId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Creates an employee in the given product and company if it does not exist.", Type = typeof(EmployeeAccessResponse))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [Route("employeeaccess/product/{productId}/persona/{personaId}")]
        [HttpPost]
        public HttpResponseMessage CreateEmployeeProductUser(int productId, long personaId)
        {
            var response = new EmployeeAccessResponse();

            if (productId == 0)
            {
                response.ErrorMessage = "Product ID not supplied.";
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }

            if (personaId == 0)
            {
                response.ErrorMessage = "Persona ID not supplied.";
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Persona ID not supplied.");
            }

            var result = _manageEmployeeAccess.CreateEmployeeProductUser(productId, personaId);
            if (result != null && result.Equals("DeletedProductLogin", StringComparison.OrdinalIgnoreCase))
            {
                // the product login was disabled, so try again and see if any other groups are assignable
                result = _manageEmployeeAccess.CreateEmployeeProductUser(productId, personaId);
            }

            if (string.IsNullOrEmpty(result))
            {
                response.Status = true;
            }
            else
            {
                response.ErrorMessage = result;
            }

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        public class EmployeeAccessResponse
        {
            public bool Status { get; set; }
            public string ErrorMessage { get; set; } = "";

        }
    }
}



