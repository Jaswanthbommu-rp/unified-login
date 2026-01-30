using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.ResponseObject;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// User Properties Sync Controller
    /// </summary>
    [ApiController]
    [Authorize]
    public class UserPropertiesSyncController : ControllerBase
    {
        private readonly IUserClaimsAccessor _userClaimsAccessor;
        private IProductRepository _productRepository;
        private IProductInternalSettingRepository _productInternalSettingRepository;
        private IManageProduct _manageProduct;
        private IRepositoryResponse _repositoryResponse;
        private IManageOrganization _manageOrganization;
        private IUserPropertiesSyncRepository _userPropertiesSyncRepository;
        private IManageUserPropertiesSync _manageUserPropertiesSync;
        private DefaultUserClaim _userClaims;
        private static readonly Guid EmployeeCompanyRealPageId = new Guid("8e31a5ec-c884-4f65-89c7-98e7f55a1a7e");

        #region Constructor
        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userClaimsAccessor">User claims accessor</param>
        public UserPropertiesSyncController(IUserClaimsAccessor userClaimsAccessor)
        {
            _userClaimsAccessor = userClaimsAccessor;
            _userClaims = _userClaimsAccessor.GetUserClaim();

            _repositoryResponse = new RepositoryResponse();
            _manageProduct = new ManageProduct(_userClaims);
            _manageOrganization = new ManageOrganization(_userClaims);
            _productRepository = new ProductRepository(_userClaims);
            _userPropertiesSyncRepository = new UserPropertiesSyncRepository();
            _manageUserPropertiesSync = new ManageUserPropertiesSync(_userClaims);
        }
        #endregion

        /// <summary>
        /// Translate and Saves the User Product primary properties based on jon task.
        /// </summary>
        /// <returns>If success then returns ok.</returns>
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [Route("userpropertiessync")]
        [HttpPost]
        public async Task<IActionResult> UserPropertiesSync(UserSyncJobTask userSyncJobTask)
        {
            return await Task.Run<IActionResult>(() =>
            {
                ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;

                if (userSyncJobTask == null)
                {
                    var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                    errorResponse.Errors.Add(new Error
                    {
                        Title = "Error",
                        Source = "/UserPropertiesSync",
                        Detail = "invalid User Sync Job Task Object.",
                        StatusCode = ""
                    });

                    return BadRequest(errorResponse);
                }

                if (currentClaimPrincipal.HasClaim("scope", "internalapi"))
                {
                    if (!string.IsNullOrEmpty(userSyncJobTask.UserOrgRealpageId.ToString()))
                    {
                        Guid adminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(userSyncJobTask.UserOrgRealpageId);

                        if (adminCreatorRealPageId == Guid.Empty)
                        {
                            var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                            errorResponse.Errors.Add(new Error
                            {
                                Title = "Error",
                                Source = "/UserPropertiesSync",
                                Detail = "Invalid UPFMId.",
                                StatusCode = ""
                            });

                            return BadRequest(errorResponse);
                        }

                        _userClaims = RecreateClaimsForClient(adminCreatorRealPageId);
                        _manageProduct = new ManageProduct(_userClaims);
                    }
                    else
                    {
                        var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                        errorResponse.Errors.Add(new Error
                        {
                            Title = "Error",
                            Source = "/UserPropertiesSync",
                            Detail = "Invalid UPFMId.",
                            StatusCode = ""
                        });

                        return BadRequest(errorResponse);
                    }
                }
                else
                {
                    var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                    errorResponse.Errors.Add(new Error
                    {
                        Title = "Error",
                        Source = "/UserPropertiesSync",
                        Detail = "Invalid Claim Scope.",
                        StatusCode = ""
                    });

                    return BadRequest(errorResponse);
                }

                _manageUserPropertiesSync = new ManageUserPropertiesSync(_userClaims);
                var response = _manageUserPropertiesSync.TranslateAndSaveUserProductProperties(userSyncJobTask);

                return StatusCode(StatusCodes.Status202Accepted, response);
            });
        }

        private DefaultUserClaim RecreateClaimsForClient(Guid realpageUserId)
        {
            DefaultUserClaim userClaim = new DefaultUserClaim();

            if (!string.IsNullOrEmpty(realpageUserId.ToString()))
            {
                IManagePerson personLogic = new ManagePerson();
                Person person = personLogic.GetPerson(realpageUserId);
                if (person == null)
                {
                    throw new Exception($"Missing persona information for client_info user while Recreation of Claims For Client.  realPageId: {realpageUserId}");
                }

                IManageUserLogin userLoginLogic = new ManageUserLogin();
                IManageUserRoleRight userRoleRight = new ManageUserRoleRight();
                var userLogin = userLoginLogic.GetUserLoginOnly(realpageUserId);

                IManagePersona managePersona = new ManagePersona();
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
                    RealPageEmployee = (persona.Organization.RealPageId == EmployeeCompanyRealPageId)
                };

                ManageProductBatch manageProductBatch = new ManageProductBatch(userClaim);
                userClaim.Rights = manageProductBatch.GetPersonaRoleRights(persona.PersonaId, persona.Organization.PartyId);
            }

            return userClaim;
        }
    }
}
