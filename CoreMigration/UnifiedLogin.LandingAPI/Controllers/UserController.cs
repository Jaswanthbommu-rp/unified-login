using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Security;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.ThirdParty;
using UnifiedLogin.LandingAPI.Attributes;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;
using UnifiedLogin.SharedObjects.Product.Rum;
using SO = UnifiedLogin.SharedObjects;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Controller to hold all user management related APIs
    /// </summary>
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class UserController : ControllerBase
    {
        #region Private variables
        private readonly IUserClaimsAccessor _userClaimsAccessor;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userClaimsAccessor">User claims accessor</param>
        public UserController(IUserClaimsAccessor userClaimsAccessor)
        {
            _userClaimsAccessor = userClaimsAccessor;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Give administrators access to missing products based on a customer company
        /// </summary>
        /// <param name="organizationRealPageId">Organization enterprise Id</param>
        /// <param name="assignUserPersonaId">Assigned to user PersonaId</param>
        /// <returns>HTTP response message including the status code and data.</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(ObjectOutput<Guid, IErrorData>), (int)HttpStatusCode.OK)]
        [HttpPost("user/assignproductstoadministrators")]
        public async Task<IActionResult> AssignProductsToAdministrators(Guid organizationRealPageId, long assignUserPersonaId = 0)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                IRepositoryResponse repositoryResponse = new RepositoryResponse();
                ObjectOutput<Guid, IErrorData> output = new ObjectOutput<Guid, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                output.obj = organizationRealPageId;

                if ((organizationRealPageId == Guid.Empty) || (organizationRealPageId == null))
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.AssignProductsToAdministrators.1";
                    errorStatus.ErrorMsg = "Invalid parameter: organizationRealPageId";
                    output.Status = errorStatus;
                    return Ok(output);
                }

                if (assignUserPersonaId < 0)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.AssignProductsToAdministrators.2";
                    errorStatus.ErrorMsg = "Invalid parameter: assignUserPersonaId";
                    output.Status = errorStatus;
                    return Ok(output);
                }

                IManageUser manageUser = new ManageUser(userClaim);
                repositoryResponse = manageUser.AssignProductsToAdministrators(organizationRealPageId, assignUserPersonaId);

                if (repositoryResponse.Id == 0 || !string.IsNullOrWhiteSpace(repositoryResponse.ErrorMessage))
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.AssignProductsToAdministrators.3";
                    errorStatus.ErrorMsg = repositoryResponse.ErrorMessage;
                    output.Status = errorStatus;
                    return Ok(output);
                }

                output.Status = errorStatus;
                return Ok(output);
            });
        }

        /// <summary>
        /// Get a user Profile detail
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <returns>Profile object</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(IProfileDetail), (int)HttpStatusCode.OK)]
        [HttpGet("user/{realPageId}")]
        public async Task<IActionResult> GetUserProfile(Guid realPageId)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                IManageUser manageUser = new ManageUser(userClaim);

                ObjectOutput<IProfileDetail, IErrorData> output = manageUser.GetUserProfile(realPageId, userClaim.UserRealPageGuid, userClaim.OrganizationPartyId);

                return Ok(output);
            });
        }

        /// <summary>
        /// Get a users product detail
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <returns>Profile object</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UserProducts), (int)HttpStatusCode.OK)]
        [HttpGet("user/{realPageId}/products")]
        [AuthorizeScope("userinfoapi")]
        public async Task<IActionResult> GetUserProducts(Guid realPageId)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                ClaimsPrincipal currentClaimPrincipal = User;

                if (!currentClaimPrincipal.Identity.IsAuthenticated)
                {
                    return Unauthorized();
                }

                ObjectOutput<IProfileDetail, IErrorData> output = new ObjectOutput<IProfileDetail, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                IPerson person = new Person();

                realPageId = (realPageId == Guid.Empty) ? userClaim.UserRealPageGuid : realPageId;
                if (realPageId == Guid.Empty)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.GetUserProducts.1";
                    errorStatus.ErrorMsg = "Get User Products: Invalid parameter realPageId";
                    output.Status = errorStatus;
                    return BadRequest(output);
                }

                IManagePerson personLogic = new ManagePerson();
                person = personLogic.GetPerson(realPageId);

                if (person != null)
                {
                    IManagePersona _managePersona = new ManagePersona(userClaim);
                    IManageOrganization _manageOrganization = new ManageOrganization(userClaim);
                    IManageProduct _manageProduct = new ManageProduct(userClaim);
                    UserProductOutputResult productResult = new UserProductOutputResult();

                    Persona persona = _managePersona.GetFirstAvailablePersonaByCompany(realPageId, userClaim.OrganizationPartyId);
                    //Verify if same company
                    bool isValidOrganization = _manageOrganization.ValidateOrganization(userClaim.OrganizationMasterId, userClaim.UserRealPageGuid, persona.Organization.RealPageId);
                    if (!isValidOrganization)
                    {
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "User.GetProfile.3";
                        errorStatus.ErrorMsg = "Get User Profile: User exists in a different organization.";
                        output.Status = errorStatus;
                        return Ok(output);
                    }

                    IList<PersonaProductUserDetails> products = _manageProduct.GetUserAssignedProductsByPersona(persona);
                    IList<PersonaProductUserDetails> resources = _manageProduct.GetUserAssignedProductsByPersona(persona: persona, productSelectType: ProductSelectType.ResourcesOnly, security: null);
                    var productIconSettings = _manageProduct.GetProductSettingByType("ProductIcon");

                    productResult.Products = ConvertDashboardProductsToRAUL(products, productIconSettings);
                    productResult.Resources = ConvertDashboardProductsToRAUL(resources, productIconSettings);

                    if (productResult.Resources.Any(m => m.Id == 89))
                    {
                        IManageUnifiedSettings manageSettings = new ManageUnifiedSettings(userClaim);
                        var internalSettings = manageSettings.GetUnifiedSettingsCached("security", userClaim.OrganizationPartyId);
                        var supportPortalTileAccess = internalSettings.FirstOrDefault(a => a.Name == "hidesupportportaltile");
                        string settingValue = supportPortalTileAccess == null ? "null" : supportPortalTileAccess.Value;
                        if (supportPortalTileAccess == null || supportPortalTileAccess.Value == "1")
                        {
                            var adminSupportPortalResource = productResult.Resources.FirstOrDefault(m => m.Id == 89);
                            productResult.Resources.Remove(adminSupportPortalResource);
                        }

                        if (persona.UserTypeId == 404 && productResult.Resources.Any(m => m.Id == 89))
                        {
                            IManageContactMechanism contactMechanism = new ManageContactMechanism();
                            IList<CommonAddress> commonAddressList = contactMechanism.ListContactMechanismForPerson(realPageId, "Email Notification");
                            CommonAddress ca = commonAddressList.Where(c => c.contactMechanismUsageType != null).FirstOrDefault();
                            if (ca == null || (ca != null && string.IsNullOrEmpty(ca.AddressString)))
                            {
                                var adminSupportPortalResource = productResult.Resources.FirstOrDefault(m => m.Id == 89);
                                productResult.Resources.Remove(adminSupportPortalResource);
                            }
                        }
                    }

                    return Ok(productResult);
                }

                errorStatus.Success = false;
                errorStatus.ErrorCode = "User.GetUserProducts.2";
                errorStatus.ErrorMsg = "Get User Products: No data.";
                output.Status = errorStatus;
                return Ok(output);
            });
        }

        /// <summary>
        /// Get a user Profile detail for clone
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <returns>Profile object</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(IProfileDetail), (int)HttpStatusCode.OK)]
        [HttpGet("userclone/{realPageId}")]
        [AuthorizeRight("cloneuser")]
        public async Task<IActionResult> GetUserProfileForClone(Guid realPageId)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                ObjectOutput<IProfileDetail, IErrorData> output = new ObjectOutput<IProfileDetail, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                IPerson person = new Person();

                realPageId = (realPageId == Guid.Empty) ? userClaim.UserRealPageGuid : realPageId;
                if (realPageId == Guid.Empty)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.GetProfile.1";
                    errorStatus.ErrorMsg = "Get User Profile: Invalid parameter realPageId";
                    output.Status = errorStatus;
                    return Ok(output);
                }

                IManagePerson personLogic = new ManagePerson();
                person = personLogic.GetPerson(realPageId);

                if (person != null)
                {
                    //Include the UserLogin details.  IsActive and Is3rdPartyIDP are used by the Edit User
                    IManageUserLogin userLoginLogic = new ManageUserLogin();
                    UserLogin userLogin = userLoginLogic.GetUserLogin(realPageId, userClaim.OrganizationPartyId);

                    IProfileDetail profileDetail = new ProfileDetail();
                    profileDetail.userLogin.Is3rdPartyIDP = userLogin.Is3rdPartyIDP;
                    profileDetail.userLogin.IsActive = userLogin.IsActive;
                    profileDetail.userLogin.FromDate = DateTime.UtcNow;
                    profileDetail.NotificationEmail = null;

                    if (profileDetail.TelecommunicationNumber.Count == 0)
                    {
                        ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType();
                        TelecommunicationNumber teleCommunicationNumber = new TelecommunicationNumber();
                        teleCommunicationNumber.contactMechanismUsageType = contactMechanismUsageType;
                        profileDetail.TelecommunicationNumber.Add(teleCommunicationNumber);
                    }

                    IManagePersona managePersona = new ManagePersona();
                    //Active Persona is linked to one organization
                    Persona persona = managePersona.GetFirstAvailablePersonaByCompany(realPageId, userClaim.OrganizationPartyId);
                    if (persona != null)
                    {
                        IManagePartyRelationship managePartyRelationship = new ManagePartyRelationship();
                        PartyRelationship partyRelationship = managePartyRelationship.GetPartyRelationship(realPageId, persona.Organization.RealPageId, "", "", "User Type");
                        if (partyRelationship != null)
                        {
                            profileDetail.UserTypeId = partyRelationship.RoleTypeIdFrom;
                        }

                        Persona clonePersona = new Persona
                        {
                            PersonaId = persona.PersonaId,
                            PersonaEnvironmentTypeId = persona.PersonaEnvironmentTypeId,
                            PersonaTypeId = persona.PersonaTypeId,
                            Name = persona.Name,
                            RealPageId = persona.RealPageId
                        };
                        profileDetail.Persona.Add(clonePersona);
                    }

                    if (FeatureFlag.GetUserCompanyAssociationFeatureFlag())
                    {
                        IUserRepository _userRepository = new UserRepository(userClaim);
                        IManageUserLoginPersona manageUserLoginPersona = new ManageUserLoginPersona(userClaim);
                        IList<UserLoginPersona> userLoginPersonaList = manageUserLoginPersona.ListUserLoginPersona(userLoginPersonaId: null, userLoginId: userLogin.UserId, organizationPartyId: userClaim.OrganizationPartyId);
                        var data = _userRepository.GetExternalUserRelationship(userLoginPersonaList[0].UserLoginPersonaId);
                        profileDetail.ExternalUserRelationship = data == null ? new ExternalUserRelationship()
                        {
                            UserLoginPersonaId = userLoginPersonaList[0].UserLoginPersonaId
                        } : data;
                    }
                    if (profileDetail != null)
                    {
                        output.obj = profileDetail;
                        output.Status = errorStatus;
                        return Ok(output);
                    }
                }

                errorStatus.Success = false;
                errorStatus.ErrorCode = "User.GetProfile.2";
                errorStatus.ErrorMsg = "Get User Profile: No data.";
                output.Status = errorStatus;
                return Ok(output);
            });
        }

        /// <summary>
        /// Update User Detail and Products
        /// </summary>
        /// <param name="profile">Edited User detail and Products</param>
        /// <returns>Response with Success Message</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(IProfileDetail), (int)HttpStatusCode.OK)]
        [HttpPut("user")]
        [AuthorizeRight("editusers", "editotherprofile", "editownprofile")]
        public async Task<IActionResult> UpdateUser([FromBody] ProfileDetail profile)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                IRepositoryResponse repositoryResponse = new RepositoryResponse();
                ObjectOutput<IProfileDetail, IErrorData> output = new ObjectOutput<IProfileDetail, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                output.obj = profile;

                IManageUserLogin manageUserLogin = new ManageUserLogin(userClaim);
                IUserLoginOnly userLoginOnly = manageUserLogin.GetUserLoginOnly(profile.RealPageId);

                IManageRoleType manageRoleType = new ManageRoleType();
                long bookCustomerMasterId = profile.organization.Select(o => o.BooksCustomerMasterId).FirstOrDefault();
                IList<RoleType> userRoles = manageRoleType.GetRoleTypeDependency(roleTypeId: profile.UserTypeId, partyId: userClaim.OrganizationPartyId, orgMasterId: bookCustomerMasterId, loginName: userLoginOnly.LoginName);
                if (userRoles == null)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.UpdateUser.3";
                    errorStatus.ErrorMsg = "User roles are missing.";
                    output.Status = errorStatus;
                    return Ok(output);
                }

                if (profile == null)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.UpdateUser.1";
                    errorStatus.ErrorMsg = "Profile is required.";
                    output.Status = errorStatus;
                    return Ok(output);
                }
                else if (profile.IsFirstNameNullOrWhiteSpace)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.UpdateUser.10";
                    errorStatus.ErrorMsg = "First name is required.";
                    output.Status = errorStatus;
                    return Ok(output);
                }
                else if (profile.IsLastNameNullOrWhiteSpace)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.UpdateUser.11";
                    errorStatus.ErrorMsg = "Last name is required.";
                    output.Status = errorStatus;
                    return Ok(output);
                }
                else if (profile.userLogin.IsLoginNameNullOrWhiteSpace)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.UpdateUser.4";
                    errorStatus.ErrorMsg = "Username is required.";
                    output.Status = errorStatus;
                    return Ok(output);
                }
                else if (!userRoles.Any(r => r.PartyRoleTypeId == profile.UserTypeId))
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.UpdateUser.5";
                    errorStatus.ErrorMsg = "Invalid user type.";
                    output.Status = errorStatus;
                    return Ok(output);
                }
                else if (!String.IsNullOrWhiteSpace(profile.NotificationEmail) && !EmailFormatValidation.IsValidEmail(profile.NotificationEmail))
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.UpdateUser.6";
                    errorStatus.ErrorMsg = "Notification email is not a valid email address.";
                    output.Status = errorStatus;
                    return Ok(output);
                }
                else if (profile.UserTypeId == (int)UserRoleType.User &&
                         !EmailFormatValidation.IsValidEmail(profile.userLogin.LoginName))
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.UpdateUser.7";
                    errorStatus.ErrorMsg = "Username is not a valid email address.";
                    output.Status = errorStatus;
                    return Ok(output);
                }
                else if (profile.userLogin.FromDate.HasValue == false)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.UpdateUser.8";
                    errorStatus.ErrorMsg = "Effective date is required.";
                    output.Status = errorStatus;
                    return Ok(output);
                }

                ProductBatch resPortal = profile.productBatch.FirstOrDefault<ProductBatch>((Func<ProductBatch, bool>)(p => p.ProductId == (int)ProductEnum.ResidentPortal));
                if (resPortal != null)
                {
                    //verify resident portal user has same or higher access level
                    IManageProductResidentPortal manageResidentPortal = new ManageProductResidentPortal(userClaim);
                    bool hasAccess = manageResidentPortal.ValidateUserAccess(userClaim.UserRealPageGuid, profile.Persona[0].PersonaId);
                    if (!hasAccess)
                    {
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "User.UpdateUser.13";
                        errorStatus.ErrorMsg = "Validate Resident Portal User Access: You do not have the permissions to edit this user's role.";
                        output.Status = errorStatus;
                        return Ok(output);
                    }
                }

                bool isValidUsername = manageUserLogin.ValidateUsername(profile.RealPageId, profile.userLogin.LoginName);
                if (!isValidUsername)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.UpdateUser.2";
                    errorStatus.ErrorMsg = "Update User: User already exists";
                    output.Status = errorStatus;
                    return Ok(output);
                }

                IManageOrganization manageOrganization = new ManageOrganization(userClaim);
                bool isValidOrganization = manageOrganization.ValidateOrganization(userClaim.OrganizationMasterId, userClaim.UserRealPageGuid, profile.Persona[0].Organization.RealPageId);
                if (!isValidOrganization)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.UpdateUser.12";
                    errorStatus.ErrorMsg = "Update User: User exists in a different organization.";
                    output.Status = errorStatus;
                    return Ok(output);
                }

                IManageUser manageUser = new ManageUser(userClaim);
                string invalidProducts = "";
                List<int> invalidProductList = new List<int>();
                foreach (var product in profile.productBatch)
                {
                    if (!manageUser.CheckProductRight(product))
                    {
                        invalidProductList.Add(product.ProductId);
                    }
                }

                if (invalidProductList.Count > 0)
                {
                    var manageProduct = new ManageProduct(userClaim);
                    IList<ProductUI> productList = manageProduct.GetProducts(profile.Persona[0].Organization.RealPageId, 0, true);
                    foreach (int productId in invalidProductList)
                    {
                        if (!string.IsNullOrWhiteSpace(invalidProducts))
                        {
                            invalidProducts += ", ";
                        }

                        if (productList.Any(p => p.ProductId == productId))
                        {
                            invalidProducts += productList.FirstOrDefault(p => p.ProductId == productId).ProductName;
                        }
                        else
                        {
                            invalidProducts += ((ProductEnum)productId).ToString();
                        }
                    }

                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.UpdateUser.15";
                    errorStatus.ErrorMsg = $"Update User: you do not have access to edit {invalidProducts} product access";
                    output.Status = errorStatus;
                    return Ok(output);
                }

                //Pass the userRealPageGuid: User unique enterpriseId
                repositoryResponse = manageUser.UpdateUser(userClaim.UserRealPageGuid, profile);
                if ((repositoryResponse.Id == 0) && (!string.IsNullOrWhiteSpace(repositoryResponse.ErrorMessage)))
                {
                    output.obj = profile;
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.UpdateUser.9";
                    errorStatus.ErrorMsg = repositoryResponse.ErrorMessage;
                    output.Status = errorStatus;
                    return Ok(output);
                }

                output.Status = errorStatus;
                return Ok(output);
            });
        }

        /// <summary>
        /// Validate New User
        /// </summary>
        /// <param name="enterpriseUserName">Enterprise UserName</param>
        /// <param name="newUserRegistrationToken">new User Registration Token</param>
        /// <returns>ValidateUserResponse object</returns>
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("user/validate")]
        [AllowAnonymous]
        public async Task<ValidateUserResponse> Validate(string enterpriseUserName, string newUserRegistrationToken)
        {
            return await Task.Run(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();

                if (string.IsNullOrEmpty(enterpriseUserName.Trim()))
                {
                    throw new ArgumentException("Invalid parameter: enterpriseUserName");
                }

                if (string.IsNullOrEmpty(newUserRegistrationToken.Trim()))
                {
                    throw new ArgumentException("Invalid parameter: newUserRegistrationToken");
                }

                var manageUser = new ManageUser(userClaim);
                return manageUser.ValidateUser(enterpriseUserName.Trim(), newUserRegistrationToken);
            });
        }

        /// <summary>
        /// Validate registration verification token is associated with user name
        /// </summary>
        /// <param name="enterpriseUserName">Enterprise UserName</param>
        /// <param name="verificationToken">verification Token</param>
        /// <returns>ValidateUserResponse object</returns>
        [ProducesResponseType(typeof(ValidateUserResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("user/validate-token")]
        [AllowAnonymous]
        public async Task<ValidateUserResponse> ValidateToken(string enterpriseUserName, string verificationToken)
        {
            return await Task.Run(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();

                if (string.IsNullOrEmpty(enterpriseUserName.Trim()) || string.IsNullOrEmpty(verificationToken.Trim()))
                {
                    throw new ArgumentException("Invalid parameters");
                }

                var manageUser = new ManageUser(userClaim);
                return manageUser.ValidateRegistrationVerificationToken(enterpriseUserName.Trim(), verificationToken);
            });
        }

        /// <summary>
        /// Set Starter Profile
        /// </summary>
        /// <param name="starterProfile">StarterProfile object</param>
        /// <returns>SetStarterProfile object</returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPost("user/setstarterprofile")]
        [AllowAnonymous]
        public async Task<SetStarterProfile> SetStarterProfile(StarterProfile starterProfile)
        {
            return await Task.Run(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();

                if (starterProfile == null)
                {
                    throw new ArgumentException("Invalid parameter: starterProfile");
                }

                var manageUser = new ManageUser(userClaim);
                return manageUser.SetStarterProfile(starterProfile);
            });
        }

        /// <summary>
        /// Create a New User and Send Email Notification
        /// </summary>
        /// <param name="newProfile">Profile of the New User</param>
        /// <returns>CreateUser Response object</returns>
        /// <remarks>Next step of this process is to call https://landing.local/new-user/#/validate/{userToken}/{enterpriseUsername}
        /// <para>User Types (MVP):</para>
        /// <para>401 - User</para>
        /// <para>402 - SuperUser</para>
        /// <para>404 - User (No Email)</para>
        /// </remarks>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPost("user")]
        [AuthorizeRight("createuser", "cloneuser")]
        [AuthorizeScope("companyfunctions", "rplandingapi", "migrationapi")]
        public async Task<CreateUserResponse<IErrorData>> CreateUser([FromBody] ProfileDetail newProfile)
        {
            return await Task.Run(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                CreateUserResponse<IErrorData> response = new CreateUserResponse<IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();

                IList<Persona> personaList = new List<Persona>();
                Persona persona = new Persona();

                DateTime utcNow = DateTime.UtcNow;
                DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();

                ManageRoleType roleTypes = new ManageRoleType();
                // use the organization id of the person creating the user
                IList<RoleType> userRoles = roleTypes.GetRoleType("User Role", userClaim.OrganizationPartyId, userClaim.OrganizationMasterId, loginName: null);
                if (userRoles == null)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.CreateUser.23";
                    errorStatus.ErrorMsg = "User roles are missing.";
                    response.Status = errorStatus;
                    response.UserStatus = errorStatus.ErrorMsg;
                    return response;
                }

                var superUser = userRoles.SingleOrDefault<RoleType>(p => p.Name.Equals("SuperUser", StringComparison.OrdinalIgnoreCase));
                var user = userRoles.SingleOrDefault<RoleType>(p => p.Name.Equals("User", StringComparison.OrdinalIgnoreCase));
                var userNoEmail = userRoles.SingleOrDefault<RoleType>(p => p.Name.Equals("User (No Email)", StringComparison.OrdinalIgnoreCase));
                var rpEmployee = userRoles.SingleOrDefault<RoleType>(p => p.Name.Equals("RealPage Employee", StringComparison.OrdinalIgnoreCase));
                var rpExternalUser = userRoles.SingleOrDefault<RoleType>(p => p.Name.Equals("External User", StringComparison.OrdinalIgnoreCase));

                if (newProfile == null)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.CreateUser.24";
                    errorStatus.ErrorMsg = "Profile is required.";
                    response.Status = errorStatus;
                    response.UserStatus = errorStatus.ErrorMsg;
                    return response;
                }
                else if (newProfile.Persona == null)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.CreateUser.25";
                    errorStatus.ErrorMsg = "Persona is required.";
                    response.Status = errorStatus;
                    response.UserStatus = errorStatus.ErrorMsg;
                    return response;
                }
                else
                {
                    personaList = newProfile.Persona;
                }

                if (newProfile.IsFirstNameNullOrWhiteSpace)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.CreateUser.26";
                    errorStatus.ErrorMsg = "First name is required.";
                    response.UserStatus = errorStatus.ErrorMsg;
                    response.Status = errorStatus;
                }
                else if (newProfile.IsLastNameNullOrWhiteSpace)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.CreateUser.27";
                    errorStatus.ErrorMsg = "Last name is required.";
                    response.UserStatus = errorStatus.ErrorMsg;
                    response.Status = errorStatus;
                }
                else if (newProfile.userLogin.IsLoginNameNullOrWhiteSpace)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.CreateUser.28";
                    errorStatus.ErrorMsg = "Username is required.";
                    response.UserStatus = errorStatus.ErrorMsg;
                    response.Status = errorStatus;
                }
                else if (((user != null) && (newProfile.UserTypeId != (int)user.PartyRoleTypeId)) &&
                         ((userNoEmail != null) && (newProfile.UserTypeId != (int)userNoEmail.PartyRoleTypeId)) &&
                         ((superUser != null) && (newProfile.UserTypeId != (int)superUser.PartyRoleTypeId)) &&
                         ((rpEmployee != null) && (newProfile.UserTypeId != (int)rpEmployee.PartyRoleTypeId)) &&
                         ((rpExternalUser != null) && (newProfile.UserTypeId != (int)rpExternalUser.PartyRoleTypeId))
                )
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.CreateUser.29";
                    errorStatus.ErrorMsg = "Invalid user type!";
                    response.UserStatus = errorStatus.ErrorMsg;
                    response.Status = errorStatus;
                }
                else if (!String.IsNullOrWhiteSpace(newProfile.NotificationEmail) && !EmailFormatValidation.IsValidEmail(newProfile.NotificationEmail))
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.CreateUser.30";
                    errorStatus.ErrorMsg = "Notification email is not a valid email address.";
                    response.UserStatus = errorStatus.ErrorMsg;
                    response.Status = errorStatus;
                }
                else if (newProfile.UserTypeId == (int)UserRoleType.User &&
                         !EmailFormatValidation.IsValidEmail(newProfile.userLogin.LoginName))
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.CreateUser.31";
                    errorStatus.ErrorMsg = "Username is not a valid email address.";
                    response.UserStatus = errorStatus.ErrorMsg;
                    response.Status = errorStatus;
                }
                else if (newProfile.userLogin.FromDate.HasValue == false)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.CreateUser.33";
                    errorStatus.ErrorMsg = "Effective date is required.";
                    response.UserStatus = errorStatus.ErrorMsg;
                    response.Status = errorStatus;
                }
                else if (userClaim.OrganizationRealPageGuid == DefaultUserClaim.ExternalCompanyRealPageId)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.CreateUser.36";
                    errorStatus.ErrorMsg = "Cannot create new user in External User company.";
                    response.UserStatus = errorStatus.ErrorMsg;
                    response.Status = errorStatus;
                }

                IManagePersona managePersona = new ManagePersona(userClaim);

                if (newProfile.Persona.Count == 0)
                {
                    //Default Persona
                    IList<PersonaEnvironment> personaEnvironment = managePersona.GetPersonaEnvironmentType();
                    var personaEnv = personaEnvironment.SingleOrDefault<PersonaEnvironment>(p => p.Name == "Production");
                    if (personaEnv == null)
                    {
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "User.CreateUser.32";
                        errorStatus.ErrorMsg = "Persona environment is missing.";
                        response.UserStatus = errorStatus.ErrorMsg;
                        response.Status = errorStatus;
                    }
                    else
                    {
                        persona.Name = newProfile.UserTypeId == (int)UserRoleType.SuperUser ? "System Administrator" : "Primary";
                        persona.PersonaEnvironmentTypeId = (int)personaEnv.PersonaEnvironmentTypeId;
                        persona.FromDate = utcNow;
                        persona.ThruDate = null;
                        personaList.Add(persona);

                        newProfile.Persona = personaList;
                    }
                }

                if (newProfile.organization.Count == 0)
                {
                    //Active Persona is linked to one organization
                    persona = managePersona.GetFirstAvailablePersonaByCompany(userClaim.UserRealPageGuid, userClaim.OrganizationPartyId);
                    newProfile.organization.Add(persona.Organization);
                }

                ManageUser manageUser = new ManageUser(userClaim);
                string invalidProducts = "";
                List<int> invalidProductList = new List<int>();
                foreach (var product in newProfile.productBatch)
                {
                    if (!manageUser.CheckProductRight(product))
                    {
                        invalidProductList.Add(product.ProductId);
                    }
                }

                if (invalidProductList.Count > 0)
                {
                    var manageProduct = new ManageProduct(userClaim);
                    IList<ProductUI> productList = manageProduct.GetProducts(newProfile.organization[0].RealPageId, 0, true);
                    foreach (int productId in invalidProductList)
                    {
                        if (!string.IsNullOrWhiteSpace(invalidProducts))
                        {
                            invalidProducts += ", ";
                        }

                        if (productList.Any(p => p.ProductId == productId))
                        {
                            invalidProducts += productList.FirstOrDefault(p => p.ProductId == productId).ProductName;
                        }
                        else
                        {
                            invalidProducts += ((ProductEnum)productId).ToString();
                        }
                    }

                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.CreateUser.41";
                    errorStatus.ErrorMsg = $"Update User: you do not have access to edit {invalidProducts} product access";
                    response.UserStatus = errorStatus.ErrorMsg;
                    response.Status = errorStatus;
                    return response;
                }

                // PBI 78569 [All Browsers] - Able to create the user with Disabled status when today's date as
                // effective and expiration date even after getting 'User Creation Failed' error
                // -- Should not allow user to enter same date for both Effective and Expiration
                DateTime fromDate = newProfile.userLogin.FromDate.HasValue ? newProfile.userLogin.FromDate.Value : DateTime.UtcNow;

                if (newProfile.userLogin.ThruDate.HasValue)
                {
                    TimeZone ctz = TimeZone.CurrentTimeZone;
                    // get local time from utc
                    DateTime clientLocal = ctz.ToLocalTime(fromDate);
                    if (clientLocal.Date.Equals(newProfile.userLogin.ThruDate.Value.Date))
                    {
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "User.CreateUser.34";
                        errorStatus.ErrorMsg = "Effective & Expiration date shouldn't be same.";
                        response.Status = errorStatus;
                        response.UserStatus = errorStatus.ErrorMsg;
                        return response;
                    }
                }

                if (errorStatus.Success == false)
                {
                    response.UserStatus = response.Status.ErrorMsg;
                    return response;
                }

                if (errorStatus.Success == true)
                {
                    ProductBatch resPortal = newProfile.productBatch.FirstOrDefault<ProductBatch>((Func<ProductBatch, bool>)(p => p.ProductId == (int)ProductEnum.ResidentPortal));
                    if (resPortal != null)
                    {
                        //verify resident portal user has Enterprise or staff admin role then only allow to create user
                        IManageProductResidentPortal manageResidentPortal = new ManageProductResidentPortal(userClaim);
                        bool hasAccess = manageResidentPortal.ValidateCreateUserAccess(persona);
                        if (!hasAccess)
                        {
                            errorStatus.Success = false;
                            errorStatus.ErrorCode = "User.CreateUser.35";
                            errorStatus.ErrorMsg = "Validate Resident Portal User Access: You do not have the permissions to create user.";
                            response.Status = errorStatus;
                            response.UserStatus = errorStatus.ErrorMsg;
                            return response;
                        }
                    }

                    return manageUser.CreateUser(newProfile, personaList);
                }
                else
                {
                    return response;
                }
            });
        }

        /// <summary>
        /// Update profile of a new user
        /// </summary>
        /// <param name="newProfile">Profile of the New User</param>
        /// <param name="companyJobTitle">Job Title of the New User</param>
        /// <param name="userLogin">User Login of the New User</param>
        /// <param name="activityToken">Activity Token</param>
        /// <returns></returns>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPost("newuser/profile")]
        [AllowAnonymous]
        public async Task<RepositoryResponse> UpdateNewUser([FromBody] Profile newProfile, string companyJobTitle, string userLogin, string activityToken)
        {
            return await Task.Run(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                RepositoryResponse response = new RepositoryResponse();
                if (String.IsNullOrWhiteSpace(userLogin))
                {
                    response.ErrorMessage = "Userlogin is required.";
                }
                else if (String.IsNullOrWhiteSpace(companyJobTitle))
                {
                    response.ErrorMessage = "Company Job Title is required.";
                }
                else if (newProfile == null)
                {
                    response.ErrorMessage = "Profile is required.";
                }
                else if (newProfile.PartyRole.RoleTypeId == 0)
                {
                    response.ErrorMessage = "PartyRoleTypeId is invalid.";
                }
                else if (newProfile.TelecommunicationNumber.Count == 0)
                {
                    response.ErrorMessage = "Telecommunication number is required.";
                }
                else if (String.IsNullOrWhiteSpace(activityToken.Trim()))
                {
                    response.ErrorMessage = "Activity Token is required.";
                }

                ManageUser manageUser = new ManageUser(userClaim);
                return manageUser.UpdateNewUser(userLogin, newProfile, newProfile.PartyRole.RoleTypeId, companyJobTitle, activityToken);
            });
        }

        /// <summary>
        /// List User Custom Fields
        /// </summary>
        /// <returns>A list of user's customfields</returns>
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ICustomFieldValue), (int)HttpStatusCode.OK)]
        [HttpGet("customfields")]
        public async Task<IActionResult> UserCustomFields(long? userLoginPersonaId = null)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                IManageCustomFields manageCustomFields = new ManageCustomFields(userClaim);

                IList<CustomFieldValue> customFieldValueList = manageCustomFields.GetCustomFieldsValues(organizationPartyId: userClaim.OrganizationPartyId, userLoginPersonaId: userLoginPersonaId, enabled: true);

                ListResponse response = new ListResponse()
                {
                    Records = customFieldValueList.Cast<object>().ToList(),
                    TotalRows = customFieldValueList.Count(),
                    RowsPerPage = customFieldValueList.Count(),
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
                return Ok(response);
            });
        }

        /// <summary>
        /// Gets the list of rights for the current authenticated user
        /// </summary>
        /// <returns>A list of the users rights</returns>
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [HttpGet("user/rights/current")]
        [AuthorizeScope("userinfoapi", "landingapi")]
        public async Task<IActionResult> GetCurrentUserRights()
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                List<string> userRights = userClaim.Rights;

                return Ok(userRights);
            });
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Used to return the product list of the user to the RAUL UI component
        /// </summary>
        /// <param name="products"></param>
        /// <param name="productIconSettings"></param>
        /// <returns></returns>
        private List<UserProducts> ConvertDashboardProductsToRAUL(IList<PersonaProductUserDetails> products, IList<ProductInternalSettingByType> productIconSettings)
        {
            List<UserProducts> productList = new List<UserProducts>();
            foreach (PersonaProductUserDetails prodDetail in products)
            {
                if (!string.IsNullOrEmpty(prodDetail.ProductUrl))
                {
                    UserProducts up = new UserProducts()
                    {
                        Id = prodDetail.ProductId,
                        Name = prodDetail.ProductName,
                        Description = prodDetail.ProductDescription,
                        Url = prodDetail.ProductUrl.ToUpper().Contains("HTTP") ? prodDetail.ProductUrl : ConfigReader.GetLandingUri + prodDetail.ProductUrl,
                        Label = productIconSettings.FirstOrDefault(f => f.ProductId == prodDetail.ProductId)?.Value,
                        FamilyId = prodDetail?.FamilyId,
                        FamilyName = prodDetail.Family,
                        IsFavorite = prodDetail.IsFavorite,
                        IsNewTab = prodDetail.IsNewTab,
                        IsResource = prodDetail.IsResource,
                        Status = prodDetail.ProductStatus,
                        ProductCode = ((ProductEnum)prodDetail.ProductId).ToEnumDescription()
                    };
                    productList.Add(up);
                }
            }

            return productList.OrderBy(p => p.FamilyName).ThenBy(p => p.Name).ToList();
        }

        /// <summary>
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            try
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                string correlationId = "";
                if (userClaim != null)
                {
                    correlationId = (userClaim.CorrelationId != Guid.Empty) ? userClaim.CorrelationId.ToString() : "";
                }
                var logger = Log.Logger;
                if (logData?.Keys != null)
                {
                    logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
                }
                logger = logger.ForContext("ProductModule", this.GetType());
                logger = logger.ForContext("CorrelationId", correlationId);
                logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
            }
            catch
            {
                /*ignored*/
            }
        }
        #endregion
    }

    /// <summary>
    /// Output result for newly created user
    /// </summary>
    public class UserProductOutputResult
    {
        /// <summary>
        /// User Product list
        /// </summary>
        public List<UserProducts> Products { get; set; }

        /// <summary>
        /// User Resource list
        /// </summary>
        public List<UserProducts> Resources { get; set; }
    }

    /// <summary>
    /// Details about the products assigned to the user
    /// </summary>
    public class UserProducts
    {
        /// <summary>
        /// Product id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the product
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The url to the product for login
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The product description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The icon for the product
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The Family id the product belongs to
        /// </summary>
        public int? FamilyId { get; set; }

        /// <summary>
        /// The Family the product belongs to
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Does the product open in a new window
        /// </summary>
        public bool IsNewTab { get; set; }

        /// <summary>
        /// Has the product been favourited
        /// </summary>
        public bool IsFavorite { get; set; }

        /// <summary>
        /// Is the product a resource
        /// </summary>
        public bool IsResource { get; set; }

        /// <summary>
        /// The status of the product, 7 errored, 8 success, 10 deleted
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Books product code
        /// </summary>
        public string ProductCode { get; set; }
    }
}
