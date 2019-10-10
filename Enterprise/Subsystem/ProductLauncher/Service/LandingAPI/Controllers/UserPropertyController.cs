using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	public class UserPropertyController : BaseApiController
    {
        /// <summary>
        /// Get persona properties
        /// </summary>
        /// <param name="productId">ProductId</param>
        /// <returns>ProductProperty List</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Profile object have invalid entries)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get properties for product", Type = typeof(HttpResponseMessage))]
        [Route("user/properties")]
        [HttpGet]
        public HttpResponseMessage GetUserProperties(long productId)
        {
            var result = new ListResponse();
            IList<ProductProperty> propertyList = new List<ProductProperty>();

            //Guid realPageId = _realpageUserId;

            if (_realpageUserId == Guid.Empty)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter realPageId.");
            }

            if (productId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter productId.");

            IManagePersona managePersona = new ManagePersona();

            //Active Persona is linked to one organization
            //long personaId = managePersona.GetActivePersonaId(_realpageUserId);

            // Get Properties for Persona by ProductId
            switch (productId)
            {
                case (int)ProductEnum.OmniChannel:

                    IManageUserProperty manageUser = new ManageUserProperty();
                    result = manageUser.GetAssignedPropertyForPersona(_personaId, productId);
                    break;

                default:

                    result = new ListResponse()
                    {
                        IsError = false,
                        Records = propertyList.Cast<object>().ToList(),
                        TotalRows = propertyList.Count,
                        RowsPerPage = propertyList.Count,
                        TotalPages = 1,
                        ErrorReason = "No results found for the product requested."
                    };
                    break;
            }
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}