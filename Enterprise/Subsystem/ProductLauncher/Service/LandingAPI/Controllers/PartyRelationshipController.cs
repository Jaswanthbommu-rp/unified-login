using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// Party Relationship Controller
	/// </summary>
	public class PartyRelationshipController : BaseApiController
	{
		#region Private variables
		private readonly IPartyRelationshipRepository _partyRelationshipRepository;
		IRepositoryResponse repositoryResponse = new RepositoryResponse();
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public PartyRelationshipController() : base() { }

		/// <summary>
		/// Testing Constructor
		/// </summary>
		/// <param name="partyRelationshipRepository">Party Relationship Repository</param>
		public PartyRelationshipController(IPartyRelationshipRepository partyRelationshipRepository)
		{
			_partyRelationshipRepository = partyRelationshipRepository;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Link a Organization to an Organization
		/// </summary>
		/// <param name="RealPageIdFrom">From Organization unique identifier</param>
		/// <param name="partyRelationship">To Party Unique Identifier and From/To Relationship RoleType</param>
		/// <returns>Response with Success Message</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Organization object have invalid entries / when Information is out of sync with the server)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Newly created User Id", Type = typeof(PartyRelationship.PartyRelationshipOutputResult))]
		[SwaggerResponseExamples(typeof(PartyRelationship.PartyRelationshipOutputResult), typeof(NewPartyRelationshipOutputResultExample))]
		[HttpPost]
		[Route("organizations/{RealPageIdFrom}/relationships/organizations")]
		public HttpResponseMessage LinkOrganizationToOrganization(Guid RealPageIdFrom, [FromBody] PartyRelationship partyRelationship)
		{
			if (RealPageIdFrom == Guid.Empty)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: RealPageIdFrom.");
			}

			if (partyRelationship == null)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: partyRelationship.");
			}

			if (partyRelationship.RealPageIdTo == Guid.Empty)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: RealPageIdTo.");
			}

			if (partyRelationship.RoleTypeIdFrom <= 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: RoleTypeIdFrom.");
			}

			if (partyRelationship.RoleTypeIdTo <= 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: RoleTypeIdTo.");
			}

			//Link Organization to Organization
			IManagePartyRelationship partyRelationshipLogic = new ManagePartyRelationship();
			if (_partyRelationshipRepository == null)
			{
				repositoryResponse = partyRelationshipLogic.LinkOrganizationToOrganization(RealPageIdFrom, partyRelationship);
			}
			else
			{
				repositoryResponse = _partyRelationshipRepository.LinkOrganizationToOrganization(RealPageIdFrom, partyRelationship);
			}

			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}

			PartyRelationship.PartyRelationshipOutputResult result = new PartyRelationship.PartyRelationshipOutputResult
			{
				NewPartyRelationshipId = repositoryResponse.Id
			};

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Link a Person to an Organization
		/// </summary>
		/// <param name="RealPageIdFrom">Person unique identifier</param>
		/// <param name="partyRelationship">To Party Unique Identifier and From/To Relationship RoleType</param>
		/// <returns>Response with Success Message</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Organization object have invalid entries / when Information is out of sync with the server)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Newly created User Id", Type = typeof(PartyRelationship.PartyRelationshipOutputResult))]
		[SwaggerResponseExamples(typeof(PartyRelationship.PartyRelationshipOutputResult), typeof(NewPartyRelationshipOutputResultExample))]
		[HttpPost]
		[Route("persons/{RealPageIdFrom}/relationships/organizations")]
		public HttpResponseMessage LinkPersonToOrganization(Guid RealPageIdFrom, [FromBody] PartyRelationship partyRelationship)
		{
			RealPageIdFrom = ((RealPageIdFrom == Guid.Empty) || (RealPageIdFrom == null)) ? _realpageUserId : RealPageIdFrom;
			if ((RealPageIdFrom == Guid.Empty) || (RealPageIdFrom == null))
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: PersonRealPageId.");
			}

			if (partyRelationship == null)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Null parameter: partyRelationship.");
			}

			if (partyRelationship.RealPageIdTo == Guid.Empty)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: RealPageIdTo");
			}

			if (partyRelationship.RoleTypeIdFrom <= 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: RoleTypeIdFrom.");
			}

			if (partyRelationship.RoleTypeIdTo <= 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameter: RoleTypeIdTo.");
			}

			//Link Person to Organization
			IManagePartyRelationship partyRelationshipLogic = new ManagePartyRelationship();
			if (_partyRelationshipRepository == null)
			{
				repositoryResponse = partyRelationshipLogic.LinkPersonToOrganization(RealPageIdFrom, partyRelationship);
			}
			else
			{
				repositoryResponse = _partyRelationshipRepository.LinkPersonToOrganization(RealPageIdFrom, partyRelationship);
			}
			if (repositoryResponse.Id == 0)
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest, repositoryResponse.ErrorMessage);
			}

			PartyRelationship.PartyRelationshipOutputResult result = new PartyRelationship.PartyRelationshipOutputResult
			{
				NewPartyRelationshipId = repositoryResponse.Id
			};

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}
		#endregion

		#region Output results for documentation
		/// <summary>
		/// Used to document examples of the New Person webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class NewPartyRelationshipOutputResultExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>Newly created user id</returns>
			public object GetExamples()
			{
				return PartyRelationship.GetNewPartyRelationshipExample();
			}
		}
		#endregion
	}
}