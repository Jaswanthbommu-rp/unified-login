using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// Email controller
	/// </summary>
	public class EmailController : BaseApiController
	{
		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public EmailController() : base() { }
		#endregion

		#region Public Methods
		/// <summary>
		/// Send email
		/// </summary>
		/// <param name="sendGridEmail">SendGridEmail object of the parameter values</param>
		/// <returns>Response with Success Message</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when SendGridEmail object have invalid entries)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[HttpPost]
		[Route("sendemail")]
		public HttpResponseMessage SendEmail([FromBody] SendGridEmail sendGridEmail)
		{
			if (sendGridEmail == null)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: sendGridEmail.");
			}

			IManageEmail manageEmail = new ManageEmail(_userClaims);
			string result = manageEmail.SendGridEmail(sendGridEmail);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Send MFA one-time authentication code email using the branded DB template.
		/// Replaces the old raw sendemail call that used a hardcoded message body.
		/// </summary>
		/// <param name="mfaEmailRequest">MFA email request containing firstName, emailAddress and otpCode</param>
		/// <returns>Response with status message</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "MFA email sent")]
		[HttpPost]
		[Route("sendmfaemail")]
		public HttpResponseMessage SendMFAEmail([FromBody] MFAEmailRequest mfaEmailRequest)
		{
			if (mfaEmailRequest == null)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: mfaEmailRequest.");
			}

			IManageEmail manageEmail = new ManageEmail(_userClaims);
			string result = manageEmail.SendMFAEmail(mfaEmailRequest.FirstName, mfaEmailRequest.EmailAddress, mfaEmailRequest.OtpCode);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}
		#endregion
	}
}
