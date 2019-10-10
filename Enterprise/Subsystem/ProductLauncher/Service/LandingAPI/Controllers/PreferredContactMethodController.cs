using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// Preferred Contact Method Controller to hold all contact mechanism management related APIs
	/// </summary>
	public class PreferredContactMethodController : BaseApiController
	{
		#region Private variables
		private readonly IPreferredContactMethodRepository _preferredContactMethodRepository;
		IRepositoryResponse repositoryResponse = new RepositoryResponse();
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public PreferredContactMethodController() : base() { }

		/// <summary>
		/// Testing Constructor
		/// </summary>
		/// <param name="preferredContactMethodRepository">Contact Mechanism Repository</param>
		public PreferredContactMethodController(IPreferredContactMethodRepository preferredContactMethodRepository)
		{
			_preferredContactMethodRepository = preferredContactMethodRepository;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// List Preferred Contact Methods details
		/// </summary>
		/// <returns>A list of Preferred Contact Methods details</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the password policy", Type = typeof(IPreferredContactMethod))]
		[SwaggerResponseExamples(typeof(IPreferredContactMethod), typeof(PreferredContactMethodExample))]
		[Route("preferredcontactmethods")]
		[HttpGet]
		public HttpResponseMessage ListPreferredContactMethod()
		{
			IList<PreferredContactMethod> preferredContactMethodList = new List<PreferredContactMethod>();

			IManagePreferredContactMethod preferredContactMethodLogic = new ManagePreferredContactMethod();

			if (_preferredContactMethodRepository == null)
			{
				preferredContactMethodList = preferredContactMethodLogic.ListPreferredContactMethod();
			}
			else
			{
				preferredContactMethodList = _preferredContactMethodRepository.ListPreferredContactMethod();
			}

			if (preferredContactMethodList != null)
			{
				ObjectListOutput<PreferredContactMethod, IErrorData> output = new ObjectListOutput<PreferredContactMethod, IErrorData>() { list = preferredContactMethodList };
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			//When trying to get a list of Contact Mechanism UsageTypes that doesn't exists
			return Request.CreateResponse(HttpStatusCode.NoContent, "No Data");
		}
		#endregion

		#region Get Examples
		/// <summary>
		/// Used to document examples of the PreferredContactMethod Model webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class PreferredContactMethodExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>PreferredContactMethod example</returns>
			public object GetExamples()
			{
				IList<PreferredContactMethod> preferredContactMethodList = new List<PreferredContactMethod>();
				PreferredContactMethod preferredContactMethodUsageType = new PreferredContactMethod()
				{
					PreferredContactMethodId = 1,
					Name = "Email"
				};

				preferredContactMethodList.Add(preferredContactMethodUsageType);

				preferredContactMethodUsageType = new PreferredContactMethod()
				{
					PreferredContactMethodId = 2,
					Name = "Phone"
				};

				preferredContactMethodList.Add(preferredContactMethodUsageType);

				ObjectListOutput<PreferredContactMethod, IErrorData> output = new ObjectListOutput<PreferredContactMethod, IErrorData>() { list = preferredContactMethodList };

				return output;
			}
		}
		#endregion
    }
}