using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    public class ProductIntegrationMarketplaceController : BaseApiController
    {

        private IManageProductIntegrationMarketplace _manageProductIntegrationMarketplace;

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public ProductIntegrationMarketplaceController() : base() { }

        /// <summary>
        /// Used for unit testing
        /// </summary> 
        public ProductIntegrationMarketplaceController(IManageProductIntegrationMarketplace manageProductIntegrationMarketplace)
        {
            _manageProductIntegrationMarketplace = manageProductIntegrationMarketplace;
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
            _manageProductIntegrationMarketplace = new ManageProductIntegrationMarketplace(_userClaims);
        }

        #endregion


        /// <summary>
        /// Returns Roles by PartyID
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>  
        /// <param name="partyId">Organization PartyID</param>
        /// <param name="datafilter"></param>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List of roles by partyid", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/integrationmarketplace/roles")]
        [HttpGet]
        public HttpResponseMessage GetRoles(long editorPersonaId, long userPersonaId, long partyId, [FromUri]RequestParameter datafilter)
        {
            if (partyId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "partyId not supplied.");

            var result = _manageProductIntegrationMarketplace.GetRoles(editorPersonaId, userPersonaId, partyId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

    }
}
