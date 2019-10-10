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
	/// Electronic Address Controller to hold all electronic address management related APIs
	/// </summary>
	public class ElectronicAddressController : BaseApiController
	{
		#region Private variables
		private int _contactMechanismId = 0;
		private readonly IElectronicAddressRepository _electronicAddressRepository;
		IRepositoryResponse repositoryResponse = new RepositoryResponse();
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public ElectronicAddressController() : base() { }

		/// <summary>
		/// Testing Constructor
		/// </summary>
		/// <param name="electronicAddressRepository">Electronic Address Repository</param>
		public ElectronicAddressController(IElectronicAddressRepository electronicAddressRepository)
		{
			_electronicAddressRepository = electronicAddressRepository;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Link an Electronic Address to a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="linkElectronicAddress">Person's Electronic Address parameter values</param>
		/// <returns>Response with Success Message</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Electronic Address object have invalid entries)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Newly created Contact Mechanism Id", Type = typeof(ElectronicAddress.ElectronicAddressOutputResult))]
		[SwaggerResponseExamples(typeof(ElectronicAddress.ElectronicAddressOutputResult), typeof(LinkElectronicAddressOutputResultExample))]
		[HttpPost]
		[Route("persons/{realPageId}/electronicaddress")]
		public HttpResponseMessage LinkElectronicAddress(Guid realPageId, [FromBody] LinkElectronicAddress linkElectronicAddress)
		{
			realPageId = ((realPageId == Guid.Empty) || (realPageId == null)) ? _realpageUserId : realPageId;
			if ((realPageId == Guid.Empty) || (realPageId == null))
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
			}

			if (linkElectronicAddress == null)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: linkElectronicAddress.");
			}

			IManageContactMechanism contactMechanismLogic = new ManageContactMechanism();

			//Add an PostalAddress and link it to a person
			//Create the Contact Mechanism
			repositoryResponse = contactMechanismLogic.CreateContactMechanism();
			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}
			_contactMechanismId = Convert.ToInt32(repositoryResponse.Id);

			//Associate the Contact Mechanism to a Party
			IPartyContactMechanism partyContactMechanism = new PartyContactMechanism();
			partyContactMechanism = linkElectronicAddress.PartyContactMechanism;
			partyContactMechanism.ContactMechanismId = _contactMechanismId;
			repositoryResponse = contactMechanismLogic.LinkContactMechanismToParty(realPageId, partyContactMechanism);
			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}

			//Assign a usage type to the Contact Mechanism
			partyContactMechanism.PartyContactMechanismId = repositoryResponse.Id;
			repositoryResponse = contactMechanismLogic.LinkUsageTypeToPartyContactMechanism(partyContactMechanism.PartyContactMechanismId, linkElectronicAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId);
			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}

			//Add an ElectronicAddress and link it to a person
			IManageElectronicAddress electronicAddressLogic = new ManageElectronicAddress();
			IElectronicAddress electronicAddress = new ElectronicAddress();
			electronicAddress = linkElectronicAddress.ElectronicAddress;
			electronicAddress.ContactMechanismId = _contactMechanismId;
			repositoryResponse = electronicAddressLogic.CreateElectronicAddress(electronicAddress);
			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}

			ElectronicAddress.ElectronicAddressOutputResult result = new ElectronicAddress.ElectronicAddressOutputResult
			{
				ContactMechanismId = _contactMechanismId
			};

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Update an Electronic Address to a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="linkElectronicAddress">Person's Electronic Address parameter values</param>
		/// <returns>Response with Success Message</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Electronic Address object have invalid entries)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Newly created Contact Mechanism Id", Type = typeof(ElectronicAddress.ElectronicAddressOutputResult))]
		[SwaggerResponseExamples(typeof(ElectronicAddress.ElectronicAddressOutputResult), typeof(LinkElectronicAddressOutputResultExample))]
		[HttpPut]
		[Route("persons/{realPageId}/electronicaddress")]
		public HttpResponseMessage UpdateElectronicAddress(Guid realPageId, [FromBody] LinkElectronicAddress linkElectronicAddress)
		{
			realPageId = ((realPageId == Guid.Empty) || (realPageId == null)) ? _realpageUserId : realPageId;
			if ((realPageId == Guid.Empty) || (realPageId == null))
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
			}

			if (linkElectronicAddress == null)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: linkElectronicAddress.");
			}

			IManageContactMechanism contactMechanismLogic = new ManageContactMechanism();
			if (linkElectronicAddress.PartyContactMechanism != null)
			{
				_contactMechanismId = linkElectronicAddress.PartyContactMechanism.ContactMechanismId;
			}

			repositoryResponse = contactMechanismLogic.UpdateContactMechanismUsageForParty(linkElectronicAddress.PartyContactMechanism.PartyContactMechanismId, linkElectronicAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId);
			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}

			//Add an ElectronicAddress and link it to a person
			IManageElectronicAddress electronicAddressLogic = new ManageElectronicAddress();
			IElectronicAddress electronicAddress = new ElectronicAddress();
			electronicAddress = linkElectronicAddress.ElectronicAddress;
			electronicAddress.ContactMechanismId = _contactMechanismId;
			repositoryResponse = electronicAddressLogic.CreateElectronicAddress(electronicAddress);
			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}

			ElectronicAddress.ElectronicAddressOutputResult result = new ElectronicAddress.ElectronicAddressOutputResult
			{
				ContactMechanismId = _contactMechanismId
			};

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// List Electronic Address details for a Person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>A list of Electronic Address Details for a person</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the password policy", Type = typeof(IElectronicAddress))]
		[SwaggerResponseExamples(typeof(IElectronicAddress), typeof(ElectronicAddressExample))]
		[Route("persons/{realPageId}/electronicaddress")]
		[HttpGet]
		public HttpResponseMessage ListElectronicAddressForPerson(Guid realPageId, string ContactMechanismUsageTypeName = "")
		{
			realPageId = ((realPageId == Guid.Empty) || (realPageId == null)) ? _realpageUserId : realPageId;
			if ((realPageId == Guid.Empty) || (realPageId == null))
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
			}

			IList<ElectronicAddress> electronicAddressList = new List<ElectronicAddress>();
			IManageElectronicAddress electronicAddressLogic = new ManageElectronicAddress();

			if (_electronicAddressRepository == null)
			{
				electronicAddressList = electronicAddressLogic.ListElectronicAddressForPerson(realPageId, ContactMechanismUsageTypeName);
			}
			else
			{
				electronicAddressList = _electronicAddressRepository.ListElectronicAddressForPerson(realPageId, ContactMechanismUsageTypeName);
			}

			if (electronicAddressList != null)
			{
				ObjectListOutput<ElectronicAddress, IErrorData> output = new ObjectListOutput<ElectronicAddress, IErrorData>() { list = electronicAddressList };
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			//When trying to get a list of electronic address for a Person that doesn't exists
			return Request.CreateResponse(HttpStatusCode.NoContent, "Invalid realPageId");
		}
		#endregion

		#region Get Examples
		/// <summary>
		/// Used to document examples of the ElectronicAddress Model webapi result
		/// </summary>
		public class ElectronicAddressExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>ElectronicAddress example</returns>
			public object GetExamples()
			{
				ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType()
				{
					ContactMechanismUsageTypeId = 201,
					ParentContactMechanismUsageTypeId = 200,
					Name = "Primary"
				};

				IElectronicAddress example = new ElectronicAddress()
				{
					ContactMechanismId = 1,
					AddressString = "none@nowhere.com",
					AddressType = "Email",
					contactMechanismUsageType = contactMechanismUsageType
				};

				ObjectOutput<IElectronicAddress, IErrorData> output = new ObjectOutput<IElectronicAddress, IErrorData>() { obj = example };

				return output;
			}
		}
		#endregion

		#region Output results for documentation
		/// <summary>
		/// Used to document examples of the New Electronic Address webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class LinkElectronicAddressOutputResultExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>Newly created Contact Mechanism Id</returns>
			public object GetExamples()
			{
				return ElectronicAddress.LinkElectronicAddressOutputResultExample();
			}
		}
		#endregion
	}
}