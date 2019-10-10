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
	/// <summary>
	/// OmniChannelController
	/// </summary>
	public class ProductOmniChannelController : BaseApiController
    {      
       
        private IManageProductOmniChannel _manageProductOmniChannel;

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public ProductOmniChannelController() : base() { }

        /// <summary>
        /// Used for unit testing
        /// </summary> 
        public ProductOmniChannelController(IManageProductOmniChannel manageProductOmniChannel)
        {
            _manageProductOmniChannel = manageProductOmniChannel;
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
            _manageProductOmniChannel = new  ManageProductOmniChannel(_userClaims);
        }

        #endregion

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
        [Route("products/omnichannel/user/properties")]
        [HttpGet]
        public HttpResponseMessage GetProperties(long editorPersonaId, long userPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            if (_realpageUserId == Guid.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

            var result = _manageProductOmniChannel.GetProperties(editorPersonaId, userPersonaId, datafilter);
           
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

                

       
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
        [Route("products/omnichannel/roles")]
        [HttpGet]
        public HttpResponseMessage GetRoles(long editorPersonaId, long userPersonaId, long partyId, [FromUri]RequestParameter datafilter)
        {
            if (partyId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "partyId not supplied.");           

            var result = _manageProductOmniChannel.GetRoles(editorPersonaId, userPersonaId, partyId);            

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

       

    }
}
