using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// Password Policy Controller
	/// </summary>
	public class PasswordPolicyController : BaseApiController
	{
        #region Private variables
        IRepositoryResponse repositoryResponse = new RepositoryResponse();
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public PasswordPolicyController() : base() { }
		#endregion

		#region Public Methods
		/// <summary>
		/// Create a Password Policy
		/// </summary>
		/// <param name="passwordPolicy">Password Policy object of the parameter values</param>
		/// <returns></returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when User object have invalid entries / when Information is out of sync with the server)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Newly created Password Policy Id", Type = typeof(PasswordPolicy.PasswordPolicyOutputResult))]
		[SwaggerResponseExamples(typeof(PasswordPolicy.PasswordPolicyOutputResult), typeof(PasswordPolicyExample))]
		[Route("passwordpolicies")]
		[HttpPost]
		public HttpResponseMessage CreatePasswordPolicy([FromBody] PasswordPolicy passwordPolicy)
		{
			if (passwordPolicy == null)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: Password Policy.");
			}

			//Use the currentClaimPrincipal to get the UserId to be stored
			passwordPolicy.UserId = _EnterpriseUserId;

			IManagePasswordPolicy passwordPolicyLogic = new ManagePasswordPolicy();
			repositoryResponse = passwordPolicyLogic.CreatePasswordPolicy(passwordPolicy);

			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}

			PasswordPolicy.PasswordPolicyOutputResult result = new PasswordPolicy.PasswordPolicyOutputResult
			{
				NewPasswordPolicyId = repositoryResponse.Id
			};

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Get/List Password Polic(y|ies) Details
		/// </summary>
		/// <param name="PartyId">Party ID (Organization ID)</param>
		/// <returns>A list of Password Polic(y|ies) Details</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when PasswordPolicy object have invalid entries / when Information is out of sync with the server)")]
		//[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the password policy", Type = typeof(IPasswordPolicy))]
        [SwaggerResponseExamples(typeof(IPasswordPolicy), typeof(PasswordPolicyExample))]
        [Route("passwordpolicies/{PartyId}")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetPasswordPolicy(long PartyId)
        {
			IPasswordPolicy passwordPolicy = new PasswordPolicy();
			ObjectOutput<IPasswordPolicy, IErrorData> output = new ObjectOutput<IPasswordPolicy, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			output.obj = passwordPolicy;

			if (PartyId <= 0)
            {
				errorStatus.Success = false;
				errorStatus.ErrorCode = "PasswordPolicy.GetPasswordPolicy.1";
				errorStatus.ErrorMsg = "Invalid parameter: Company PartyId";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
            }

			IManagePasswordPolicy passwordPolicyLogic = new ManagePasswordPolicy();
            passwordPolicy = passwordPolicyLogic.GetPasswordPolicy(PartyId);

            if (passwordPolicy != null)
            {
				output.obj = passwordPolicy;
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			//When trying to get a Password Policy that doesn't exists / deleted
			errorStatus.Success = false;
			errorStatus.ErrorCode = "PasswordPolicy.GetPasswordPolicy.2";
			errorStatus.ErrorMsg = "Get PasswordPolicy details: No data.";
			output.Status = errorStatus;
			return Request.CreateResponse(HttpStatusCode.OK, output);
		}

		/// <summary>
		/// Update Password Policy
		/// </summary>
		/// <param name="passwordPolicy">Password Policy object of the parameter values</param>
		/// <returns></returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when PasswordPolicy object have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Password Policy Updated")]
        [Route("passwordpolicies")]
        [HttpPut]
        public HttpResponseMessage UpdatePasswordPolicy([FromBody] PasswordPolicy passwordPolicy)
        {
			if (passwordPolicy == null)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: Password Policy.");
			}

			//Use the currentClaimPrincipal to get the UserId to be stored
			passwordPolicy.UserId = _EnterpriseUserId;

			IManagePasswordPolicy passwordPolicyLogic = new ManagePasswordPolicy();
            repositoryResponse = passwordPolicyLogic.UpdatePasswordPolicy(passwordPolicy);

            if (repositoryResponse.Id == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
            }

            return Request.CreateResponse(HttpStatusCode.OK, passwordPolicy);
        }
        #endregion

        #region Get Examples
        /// <summary>
        /// Used to document examples of the User List Model webapi result
        /// </summary>
        public class PasswordPolicyExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>User List example</returns>
            public object GetExamples()
            {
                IPasswordPolicy example = new PasswordPolicy()
                {
                    PasswordPolicyId = 1,
                    PartyId = 1,
                    Name = "RealPage",
                    MinimumLength = 8,
                    MaximumLength = 128,
                    MinimumLowercase = 0,
                    MinimumUppercase = 0,
                    MinimumNumeric = 0,
                    MinimumSpecialCharacter = 0,
                    AllowUsersToChangeOwnPassword = true,
                    EnablePasswordExpiration = false,
                    PasswordExpirationPeriodInDays = 0,
                    PreventPasswordReuse = false,
                    NumberOfPasswordsToRemember = 0
                };

				Status<IErrorData> errorStatus = new Status<IErrorData>();
				ObjectOutput<IPasswordPolicy, IErrorData> output = new ObjectOutput<IPasswordPolicy, IErrorData>() { obj = example };
				output.Status = errorStatus;

				return output;
			}
		}
		#endregion

		#region Output results for documentation
		/// <summary>
		/// Used to document examples of the New User webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class NewPasswordPolicyOutputResultExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Newly created Password Policy id</returns>
            public object GetExamples()
            {
                return PasswordPolicy.GetNewPasswordPolicyExample();
            }
        }

		/// <summary>
		/// Used to document examples of the New User webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class NewSecuritySetingsOutputResultExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>Newly created Password Policy</returns>
			public object GetExamples()
			{
				return PasswordPolicy.GetNewSecuritySettingsExample();
			}
		}
	  	#endregion
	}
}