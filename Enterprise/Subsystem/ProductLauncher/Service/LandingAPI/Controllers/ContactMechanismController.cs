using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System;
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
	/// Contact Mechanism Controller to hold all contact mechanism management related APIs
	/// </summary>
	public class ContactMechanismController : BaseApiController
	{
		#region Private variables
		private readonly IContactMechanismRepository _contactMechanismRepository;
		IRepositoryResponse repositoryResponse = new RepositoryResponse();
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public ContactMechanismController() : base() { }

		/// <summary>
		/// Testing Constructor
		/// </summary>
		/// <param name="contactMechanismRepository">Contact Mechanism Repository</param>
		public ContactMechanismController(IContactMechanismRepository contactMechanismRepository) {
			_contactMechanismRepository = contactMechanismRepository;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// List Contact Mechanism details for a Person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>A list of Contact Mechanism Details for a person</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the password policy", Type = typeof(ICommonAddress))]
		[SwaggerResponseExamples(typeof(ICommonAddress), typeof(ContactMechanismExample))]
		[Route("persons/{realPageId}/contactmechanism")]
		[HttpGet]
		public HttpResponseMessage ListContactMechanismForPerson(Guid realPageId, string ContactMechanismUsageTypeName = "")
		{
			realPageId = ((realPageId == Guid.Empty) || (realPageId == null)) ? _realpageUserId : realPageId;
			if ((realPageId == Guid.Empty) || (realPageId == null))
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
			}

			IList<CommonAddress> contactMechanismList = new List<CommonAddress>();

			IManageContactMechanism contactMechanismLogic = new ManageContactMechanism();
			if (_contactMechanismRepository == null) {
				contactMechanismList = contactMechanismLogic.ListContactMechanismForPerson(realPageId, ContactMechanismUsageTypeName);
			}
			else {
				contactMechanismList = _contactMechanismRepository.ListContactMechanismForPerson(realPageId, ContactMechanismUsageTypeName);
			}

			if (contactMechanismList != null)
			{
				ObjectListOutput<CommonAddress, IErrorData> output = new ObjectListOutput<CommonAddress, IErrorData>() { list = contactMechanismList };
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			//When trying to get a list of Contact Mechanism(s) for a Person that doesn't exists
			return Request.CreateResponse(HttpStatusCode.NoContent, "Invalid realPageId");
		}
		#endregion

		#region Get Examples
		/// <summary>
		/// Used to document examples of the ContactMechanism Model webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class ContactMechanismExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>ContactMechanism example</returns>
			public object GetExamples()
			{
				ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType()
				{
					ContactMechanismUsageTypeId = 201,
					ParentContactMechanismUsageTypeId = 200,
					Name = "Primary"
				};

				ICommonAddress example = new CommonAddress()
				{
					ContactMechanismId = 1,
					AddressString = "none@nowhere.com",
					AddressType = "Email",
					contactMechanismUsageType = contactMechanismUsageType
				};

				ObjectOutput<ICommonAddress, IErrorData> output = new ObjectOutput<ICommonAddress, IErrorData>() { obj = example };

				return output;
			}
		}
		#endregion
    }
}