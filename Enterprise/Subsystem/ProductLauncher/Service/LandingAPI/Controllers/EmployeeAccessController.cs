using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.ResponseObject;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    public class EmployeeAccessController : BaseApiController
    {
        private IManageEmployeeAccess _manageEmployeeAccess;
        private IManagePersona _managePersona;
        private IManageProductUser _manageProductUser;
        private IManageOrganization _manageOrganization;

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
            _manageProductUser = new ManageProductUser(_userClaims);
            _managePersona = new ManagePersona(_userClaims);
            _manageOrganization = new ManageOrganization(_userClaims);
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
            
            return Request.CreateResponse(HttpStatusCode.OK, _manageEmployeeAccess.GetOrCreateEmployeePersonaId(companyRealPageId, _userClaims.LoginName));
        }

        /// <summary>
        /// Creates an employee in the given product and company if it does not exist.
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="personaId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Creates an employee in the given product and company if it does not exist.", Type = typeof(EmployeeResponse))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [Route("employeeaccess/product/{productId}/persona/{personaId}")]
        [HttpPost]
        public HttpResponseMessage CreateEmployeeProductUser(int productId, long personaId)
        {
            EmployeeResponse response = new EmployeeResponse();
            long adminUserPersonaId = 0;
            Guid adminCreatorRealPageId = Guid.Empty;

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

            var userPersona = _managePersona.GetPersona(personaId);

            if (userPersona.Organization.RealPageId != Guid.Empty)
            {
                adminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(userPersona.Organization.RealPageId);
                //recreate clams
                if (adminCreatorRealPageId == Guid.Empty)
                {
                    response.ErrorMessage = "Missing company admin user.";

                    // return errors with bad request
                    return Request.CreateResponse(HttpStatusCode.BadRequest, response);
                }

                adminUserPersonaId = _managePersona.GetFirstAvailablePersonaByCompany(adminCreatorRealPageId, userPersona.OrganizationPartyId).PersonaId;
                _manageProductUser = new ManageProductUser(_userClaims);
            }

            var rolePropertyList = new RolePropertyList();
            var productUser = new ProductUserProperitiesRoles()
            {
                RealPageId = adminCreatorRealPageId,
                ProductId = productId, 
                CreateUserPersonaId = adminUserPersonaId, 
                AssignUserPersonaId = personaId,
                CorrelationId = _userClaims.CorrelationId,
                InputJson = JsonConvert.SerializeObject(rolePropertyList)
            };
            var result = _manageProductUser.CreateProductUser(productUser);

            if (string.IsNullOrEmpty(result))
            {
                response.Status = true;
            }
            else
            {
                response.ErrorMessage = result;
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        public class EmployeeResponse
        {
            public bool Status { get; set; }
            public string ErrorMessage { get; set; }

        }
    }
}



