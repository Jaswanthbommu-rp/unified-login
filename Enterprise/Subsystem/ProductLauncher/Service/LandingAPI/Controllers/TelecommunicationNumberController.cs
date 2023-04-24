using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// TelecommunicationNumber Controller to hold all telecommunication number management related APIs
	/// </summary>
	public class TelecommunicationNumberController : BaseApiController
	{
		#region Private variables
		private int _contactMechanismId = 0;
		private readonly ITelecommunicationNumberRepository _telecommunicationNumberRepository;
		IRepositoryResponse repositoryResponse = new RepositoryResponse();
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public TelecommunicationNumberController() : base() { }

		/// <summary>
		/// Testing Constructor
		/// </summary>
		/// <param name="telecommunicationNumberRepository">Telecommunication Number Repository</param>
		public TelecommunicationNumberController(ITelecommunicationNumberRepository telecommunicationNumberRepository)
		{
			_telecommunicationNumberRepository = telecommunicationNumberRepository;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Link a Telecommunication Number to a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="linkTelecommunicationNumber">Person's Telecommunication Number parameter values</param>
		/// <returns>Response with Success Message</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Telecommunication Number object have invalid entries)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Newly created Contact Mechanism Id", Type = typeof(TelecommunicationNumber.TelecommunicationNumberOutputResult))]
		[SwaggerResponseExamples(typeof(TelecommunicationNumber.TelecommunicationNumberOutputResult), typeof(LinkTelecommunicationNumberOutputResultExample))]
		[HttpPost]
		[Route("persons/{realPageId}/telecommunicationnumber")]
		public HttpResponseMessage LinkTelecommunicationNumber(Guid realPageId, [FromBody] LinkTelecommunicationNumber linkTelecommunicationNumber)
		{
			realPageId = ((realPageId == Guid.Empty) || (realPageId == null)) ? _realpageUserId : realPageId;
			if ((realPageId == Guid.Empty) || (realPageId == null))
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
			}

			if (linkTelecommunicationNumber == null)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: linkTelecommunicationNumber");
			}

			IManageContactMechanism contactMechanismLogic = new ManageContactMechanism();

			//Add an Telecommunication and link it to a person
			//Create the Contact Mechanism
			repositoryResponse = contactMechanismLogic.CreateContactMechanism();
			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}
			_contactMechanismId = Convert.ToInt32(repositoryResponse.Id);

			//Associate the Contact Mechanism to a Party
			IPartyContactMechanism partyContactMechanism = new PartyContactMechanism();
			partyContactMechanism = linkTelecommunicationNumber.PartyContactMechanism;
			partyContactMechanism.ContactMechanismId = _contactMechanismId;
            repositoryResponse = contactMechanismLogic.LinkContactMechanismToParty(realPageId, partyContactMechanism);
			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}

			//Assign a usage type to the Contact Mechanism
			partyContactMechanism.PartyContactMechanismId = repositoryResponse.Id;
			repositoryResponse = contactMechanismLogic.LinkUsageTypeToPartyContactMechanism(partyContactMechanism.PartyContactMechanismId, linkTelecommunicationNumber.ContactMechanismUsageType.ContactMechanismUsageTypeId);
			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}

			//Add/Update a TelecommunicationNumber and link it to a person
			IManageTelecommunicationNumber telecommunicationNumberLogic = new ManageTelecommunicationNumber();
			ITelecommunicationNumber telecommunicationNumber = new TelecommunicationNumber();
			telecommunicationNumber = linkTelecommunicationNumber.TelecommunicationNumber;
			telecommunicationNumber.ContactMechanismId = _contactMechanismId;
			repositoryResponse = telecommunicationNumberLogic.CreateTelecommunicationNumber(telecommunicationNumber);
			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}

			TelecommunicationNumber.TelecommunicationNumberOutputResult result = new TelecommunicationNumber.TelecommunicationNumberOutputResult
			{
				ContactMechanismId = _contactMechanismId
			};

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Update a Telecommunication Number to a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="linkTelecommunicationNumber">Person's Telecommunication Number parameter values</param>
		/// <returns>Response with Success Message</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Telecommunication Number object have invalid entries)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Newly created Contact Mechanism Id", Type = typeof(TelecommunicationNumber.TelecommunicationNumberOutputResult))]
		[SwaggerResponseExamples(typeof(TelecommunicationNumber.TelecommunicationNumberOutputResult), typeof(LinkTelecommunicationNumberOutputResultExample))]
		[HttpPut]
		[Route("persons/{realPageId}/telecommunicationnumber")]
		public HttpResponseMessage UpdateTelecommunicationNumber(Guid realPageId, [FromBody] LinkTelecommunicationNumber linkTelecommunicationNumber)
		{
			realPageId = ((realPageId == Guid.Empty) || (realPageId == null)) ? _realpageUserId : realPageId;
			if ((realPageId == Guid.Empty) || (realPageId == null))
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
			}

			if (linkTelecommunicationNumber == null)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: linkTelecommunicationNumber");
			}

			IManageContactMechanism contactMechanismLogic = new ManageContactMechanism();
			if (linkTelecommunicationNumber.PartyContactMechanism != null)
			{
				_contactMechanismId = linkTelecommunicationNumber.PartyContactMechanism.ContactMechanismId;
			}

			repositoryResponse = contactMechanismLogic.UpdateContactMechanismUsageForParty(linkTelecommunicationNumber.PartyContactMechanism.PartyContactMechanismId, linkTelecommunicationNumber.ContactMechanismUsageType.ContactMechanismUsageTypeId);
			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}

			//Add/Update a TelecommunicationNumber and link it to a person
			IManageTelecommunicationNumber telecommunicationNumberLogic = new ManageTelecommunicationNumber();
			ITelecommunicationNumber telecommunicationNumber = new TelecommunicationNumber();
			telecommunicationNumber = linkTelecommunicationNumber.TelecommunicationNumber;
			telecommunicationNumber.ContactMechanismId = _contactMechanismId;
			repositoryResponse = telecommunicationNumberLogic.CreateTelecommunicationNumber(telecommunicationNumber);
			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}

			TelecommunicationNumber.TelecommunicationNumberOutputResult result = new TelecommunicationNumber.TelecommunicationNumberOutputResult
			{
				ContactMechanismId = _contactMechanismId
			};

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// List Telecommunication Number details for a Person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>A list of Telecommunication Number Details for a person</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the password policy", Type = typeof(ITelecommunicationNumber))]
		[SwaggerResponseExamples(typeof(ITelecommunicationNumber), typeof(TelecommunicationNumberExample))]
		[Route("persons/{realPageId}/telecommunicationnumber")]
		[HttpGet]
		public HttpResponseMessage ListTelecommunicationNumberForPerson(Guid realPageId, string ContactMechanismUsageTypeName = "")
		{
			realPageId = ((realPageId == Guid.Empty) || (realPageId == null)) ? _realpageUserId : realPageId;
			if ((realPageId == Guid.Empty) || (realPageId == null))
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
			}

			IList<TelecommunicationNumber> telecommunicationNumberList = new List<TelecommunicationNumber>();
			IManageTelecommunicationNumber telecommunicationNumberLogic = new ManageTelecommunicationNumber();

			if (_telecommunicationNumberRepository == null)
			{
				telecommunicationNumberList = telecommunicationNumberLogic.ListTelecommunicationNumberForPerson(realPageId, ContactMechanismUsageTypeName);
			}
			else
			{
				telecommunicationNumberList = _telecommunicationNumberRepository.ListTelecommunicationNumberForPerson(realPageId, ContactMechanismUsageTypeName);
			}

			if (telecommunicationNumberList != null)
			{
				ObjectListOutput<TelecommunicationNumber, IErrorData> output = new ObjectListOutput<TelecommunicationNumber, IErrorData>() { list = telecommunicationNumberList };
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			//When trying to get a list of telecommunication number for a Person that doesn't exists
			return Request.CreateResponse(HttpStatusCode.NoContent, "Invalid realPageId");
		}
		#endregion

		#region Get Examples
		/// <summary>
		/// Used to document examples of the TelecommunicationNumber Model webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class TelecommunicationNumberExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>TelecommunicationNumber example</returns>
			public object GetExamples()
			{
				ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType()
				{
					ContactMechanismUsageTypeId = 201,
					ParentContactMechanismUsageTypeId = 200,
					Name = "Primary"
				};

				ITelecommunicationNumber example = new TelecommunicationNumber()
				{
					ContactMechanismId = 1,
					AreaCode = "972",
					CountryCode = "1",
					PhoneNumber = "820-3000",
					contactMechanismUsageType = contactMechanismUsageType
				};

				ObjectOutput<ITelecommunicationNumber, IErrorData> output = new ObjectOutput<ITelecommunicationNumber, IErrorData>() { obj = example };

				return output;
			}
		}
		#endregion

		#region Output results for documentation
		/// <summary>
		/// Used to document examples of the New Telecommunication Number webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class LinkTelecommunicationNumberOutputResultExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>Newly created Contact Mechanism Id</returns>
			public object GetExamples()
			{
				return TelecommunicationNumber.LinkTelecommunicationNumberOutputResultExample();
			}
		}
		#endregion
	}
}