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
	/// Postal Address Controller to hold all postal address management related APIs
	/// </summary>

	public class PostalAddressController : BaseApiController
	{
		#region Private variables
		private int _contactMechanismId = 0;
		private readonly IPostalAddressRepository _postalAddressRepository;
		IRepositoryResponse repositoryResponse = new RepositoryResponse();
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public PostalAddressController() : base() { }

		/// <summary>
		/// Testing Constructor
		/// </summary>
		/// <param name="postalAddressRepository">Postal Address Repository</param>
		public PostalAddressController(IPostalAddressRepository postalAddressRepository)
		{
			_postalAddressRepository = postalAddressRepository;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Link an Postal Address to a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="linkPostalAddress">Person's Postal Address parameter values</param>
		/// <returns>Response with Success Message</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Postal Address object have invalid entries)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Newly created Contact Mechanism Id", Type = typeof(PostalAddress.PostalAddressOutputResult))]
		[SwaggerResponseExamples(typeof(PostalAddress.PostalAddressOutputResult), typeof(LinkPostalAddressOutputResultExample))]
		[HttpPost]
		[Route("persons/{realPageId}/postaladdress")]
		public HttpResponseMessage LinkPostalAddress(Guid realPageId, [FromBody] LinkPostalAddress linkPostalAddress)
		{
			realPageId = ((realPageId == Guid.Empty) || (realPageId == null)) ? _realpageUserId : realPageId;
			if ((realPageId == Guid.Empty) || (realPageId == null))
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
			}

			if (linkPostalAddress == null)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: linkPostalAddress.");
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
			partyContactMechanism = linkPostalAddress.PartyContactMechanism;
			partyContactMechanism.ContactMechanismId = _contactMechanismId;
			repositoryResponse = contactMechanismLogic.LinkContactMechanismToParty(realPageId, partyContactMechanism);
			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}

			//Assign a usage type to the Contact Mechanism
			partyContactMechanism.PartyContactMechanismId = repositoryResponse.Id;
			repositoryResponse = contactMechanismLogic.LinkUsageTypeToPartyContactMechanism(partyContactMechanism.PartyContactMechanismId, linkPostalAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId);
			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}

			//Create the Street Address
			IManageStreetAddress streetAddressLogic = new ManageStreetAddress();
			IStreetAddress streetAddress = new StreetAddress();
			streetAddress = linkPostalAddress.StreetAddress;
			streetAddress.ContactMechanismId = _contactMechanismId;
			repositoryResponse = streetAddressLogic.CreateStreetAddress(streetAddress);

			IManageGeographicBoundary manageGeographicBoundary = new ManageGeographicBoundary();
			IContactMechanismBoundary contactMechanismBoundry = new ContactMechanismBoundary();
			contactMechanismBoundry = linkPostalAddress.ContactMechanismBoundary;
			contactMechanismBoundry.ContactMechanismId = _contactMechanismId;
			foreach (var geographicBoundary in linkPostalAddress.GeographicBoundary)
			{
				repositoryResponse = manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary);
				if (repositoryResponse.Id == 0)
				{
					return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
				}
				contactMechanismBoundry.GeographicBoundaryId = Convert.ToInt32(repositoryResponse.Id);
				repositoryResponse = contactMechanismLogic.LinkGeographicBoundaryToContactMechanism(contactMechanismBoundry);
				if (repositoryResponse.Id == 0)
				{
					return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
				}
			}

			PostalAddress.PostalAddressOutputResult result = new PostalAddress.PostalAddressOutputResult
			{
				ContactMechanismId = _contactMechanismId
			};

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Update an Postal Address to a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="linkPostalAddress">Person's Postal Address parameter values</param>
		/// <returns>Response with Success Message</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Postal Address object have invalid entries)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Newly created Contact Mechanism Id", Type = typeof(PostalAddress.PostalAddressOutputResult))]
		[SwaggerResponseExamples(typeof(PostalAddress.PostalAddressOutputResult), typeof(LinkPostalAddressOutputResultExample))]
		[HttpPut]
		[Route("persons/{realPageId}/postaladdress")]
		public HttpResponseMessage UpdatePostalAddress(Guid realPageId, [FromBody] LinkPostalAddress linkPostalAddress)
		{
			realPageId = ((realPageId == Guid.Empty) || (realPageId == null)) ? _realpageUserId : realPageId;
			if ((realPageId == Guid.Empty) || (realPageId == null))
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
			}

			if (linkPostalAddress == null)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: linkPostalAddress.");
			}

			IManageContactMechanism contactMechanismLogic = new ManageContactMechanism();
			if (linkPostalAddress.PartyContactMechanism != null)
			{
				_contactMechanismId = linkPostalAddress.PartyContactMechanism.ContactMechanismId;
			}

			//Expire existing associated Contact Mechanism to a Party
			IPartyContactMechanism partyContactMechanism = new PartyContactMechanism();
			partyContactMechanism = linkPostalAddress.PartyContactMechanism;
			partyContactMechanism.ContactMechanismId = _contactMechanismId;
			repositoryResponse = contactMechanismLogic.LinkContactMechanismToParty(realPageId, partyContactMechanism);
			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}

			//Add an PostalAddress and link it to a person
			//Create the Contact Mechanism
			repositoryResponse = contactMechanismLogic.CreateContactMechanism();
			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}
			_contactMechanismId = Convert.ToInt32(repositoryResponse.Id);

			//Associate the new Contact Mechanism to a Party
			linkPostalAddress.PartyContactMechanism.PartyContactMechanismId = 0;
			partyContactMechanism = new PartyContactMechanism();
			partyContactMechanism = linkPostalAddress.PartyContactMechanism;
			partyContactMechanism.ContactMechanismId = _contactMechanismId;
			repositoryResponse = contactMechanismLogic.LinkContactMechanismToParty(realPageId, partyContactMechanism);
			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}

			//Assign a usage type to the Contact Mechanism
			partyContactMechanism.PartyContactMechanismId = repositoryResponse.Id;
			repositoryResponse = contactMechanismLogic.LinkUsageTypeToPartyContactMechanism(partyContactMechanism.PartyContactMechanismId, linkPostalAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId);
			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}

			//Create the Street Address
			IManageStreetAddress streetAddressLogic = new ManageStreetAddress();
			IStreetAddress streetAddress = new StreetAddress();
			streetAddress = linkPostalAddress.StreetAddress;
			streetAddress.ContactMechanismId = _contactMechanismId;
			repositoryResponse = streetAddressLogic.CreateStreetAddress(streetAddress);

			IManageGeographicBoundary manageGeographicBoundary = new ManageGeographicBoundary();
			IContactMechanismBoundary contactMechanismBoundry = new ContactMechanismBoundary();
			contactMechanismBoundry = linkPostalAddress.ContactMechanismBoundary;
			contactMechanismBoundry.ContactMechanismId = _contactMechanismId;
			foreach (var geographicBoundary in linkPostalAddress.GeographicBoundary)
			{
				repositoryResponse = manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary);
				if (repositoryResponse.Id == 0)
				{
					return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
				}
				contactMechanismBoundry.GeographicBoundaryId = Convert.ToInt32(repositoryResponse.Id);
				repositoryResponse = contactMechanismLogic.LinkGeographicBoundaryToContactMechanism(contactMechanismBoundry);
				if (repositoryResponse.Id == 0)
				{
					return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
				}
			}

			PostalAddress.PostalAddressOutputResult result = new PostalAddress.PostalAddressOutputResult
			{
				ContactMechanismId = _contactMechanismId
			};

			return Request.CreateResponse(HttpStatusCode.OK, result);

		}

		/// <summary>
		/// List Postal Address details for a Person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>A list of Postal Address Details for a person</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the password policy", Type = typeof(IPostalAddress))]
		[SwaggerResponseExamples(typeof(IPostalAddress), typeof(PostalAddressExample))]
		[Route("persons/{realPageId}/postaladdress")]
		[HttpGet]
		public HttpResponseMessage ListPostalAddressForPerson(Guid realPageId, string ContactMechanismUsageTypeName = "")
		{
			realPageId = ((realPageId == Guid.Empty) || (realPageId == null)) ? _realpageUserId : realPageId;
			if ((realPageId == Guid.Empty) || (realPageId == null))
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: realPageId");
			}

			IList<PostalAddress> postalAddressList = new List<PostalAddress>();
			IManagePostalAddress postalAddressLogic = new ManagePostalAddress();

			if (_postalAddressRepository == null)
			{
				postalAddressList = postalAddressLogic.ListPostalAddressForPerson(realPageId, ContactMechanismUsageTypeName);
			}
			else
			{
				postalAddressList = _postalAddressRepository.ListPostalAddressForPerson(realPageId, ContactMechanismUsageTypeName);
			}

			if (postalAddressList != null)
			{
				ObjectListOutput<PostalAddress, IErrorData> output = new ObjectListOutput<PostalAddress, IErrorData>() { list = postalAddressList };
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			//When trying to get a list of postal address for a Person that doesn't exists
			return Request.CreateResponse(HttpStatusCode.NoContent, "Invalid realPageId");
		}
		#endregion

		#region Get Examples
		/// <summary>
		/// Used to document examples of the PostalAddress Model webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class PostalAddressExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>PostalAddress example</returns>
			public object GetExamples()
			{
				ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType()
				{
					ContactMechanismUsageTypeId = 201,
					ParentContactMechanismUsageTypeId = 200,
					Name = "Primary"
				};

				IPostalAddress example = new PostalAddress()
				{
					ContactMechanismId = 1,
					AddressString = "2201 Lakeside Blvd",
					AddressType = "StreetAddress1",
					contactMechanismUsageType = contactMechanismUsageType
				};

				ObjectOutput<IPostalAddress, IErrorData> output = new ObjectOutput<IPostalAddress, IErrorData>() { obj = example };

				return output;
			}
		}
		#endregion

		#region Output results for documentation
		/// <summary>
		/// Used to document examples of the New Postal Address webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class LinkPostalAddressOutputResultExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>Newly created Contact Mechanism Id</returns>
			public object GetExamples()
			{
				return PostalAddress.LinkPostalAddressOutputResultExample();
			}
		}
		#endregion
	}
}