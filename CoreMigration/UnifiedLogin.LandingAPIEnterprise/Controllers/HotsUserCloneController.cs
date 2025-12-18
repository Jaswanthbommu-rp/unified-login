using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.ServiceDefaults;
using UnifiedLogin.SharedObjects.Hots;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Rum;
using UnifiedLogin.SharedObjects.ResponseObject;

namespace UnifiedLogin.LandingAPIEnterprise.Controllers
{
	/// <summary>
	/// HotsUserCloneController for managing HOTS user cloning operations
	/// </summary>
	[Authorize]
	[ApiController]
	[ApiVersion("1.0")]
	[Route("v{version:apiVersion}/[controller]")]
	public class HotsUserCloneController : ControllerBase
	{
		private readonly IProductRepository _productRepository;
		private readonly IManagePersona _managePersona;
		private readonly IManageProduct _manageProduct;
		private readonly IManageHotsCloneUsers _manageHotsCloneUsers;
		private readonly IManageOrganization _manageOrganization;
		private readonly IUserClaimsAccessor _userClaimsAccessor;
		private readonly IRepository _repository;
		private readonly HttpMessageHandler _messageHandler;
		private readonly DefaultUserClaim _userClaims;

		#region Constructor

		/// <summary>
		/// Constructor with dependency injection for HOTS user clone controller.
		/// Follows modern ASP.NET Core patterns for testable, maintainable code.
		/// </summary>
		/// <param name="repository">Data access repository</param>
		/// <param name="messageHandler">HTTP message handler for external calls</param>
		/// <param name="userClaims">Legacy user claims object (for backward compatibility)</param>
		/// <param name="userClaimsAccessor">Accessor for current authenticated user's claims</param>
		/// <param name="managePersona">Persona management service</param>
		/// <param name="manageProduct">Product management service</param>
		/// <param name="manageOrganization">Organization management service</param>
		public HotsUserCloneController(
			IRepository repository,
			HttpMessageHandler messageHandler,
			DefaultUserClaim userClaims,
			IUserClaimsAccessor userClaimsAccessor,
			IManagePersona managePersona,
			IManageProduct manageProduct,
			IManageOrganization manageOrganization)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
			_userClaims = userClaims ?? throw new ArgumentNullException(nameof(userClaims));
			_userClaimsAccessor = userClaimsAccessor ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
			_managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
			_manageProduct = manageProduct ?? throw new ArgumentNullException(nameof(manageProduct));
			_manageOrganization = manageOrganization ?? throw new ArgumentNullException(nameof(manageOrganization));

			_productRepository = new ProductRepository(repository, userClaims);
			_manageHotsCloneUsers = new ManageHotsCloneUsers(userClaims);
		}

		#endregion

		/// <summary>
		/// Create a user in RealPage Unified platform and assign product(s).
		/// </summary>
		/// <param name="cloneUsers">Clone users request object</param>
		/// <returns>If success then returns real page id for newly created user else error object.</returns>
		[SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Bad request when Request object have invalid entries.")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Internal Server Error.")]
		[SwaggerResponse((int)HttpStatusCode.Accepted, Description = "Request has been accepted for further processing.")]
		[Route("userclone")]
		[HttpPost]
		public ActionResult HOTCloneUsers(CloneUsers cloneUsers)
		{
			ClaimsPrincipal currentClaimPrincipal = User;

			if (cloneUsers == null)
			{
				var errorResponse = new ErrorResponse { Errors = new List<Error>() };
				errorResponse.Errors.Add(new Error
				{ Title = "Error", Source = "/HotsCloneUser", Detail = "Null request received.", StatusCode = "" });

				return BadRequest(errorResponse);
			}

			// Validate claim scope
			if (!currentClaimPrincipal.HasClaim("scope", "usermanagement"))
			{
				var errorResponse = new ErrorResponse { Errors = new List<Error>() };
				errorResponse.Errors.Add(new Error
				{ Title = "Error", Source = "/HotsCloneUser", Detail = "Invalid Claim Scope.", StatusCode = "" });

				return BadRequest(errorResponse);
			}

			// Validate UPFMId
			if (cloneUsers.CloneCustomerUPFMId == null || cloneUsers.CloneCustomerUPFMId == Guid.Empty)
			{
				var errorResponse = new ErrorResponse { Errors = new List<Error>() };
				errorResponse.Errors.Add(new Error
				{ Title = "Error", Source = "/HotsCloneUser", Detail = "Invalid Clone Customer UPFMId.", StatusCode = "" });

				return BadRequest(errorResponse);
			}

			// Get admin creator for the clone company
			Guid adminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(cloneUsers.CloneCustomerUPFMId);
			if (adminCreatorRealPageId == Guid.Empty)
			{
				var errorResponse = new ErrorResponse { Errors = new List<Error>() };
				errorResponse.Errors.Add(new Error
				{ Title = "Error", Source = "/HotsCloneUser", Detail = "Invalid UPFMId.", StatusCode = "" });

				return BadRequest(errorResponse);
			}

			// Recreate claims for client
			var cloneCompanyUserClaim = RecreateClaimsForClient(adminCreatorRealPageId);

			// Get base company UPFM ID
			var baseUpfmId = _manageHotsCloneUsers.GetBaseCompanyUPFMId(cloneUsers.CloneCustomerUPFMId);

			// Get base organization
			Organization baseOrg = _manageOrganization.GetOrganization(baseUpfmId);
			if (baseOrg == null)
			{
				var errorResponse = new ErrorResponse { Errors = new List<Error>() };
				errorResponse.Errors.Add(new Error
				{ Title = "Error", Source = "/HotsCloneUser", Detail = "Base Line Organization not found.", StatusCode = "" });

				return BadRequest(errorResponse);
			}

			// Get base organization admin
			Guid baseOrgAdminRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(baseUpfmId);
			var baseOrgAdminPersona = _managePersona.GetActivePersonaWithoutRights(baseOrgAdminRealPageId);
			var baseOrgAdminClaim = RecreateClaimsForClient(baseOrgAdminRealPageId);

			// Get clone organization
			var cloneOrg = _manageOrganization.GetOrganization(cloneUsers.CloneCustomerUPFMId);
			if (cloneOrg == null)
			{
				var errorResponse = new ErrorResponse { Errors = new List<Error>() };
				errorResponse.Errors.Add(new Error
				{ Title = "Error", Source = "/HotsCloneUser", Detail = "Clone Customer Organization not found.", StatusCode = "" });

				return BadRequest(errorResponse);
			}

			// Create instance with updated claims
			var manageHotsCloneUsers = new ManageHotsCloneUsers(cloneCompanyUserClaim);

			// Clone users from baseline company
			var clonedUserResult = manageHotsCloneUsers.CloneUsersFromBaseLineCompany(
				cloneUsers,
				baseOrg.PartyId,
				cloneOrg.PartyId,
				baseOrgAdminClaim,
				baseOrgAdminPersona.PersonaId);

			return StatusCode((int)HttpStatusCode.Accepted, clonedUserResult);
		}

		/// <summary>
		/// Recreates user claims for a client based on their real page user ID
		/// </summary>
		/// <param name="realpageUserId">The real page user ID</param>
		/// <returns>DefaultUserClaim object with user information</returns>
		private DefaultUserClaim RecreateClaimsForClient(Guid realpageUserId)
		{
			DefaultUserClaim userClaim = new DefaultUserClaim();

			if (realpageUserId == Guid.Empty)
			{
				throw new ArgumentException("Invalid RealPage User ID", nameof(realpageUserId));
			}

			IManagePerson personLogic = new ManagePerson(_repository);
			Person person = personLogic.GetPerson(realpageUserId);
			if (person == null)
			{
				throw new Exception($"Missing persona information for client_info user while Recreation of Claims For Client.  realPageId: {realpageUserId}");
			}

			IManageUserLogin userLoginLogic = new ManageUserLogin(_repository, _userClaims, _messageHandler);
			var userLogin = userLoginLogic.GetUserLoginOnly(realpageUserId);

			IManagePersona managePersona = new ManagePersona(_repository, _userClaims, _messageHandler);
			//Active Persona is linked to one organization
			Persona persona = managePersona.GetActivePersonaWithoutRights(realpageUserId); // this user can only be under 1 company to work correctly

			userClaim = new DefaultUserClaim
			{
				UserId = (int)userLogin.UserId,
				OrganizationPartyId = persona.Organization.PartyId,
				LoginName = userLogin.LoginName,
				OrganizationMasterId = (long)persona.Organization.BooksMasterId,
				CustomerMasterId = (long)persona.Organization.BooksMasterId,
				OrganizationName = persona.Organization.Name.ToString(),
				FirstName = person.FirstName,
				LastName = person.LastName,
				PersonaId = persona.PersonaId,
				OrganizationRealPageGuid = persona.Organization.RealPageId,
				UserRealPageGuid = realpageUserId,
				CorrelationId = Guid.NewGuid(),
				RealPageEmployee = persona.Organization.Name.ToUpper() == "REALPAGE EMPLOYEE"
			};

			return userClaim;
		}
	}
}