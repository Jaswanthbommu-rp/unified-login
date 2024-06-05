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
		//[SwaggerResponse(HttpStatusCode.OK, Description = "Newly created Person Id", Type = typeof(Person.PersonOutputResult))]
		//[SwaggerResponseExamples(typeof(Person.PersonOutputResult), typeof(NewPersonOutputResultExample))]
		[HttpPost]
		[Route("sendemail")]
		public HttpResponseMessage SendEmail([FromBody] SendGridEmail sendGridEmail)
		{
			//CreatePerson
			if (sendGridEmail == null)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: sendGridEmail.");
			}

			IManageEmail manageEmail = new ManageEmail(_userClaims, FusionCache);
			string result = manageEmail.SendGridEmail(sendGridEmail);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}
		#endregion
	}
}
