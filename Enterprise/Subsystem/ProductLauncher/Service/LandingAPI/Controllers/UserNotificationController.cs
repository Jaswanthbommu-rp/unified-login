using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UserLogin = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.UserLogin;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    /// <summary>
    /// User Notificatio nController to hold all user notification related APIs
    /// </summary>
    public class UserNotificationController : BaseApiController
	{
		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public UserNotificationController() : base() { }
		#endregion


		/// <summary>
		/// Send Welcome Email Invitations
		/// </summary>        
		/// <param name="userLogins">Array of user realpage Ids</param>
		/// <returns>List of patched UserLogins</returns>		
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[Route("usernotification/sendwelcomeemail")]
		[System.Web.Http.AllowAnonymous]
		[HttpPost]
		public HttpResponseMessage SendWelcomeEmail([FromBody]IList<UserLogin> userLogins)
		{
			ObjectListOutput<UserLogin, IErrorData> output = new ObjectListOutput<UserLogin, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			bool response = true;
			IManageUserLogin manageUserLogin = new ManageUserLogin();

			if (userLogins.Count > 0)
			{
				ManageUserLogin userLoginLogic = new ManageUserLogin(_userClaims);
				response = userLoginLogic.ResendInvitation(userLogins,true);

				if (response)
				{
					return Request.CreateResponse(HttpStatusCode.OK, response);
				}

				return Request.CreateResponse(HttpStatusCode.ExpectationFailed, response);


			}
			return Request.CreateResponse(HttpStatusCode.OK, response);
		}
	}
}
