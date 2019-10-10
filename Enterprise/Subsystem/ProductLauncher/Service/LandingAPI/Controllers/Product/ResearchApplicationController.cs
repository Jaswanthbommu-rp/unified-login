using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using Swashbuckle.Swagger.Annotations;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers.Product
{
	/// <summary>
	/// Research Application Controller
	/// </summary>
	public class ResearchApplicationController : BaseApiController
    {
        private IManageResearchApplication _manageResearchApplication;
        
        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public ResearchApplicationController() : base() { }

        /// <summary>
        /// Used for unit testing
        /// </summary> 
        public ResearchApplicationController(IManageResearchApplication manageProductResearchApplication)
        {
            _manageResearchApplication = manageProductResearchApplication;
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
            _manageResearchApplication = new ManageResearchApplication(_userClaims);
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
        [Route("products/ResearchApplication/roles")]
        [HttpGet]
        public HttpResponseMessage GetRoles(long editorPersonaId, long userPersonaId, long partyId, [FromUri]RequestParameter datafilter)
        {
            if (partyId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "partyId not supplied.");

            var result = _manageResearchApplication.GetRoles(editorPersonaId, userPersonaId, partyId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

		/// <summary>
		/// Used to get the rights for the given party and role Id
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="partyId"></param>
		/// <param name="roleId"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List of rights by roleid", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/ResearchApplication/role/rights")]
        [HttpGet]
        public HttpResponseMessage GetRightsByRole(long editorPersonaId, long partyId, long roleId, [FromUri] RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
            if (partyId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "partyId not supplied.");
            return Request.CreateResponse(HttpStatusCode.OK, _manageResearchApplication.GetRightsByRole(editorPersonaId, partyId, roleId));
        }
    }
}
