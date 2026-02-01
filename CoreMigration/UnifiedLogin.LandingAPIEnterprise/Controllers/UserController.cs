using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog.Events;
using System.Net;
using UnifiedLogin.BusinessLogic.Attributes;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.User.Models;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPIEnterprise.Services;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.ResponseObject;
using ValidationException = UnifiedLogin.LandingAPIEnterprise.Services.ValidationException;

namespace UnifiedLogin.LandingAPIEnterprise.Controllers
{
    /// <summary>
    /// API Controller for user management operations
    /// </summary>
   [ApiController]
   [Route("")]
    public class UserController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        private readonly IUserQueryService _userQueryService;
        private readonly IUserValidationService _validationService;
        private readonly ILoggingService _loggingService;
        private readonly IClientAuthenticationService _clientAuthService;
        private readonly IProductRepository _productRepository;
        private readonly DefaultUserClaim _userClaims;

        public UserController(
            IUserManagementService userManagementService,
            IUserQueryService userQueryService,
            IUserValidationService validationService,
            ILoggingService loggingService,
            IClientAuthenticationService clientAuthService,
            IProductRepository productRepository,
            DefaultUserClaim userClaims)
        {
            _userManagementService = userManagementService ?? throw new ArgumentNullException(nameof(userManagementService));
            _userQueryService = userQueryService ?? throw new ArgumentNullException(nameof(userQueryService));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _clientAuthService = clientAuthService ?? throw new ArgumentNullException(nameof(clientAuthService));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _userClaims = userClaims ?? throw new ArgumentNullException(nameof(userClaims));
        }

        /// <summary>
        /// Create a user in RealPage Unified platform and assign product(s).
        /// </summary>
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(ObjectResponse), (int)HttpStatusCode.Created)]
        [Route("user")]
        [HttpPost]
        public async Task<IActionResult> CreateUser(UserProductDetailsDto userProductDetailsDto, Guid? upfmId = null)
        {
            try
            {
                // Handle client credential authentication
                var authError = await _clientAuthService.AuthenticateClientAsync(upfmId, User, _userClaims);
                if (authError != null)
                {
                    return BadRequest(authError);
                }

                // Validate SuperUser creation BEFORE other validations (as in original code)
                var superUserValidationError = _validationService.ValidateSuperUserCreation(userProductDetailsDto, _userClaims);
                if (superUserValidationError != null && superUserValidationError.Errors.Any())
                {
                    return BadRequest(superUserValidationError);
                }

                // Validate input
                var validationErrors = _validationService.ValidateUserProductDetails(userProductDetailsDto, _userClaims);
                if (validationErrors.Errors.Any())
                {
                    return BadRequest(validationErrors);
                }

                // Create user
                var response = await _userManagementService.CreateUserAsync(userProductDetailsDto, _userClaims);

                return CreatedAtAction(
                    nameof(GetUser),
                    new { unityRealPageUserId = response.Data },
                    response);
            }
            catch (ValidationException ex)
            {
                return BadRequest(CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return HandleException(ex, "CreateUser",
                    $"Error while creating new user. Organization: {_userClaims.OrganizationName}, LoginName: {userProductDetailsDto?.UserProfileDetails.LoginName}");
            }
        }

        /// <summary>
        /// Update the user in RealPage Unified platform and product(s).
        /// </summary>
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(ObjectResponse), (int)HttpStatusCode.OK)]
        [Route("user")]
        [HttpPut]
        public async Task<IActionResult> UpdateUser(UserProductDetailsDto userProductDetailsDto, Guid? upfmId = null)
        {
            try
            {
                // Handle client credential authentication
                var authError = await _clientAuthService.AuthenticateClientAsync(upfmId, User, _userClaims);
                if (authError != null)
                {
                    return BadRequest(authError);
                }

                // Validate realpage guid supplied
                if (userProductDetailsDto.UserProfileDetails.UnityRealPageUserId == Guid.Empty)
                {
                    return BadRequest(CreateErrorResponse("UnityRealPageUserId not supplied."));
                }

                // Get existing user details to check if user type is changing to SuperUser
                int? existingUserTypeId = null;
                if (userProductDetailsDto.UserProfileDetails.UserType == UserTypeDto.SuperUser)
                {
                    var existingUserDetails = await _userQueryService.GetUserDetailsByIdAsync(
                        userProductDetailsDto.UserProfileDetails.UnityRealPageUserId);
                    existingUserTypeId = existingUserDetails?.UserRoleTypeId;
                }

                // Validate SuperUser promotion (if changing TO SuperUser)
                var superUserValidationError = _validationService.ValidateSuperUserUpdate(
                    userProductDetailsDto,
                    _userClaims,
                    existingUserTypeId);
                if (superUserValidationError != null && superUserValidationError.Errors.Any())
                {
                    return BadRequest(superUserValidationError);
                }

                // Validate input
                var validationErrors = _validationService.ValidateUserProductDetails(userProductDetailsDto, _userClaims);
                if (validationErrors.Errors.Any())
                {
                    return BadRequest(validationErrors);
                }

                // Update user
                var response = await _userManagementService.UpdateUserAsync(userProductDetailsDto, _userClaims);

                return Ok(response);
            }
            catch (ValidationException ex)
            {
                return BadRequest(CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return HandleException(ex, "UpdateUser",
                    $"Error while updating user. Organization: {_userClaims.OrganizationName}, LoginName: {userProductDetailsDto?.UserProfileDetails.LoginName}");
            }
        }

        /// <summary>
        /// Activate or deactivate a user.
        /// </summary>
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(ObjectResponse), (int)HttpStatusCode.OK)]
        [Route("user/status/{unityRealPageUserId}")]
        [HttpPost]
        public async Task<IActionResult> CreateUpdateUserStatus(Guid unityRealPageUserId, EntApiUserStatus userStatusToChange)
        {
            try
            {
                var response = await _userManagementService.ChangeUserStatusAsync(unityRealPageUserId, userStatusToChange, _userClaims);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(CreateErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return HandleException(ex, "CreateUpdateUserStatus",
                    $"Error while changing user status. Organization: {_userClaims.OrganizationName}, UserId: {unityRealPageUserId}");
            }
        }

        /// <summary>
        /// Get a list of users
        /// </summary>
        [ProducesResponseType(typeof(PagedResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UsersDataDto), (int)HttpStatusCode.OK)]
        // [SwaggerResponseExamples(typeof(UsersDataDto), typeof(EnterpriseGetUserExample))]
        [Route("user")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public async Task<IActionResult> GetUser(
            Guid? upfmId = null,
            Guid? unityRealPageUserId = null,
            string name = null,
            int rowsPerPage = 1,
            int pageNumber = 1,
            string userStatus = null)
        {
            try
            {
                // Handle client credential authentication
                var authError = await _clientAuthService.AuthenticateClientAsync(upfmId, User, _userClaims);
                if (authError != null)
                {
                    return BadRequest(CreatePagedErrorResponse(authError, pageNumber, rowsPerPage));
                }

                // Validate pagination parameters
                if (rowsPerPage <= 0)
                {
                    return BadRequest(CreatePagedErrorResponse("rowsPerPage must be 1 or greater.", pageNumber, rowsPerPage));
                }

                if (pageNumber <= 0)
                {
                    return BadRequest(CreatePagedErrorResponse("pageNumber must be 1 or greater.", pageNumber, rowsPerPage));
                }

                // Parse status
                int statusTypeId = 0;
                if (!string.IsNullOrEmpty(userStatus) && Enum.TryParse<UserUiStatusType>(userStatus, true, out var parsedStatus))
                {
                    statusTypeId = (int)parsedStatus;
                }

                var response = await _userQueryService.GetUsersAsync(
                    _userClaims.OrganizationPartyId,
                    statusTypeId,
                    unityRealPageUserId,
                    name,
                    rowsPerPage,
                    pageNumber);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetUser",
                    $"Error while Get/List user(s). Organization: {_userClaims.OrganizationName}");
            }
        }

        /// <summary>
        /// Get User role and asset details
        /// </summary>
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UserRoleAssetDto), (int)HttpStatusCode.OK)]
        // [SwaggerResponseExamples(typeof(UserRoleAssetDto), typeof(EnterpriseGetUserRoleAssetExample))]
        [Route("user/{realPageId}/product/{productCode}")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public async Task<IActionResult> GetUserRoleAsset(Guid realPageId, string productCode)
        {
            try
            {
                var response = await _userQueryService.GetUserRoleAssetAsync(
                    realPageId,
                    productCode,
                    _userClaims.OrganizationPartyId,
                    _userClaims);

                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetUserRoleAsset",
                    $"Error retrieving user role and asset details. UserId: {realPageId}, ProductCode: {productCode}");
            }
        }

        /// <summary>
        /// Get a specific users product detail
        /// </summary>
        [ProducesResponseType(typeof(ObjectOutput<IProfileDetail, IErrorData>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UserProductOutputResult), (int)HttpStatusCode.OK)]
        // [SwaggerResponseExamples(typeof(UserProducts), typeof(GetUserProductsExample))]
        [Route("user/{realPageId}/products")]
        [AuthorizeScope("userinfoapi")]
        [HttpGet]
        public async Task<IActionResult> GetUserProducts(Guid realPageId)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized();
                }

                if (realPageId == Guid.Empty)
                {
                    return BadRequest("Invalid parameter realPageId");
                }

                var response = await _userQueryService.GetUserProductDetailsAsync(
                    realPageId,
                    _userClaims.OrganizationPartyId,
                    _userClaims);

                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return Ok(new { Status = new { Success = false, ErrorCode = "User.GetUserProducts.2", ErrorMsg = "Get User Products: No data." } });
            }
            catch (UnauthorizedAccessException)
            {
                return Ok(new { Status = new { Success = false, ErrorCode = "User.GetProfile.3", ErrorMsg = "Get User Profile: User exists in a different organization." } });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetUserProducts",
                    $"Error retrieving user products. UserId: {realPageId}");
            }
        }

        /// <summary>
        /// Get the products for the given personaid 
        /// </summary>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UserProductOutputResultv2), (int)HttpStatusCode.OK)]
        // [SwaggerResponseExamples(typeof(UserProducts), typeof(GetUserProductsExamplev2))]
        [Route("user/products")]
        [AuthorizeScope("userinfoapi,internalapi")]
        [HttpGet]
        public async Task<IActionResult> GetUserProductsByPersonaId(long? personaId = 0, bool withStatus = false)
        {
            try
            {
                var response = await _userQueryService.GetUserProductsByPersonaIdAsync(personaId, withStatus, _userClaims);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetUserProductsByPersonaId",
                    $"Error retrieving user products by persona. PersonaId: {personaId}");
            }
        }

        /// <summary>
        /// Get the user details for the OmniBar
        /// </summary>
        [ProducesResponseType(typeof(ObjectOutput<IProfileDetail, IErrorData>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UserProductOutputResultv2), (int)HttpStatusCode.OK)]
        // [SwaggerResponseExamples(typeof(UserProducts), typeof(GetUserProductsExample))]
        [Route("user/omnibar")]
        [AuthorizeScope("userinfoapi")]
        [HttpGet]
        public async Task<IActionResult> GetOmnibarInfo()
        {
            //return await GetUserProductsByPersonaId();
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized();
                }

                var response = await _userQueryService.GetUserOmniBarProductDetailsAsync(
                    _userClaims.UserRealPageGuid,
                    _userClaims.OrganizationPartyId,
                    _userClaims);

                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return Ok(new { Status = new { Success = false, ErrorCode = "User.GetOmnibarInfo.2", ErrorMsg = "Get User Omnibar Products: No data." } });
            }
            catch (UnauthorizedAccessException)
            {
                return Ok(new { Status = new { Success = false, ErrorCode = "User.GetOmnibarInfo.3", ErrorMsg = "Get User Profile: User exists in a different organization." } });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetOmnibarInfo",
                    $"Error retrieving user products. UserId: {_userClaims.UserRealPageGuid}");
            }
        }

        /// <summary>
        /// Get Users Product Detail Login By PersonaId
        /// </summary>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UserProductDetailLogin), (int)HttpStatusCode.OK)]
        // [SwaggerResponseExamples(typeof(UserProductDetailLogin), typeof(GetUserProductsDetailsLoginExample))]
        [Route("user/products/details/login")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public async Task<IActionResult> GetUserProductsDetailsLoginByPersonaId()
        {
            try
            {
                var response = await _userQueryService.GetUserProductDetailsLoginByPersonaIdAsync(_userClaims.PersonaId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetUserProductsDetailsLoginByPersonaId", "Error");
            }
        }

        /// <summary>
        /// Get the user (Regular and External) with the product login details and company by LoginName
        /// </summary>
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UserProductDetailLogin), (int)HttpStatusCode.OK)]
        // [SwaggerResponseExamples(typeof(UserProductDetailLogin), typeof(GetUserProductsDetailsLoginCompanyExample))]
        [Route("user/products/details/login/company")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public async Task<IActionResult> GetUserProductsDetailsLoginByLoginName()
        {
            try
            {
                var response = await _userQueryService.GetUserProductDetailsLoginByLoginNameAsync(_userClaims.LoginName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetUserProductsDetailsLoginByLoginName", "Error");
            }
        }

        /// <summary>
        /// Gets the list of rights for the current authenticated user
        /// </summary>
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(List<string>), (int)HttpStatusCode.OK)]
        [Route("user/rights/current")]
        [HttpGet]
        [AuthorizeScope("userinfoapi,landingapi")]
        public IActionResult GetCurrentUserRights()
        {
            return Ok(_userClaims.Rights);
        }

        /// <summary>
        /// Get custom fields for a user or organization
        /// </summary>
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [Route("customfieldsmaster")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public async Task<IActionResult> GetUserCustomFields(long? userLoginPersonaId = null)
        {
            try
            {
                var response = await _userQueryService.GetUserCustomFieldsAsync(
                    _userClaims.OrganizationPartyId,
                    userLoginPersonaId,
                    _userClaims);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetUserCustomFields",
                    $"Error retrieving custom fields. Organization: {_userClaims.OrganizationName}, PersonaId: {userLoginPersonaId}");
            }
        }

        /// <summary>
        /// Change the active company/persona for the current user
        /// </summary>
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [Route("user/persona/{personaId}/company")]
        [AuthorizeScope("userinfoapi")]
        [HttpPost]
        public async Task<IActionResult> ChangeCompany(long personaId)
        {
            try
            {
                if (personaId <= 0)
                {
                    return BadRequest(CreateErrorResponse("Invalid personaId. Must be greater than 0."));
                }

                var result = await _userManagementService.ChangeCompanyAsync(personaId, _userClaims);

                return result.IsSuccess ? Accepted() : BadRequest(CreateErrorResponse(result.ErrorMessage));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return HandleException(ex, "ChangeCompany",
                    $"Error changing company. PersonaId: {personaId}, CurrentPersona: {_userClaims.PersonaId}");
            }
        }

        /// <summary>
        /// Get the list of employee personas for the current user
        /// </summary>
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(ObjectListOutput<PersonaCompanyDetails, IErrorData>), (int)HttpStatusCode.OK)]
        [Route("user/employeepersonas")]
        [AuthorizeScope("userinfoapi")]
        [HttpGet]
        public async Task<IActionResult> GetEmployeePersonasList()
        {
            try
            {
                var result = await _userQueryService.GetEmployeePersonasListAsync(_userClaims.UserId, _userClaims.OrganizationPartyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetEmployeePersonasList",
                    $"Error retrieving employee personas. UserId: {_userClaims.UserId}, OrgPartyId: {_userClaims.OrganizationPartyId}");
            }
        }

        /// <summary>
        /// Get the list of active personas grouped by company for the current user
        /// </summary>
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(ObjectListOutput<PersonaCompany, IErrorData>), (int)HttpStatusCode.OK)]
        [Route("user/personas")]
        [AuthorizeScope("userinfoapi")]
        [HttpGet]
        public async Task<IActionResult> GetPersonasList()
        {
            try
            {
                var result = await _userQueryService.GetPersonasListAsync(_userClaims.UserRealPageGuid);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetPersonasList",
                    $"Error retrieving personas list. UserId: {_userClaims.UserRealPageGuid}");
            }
        }

        /// <summary>
        /// Delete SAML user product information and status
        /// </summary>
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [Route("user/productuser/details")]
        [AuthorizeScope("internalapi")]
        [HttpDelete]
        public async Task<IActionResult> DeleteSamlUserProductInfoAndStatus([FromBody] ProductUserAccountDetails productUser)
        {
            try
            {
                if (productUser == null)
                {
                    return BadRequest(CreateErrorResponse("productUser is null."));
                }

                if (productUser.ProductId <= 0)
                {
                    return BadRequest(CreateErrorResponse("ProductId is required and must be greater than 0."));
                }

                var result = await _userManagementService.DeleteSamlUserProductInfoAndStatusAsync(productUser);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return HandleException(ex, "DeleteSamlUserProductInfoAndStatus",
                    $"Error deleting SAML user product. ProductId: {productUser?.ProductId}");
            }
        }

        /// <summary>
        /// Update product user account details
        /// </summary>
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [Route("user/productuser/details")]
        [AuthorizeScope("internalapi")]
        [HttpPut]
        public async Task<IActionResult> UpdateProductUserAccountDetails([FromBody] ProductUserAccountDetails productUser)
        {
            try
            {
                if (productUser == null)
                {
                    return BadRequest(CreateErrorResponse("productUser is null."));
                }

                if (productUser.ProductId <= 0)
                {
                    return BadRequest(CreateErrorResponse("ProductId is required and must be greater than 0."));
                }

                var result = await _userManagementService.UpdateProductUserAccountDetailsAsync(productUser);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return HandleException(ex, "UpdateProductUserAccountDetails",
                    $"Error updating product user account. ProductId: {productUser?.ProductId}");
            }
        }

        /// <summary>
        /// Get SAML product attributes for a specific product
        /// </summary>
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(IList<SharedObjects.Saml.SamlProductAttributes>), (int)HttpStatusCode.OK)]
        [Route("user/productuser/attributes")]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetSamlProductAttributes(int productId)
        {
            try
            {
                if (productId <= 0)
                {
                    return BadRequest(CreateErrorResponse("ProductId must be greater than 0."));
                }

                var result = await _userQueryService.GetSamlProductAttributesAsync(productId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetSamlProductAttributes",
                    $"Error retrieving SAML product attributes. ProductId: {productId}");
            }
        }

        /// <summary>
        /// Get product users with role and asset details for a specific product
        /// </summary>
        [ProducesResponseType(typeof(PagedResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(PagedResponse), (int)HttpStatusCode.OK)]
        [Route("user/product/{productCode}")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public async Task<IActionResult> GetProductUsersWithRoleAsset(string productCode, int rowsPerPage = 1, int pageNumber = 1)
        {
            try
            {
                // Validate pagination parameters
                if (rowsPerPage <= 0)
                {
                    return BadRequest(CreatePagedErrorResponse("rowsPerPage must be 1 or greater.", pageNumber, rowsPerPage));
                }

                if (pageNumber <= 0)
                {
                    return BadRequest(CreatePagedErrorResponse("pageNumber must be 1 or greater.", pageNumber, rowsPerPage));
                }

                if (string.IsNullOrWhiteSpace(productCode))
                {
                    return BadRequest(CreatePagedErrorResponse("productCode is required.", pageNumber, rowsPerPage));
                }

                var response = await _userQueryService.GetProductUsersWithRoleAssetAsync(
                    productCode,
                    rowsPerPage,
                    pageNumber,
                    _userClaims.PersonaId,
                    _productRepository);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(CreatePagedErrorResponse(ex.Message, pageNumber, rowsPerPage));
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetProductUsersWithRoleAsset",
                    $"Error retrieving product users. ProductCode: {productCode}");
            }
        }

        /// <summary>
        /// Get product user properties for a specific product with optional client credential support
        /// </summary>
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(ListResponse), (int)HttpStatusCode.OK)]
        [Route("user/product/{productCode}/properties")]
        [AuthorizeScope("enterpriseapi")]
        [HttpGet]
        public async Task<IActionResult> GetProductUserProperties(
            string productCode,
            [FromQuery] RequestParameter dataFilter,
            Guid? upfmId = null,
            Guid? userRealPageId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productCode))
                {
                    return BadRequest(CreateErrorResponse("productCode is required."));
                }

                var result = await _userQueryService.GetProductUserPropertiesAsync(
                    productCode,
                    dataFilter,
                    upfmId,
                    userRealPageId,
                    User,
                    _userClaims,
                    _productRepository);

                // Check if result indicates forbidden access
                if (result.IsError && result.ErrorReason?.Contains("Forbidden") == true)
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(CreateErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GetProductUserProperties",
                    $"Error retrieving product user properties. ProductCode: {productCode}");
            }
        }

        #region Helper Methods

        private ErrorResponse CreateErrorResponse(string detail)
        {
            return new ErrorResponse
            {
                Errors = new List<Error>
                {
                    new Error
                    {
                        Title = "Error",
                        Source = "/user",
                        Detail = detail,
                        StatusCode = ""
                    }
                }
            };
        }

        private PagedResponse CreatePagedErrorResponse(string errorMessage, int pageNumber, int rowsPerPage)
        {
            return new PagedResponse
            {
                Data = new List<object>(),
                Meta = new Meta
                {
                    CurrentPage = pageNumber,
                    TotalRows = 0,
                    RowsPerPage = rowsPerPage
                },
                IsError = true,
                ErrorReason = errorMessage
            };
        }

        private PagedResponse CreatePagedErrorResponse(ErrorResponse errorResponse, int pageNumber, int rowsPerPage)
        {
            var firstError = errorResponse.Errors.FirstOrDefault();
            return CreatePagedErrorResponse(firstError?.Detail ?? "An error occurred", pageNumber, rowsPerPage);
        }

        private IActionResult HandleException(Exception ex, string actionName, string state)
        {
            if (_userClaims.CorrelationId == Guid.Empty)
            {
                _userClaims.CorrelationId = Guid.NewGuid();
            }

            _loggingService.WriteToLog(
                LogEventLevel.Error,
                "{ActionName} - {state}",
                exception: ex,
                messageProperties: new object[] { actionName, state },
                correlationId: _userClaims.CorrelationId);

            return StatusCode(
                (int)HttpStatusCode.InternalServerError,
                $"Internal system error. Please contact RealPage support with correlation Id - {_userClaims.CorrelationId}");
        }

        #endregion

        #region Example Classes (Keep existing swagger examples)
        // ... Keep all the existing example classes for Swagger documentation
        #endregion
    }
}

