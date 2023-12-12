using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.BatchProcessor;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Batch;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.EnterpriseRole;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    public class BulkController : BaseApiController
    {

        private readonly IManageBulkEnterpriseRoles _manageBulkEnterpriseRoles;


        public BulkController(IManageBulkEnterpriseRoles manageBulkEnterpriseRoles)
        {
            _manageBulkEnterpriseRoles = manageBulkEnterpriseRoles;
        }

        /// <summary>
        /// Used to assign Entrprise roles to users in bulk.
        /// </summary>
        /// <param name="editorUserPersonaId"></param>
        /// <param name="userPersonaIds"></param>
        /// <param name="roleTemplateProductRole"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("bulk/bulkassignenterpriseroles")]      
        [HttpPost]
        public HttpResponseMessage GetPersonaEnvironmentType(long editorUserPersonaId, List<long> userPersonaIds, RoleTemplateProductRoleMapping roleTemplateProductRole)
        {
            var response = _manageBulkEnterpriseRoles.UpdateEnterpriseToUsers(editorUserPersonaId, userPersonaIds, roleTemplateProductRole);
            if (!string.IsNullOrEmpty(response))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }
            return Request.CreateResponse(HttpStatusCode.OK); 
        }

    }
}