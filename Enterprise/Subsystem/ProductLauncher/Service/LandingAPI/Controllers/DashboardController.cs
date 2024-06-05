using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// APIs for Landing App Dashboard
	/// </summary>
	public class DashboardController : BaseApiController
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DashboardController() : base() { }

        #region Public Methods        

        /// <summary>
		/// Get dashboard content for the logged in user with its active persona
		/// </summary>
		/// <remarks>Get dashboard content for the logged in persona</remarks>
		/// <returns>Information about the user persona's profile and assigned products</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [Route("dashboard")]
        [HttpGet]        
        public DashboardElementResponse GetDashboardContent()
        {
            IManagePersona managePersona = new ManagePersona(_userClaims);             
            //long? personaId = managePersona.GetActivePersonaId(_realpageUserId);
                
            //if (personaId == null || personaId == 0)
            //{                
            //    //throw new HttpResponseException(HttpStatusCode.BadRequest);
            //    return new DashboardElementResponse {
            //            DashboardElements = new DashboardElements(),
            //            ErrorReason = " - Setup Error: No persona is associated for the user!",
            //            IsError = true
            //        };
            //}           
            
            Persona persona = managePersona.GetPersona(_userClaims.PersonaId);

            if (persona == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

			IManageDashboardContent manageDashboard = new ManageDashboardContent(_userClaims, FusionCache);
			DashboardElementResponse dashboardElementResponse = manageDashboard.GetDashboardElementResponse(_realpageUserId, persona);

			IManageCredential manageCredential = new ManageCredential(_userClaims);
			CheckPasswordExpirationResponse checkPasswordExpirationResponse = manageCredential.CheckPasswordExpiration(_userClaims.UserId, _userClaims.UserRealPageGuid);
			if ((checkPasswordExpirationResponse != null) && (checkPasswordExpirationResponse.IsPasswordExpired))
			{
				dashboardElementResponse.DashboardElements.Resources = null;
				dashboardElementResponse.DashboardElements.ProfileDetail.AssignedProducts = null;
			}

			return dashboardElementResponse;

		}

		#endregion
	}
}
