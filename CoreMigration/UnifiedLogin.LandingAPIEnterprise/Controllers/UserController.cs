using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog.Events;
using System.Net;
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
        [Authorize(Policy = "enterpriseapi")]
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
        [Authorize(Policy = "enterpriseapi")]
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
        [Authorize(Policy = "userinfoapi")]
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
        [Authorize(Policy = "userinfoapi,internalapi")]
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
        [Authorize(Policy = "userinfoapi")]
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
        [Authorize(Policy = "enterpriseapi")]
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
        [Authorize(Policy = "enterpriseapi")]
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
        [Authorize(Policy = "userinfoapi,landingapi")]
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
        [Authorize(Policy = "enterpriseapi")]
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
        [Authorize(Policy = "userinfoapi")]
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
        [Authorize(Policy = "userinfoapi")]
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
        [Authorize(Policy = "userinfoapi")]
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
        [Authorize(Policy = "internalapi")]
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
        [Authorize(Policy = "internalapi")]
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
        [Authorize(Policy = "enterpriseapi")]
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
        [Authorize(Policy = "enterpriseapi")]
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

//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
//using RealPage.DataAccess.Dapper;
//using Serilog;
//using Serilog.Events;
//using Swashbuckle.AspNetCore.Annotations;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;
//using System.Net;
//using System.Security.Claims;
//using UnifiedLogin.BusinessLogic.Attributes;
//using UnifiedLogin.BusinessLogic.Logic;
//using UnifiedLogin.BusinessLogic.Logic.Enterprise.User;
//using UnifiedLogin.BusinessLogic.Logic.Enterprise.User.Models;
//using UnifiedLogin.BusinessLogic.Logic.Interfaces;
//using UnifiedLogin.BusinessLogic.Logic.Product;
//using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
//using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
//using UnifiedLogin.BusinessLogic.Logic.Security;
//using UnifiedLogin.BusinessLogic.Repository;
//using UnifiedLogin.BusinessLogic.Repository.Interfaces;
//using UnifiedLogin.BusinessLogic.Repository.Security;
//using UnifiedLogin.DataAccess;
//using UnifiedLogin.ServiceDefaults;
//using UnifiedLogin.SharedObjects.Attribute;
//using UnifiedLogin.SharedObjects.Base;
//using UnifiedLogin.SharedObjects.Constants;
//using UnifiedLogin.SharedObjects.Enterprise;
//using UnifiedLogin.SharedObjects.Enum;
//using UnifiedLogin.SharedObjects.Helper;
//using UnifiedLogin.SharedObjects.IdentityConfig;
//using UnifiedLogin.SharedObjects.Landing;
//using UnifiedLogin.SharedObjects.Landing.Security;
//using UnifiedLogin.SharedObjects.Product;
//using UnifiedLogin.SharedObjects.Product.OneSite;
//using UnifiedLogin.SharedObjects.Product.Ops;
//using UnifiedLogin.SharedObjects.Product.Rum;
//using UnifiedLogin.SharedObjects.ResponseObject;
//using UnifiedLogin.SharedObjects.Saml;
//using UnifiedLogin.SharedObjects.Swagger;
//using IRepository = UnifiedLogin.DataAccess.IRepository;

//namespace UnifiedLogin.LandingAPIEnterprise.Controllers
//{
//    /// <summary>
//    /// User Controller - Refactored to modern ASP.NET Core patterns
//    /// TODO: This controller should be split into:
//    /// - UserManagementController (CRUD operations)
//    /// - UserProductController (product assignments)
//    /// - UserProfileController (profile operations)
//    /// </summary>
//    [Authorize]
//    [ApiController]
//    [ApiVersion("1.0")]
//    [Route("v{version:apiVersion}/[controller]")]
//    public class UserController : ControllerBase
//    {
//        #region Private Fields
//        private readonly IManagePersona _managePersona;
//        private readonly IManagePerson _managePerson;
//        private readonly IManageProduct _manageProduct;
//        private readonly IManageOrganization _manageOrganization;
//        private readonly IManageUnifiedSettings _manageSettings;
//        private readonly IProductRepository _productRepository;
//        private readonly IUserRepository _userRepository;
//        private readonly IManageSecurity _manageSecurityLogic;
//        private readonly IManageProductPanel _manageProductPanel;
//        private readonly IIntegrationTypeFactory _integrationTypeFactory;
//        private readonly IManageUserLogin _userLoginLogic;
//        private readonly IManageProductUser _manageProductUser;
//        private readonly ISamlRepository _samlRepository;
//        private readonly IUserClaimsAccessor _userClaimsAccessor;
//        private readonly IManageCustomFields _manageCustomFields;
//        private readonly IManageProductOps _manageProductOps;
//        private readonly IManageBlueBook manageBlueBook;
//        private UserManagement _userManagement;
//        private IRepository _repository;
//        private IOneSiteProductService _oneSiteProductService;
//        private HttpMessageHandler _messageHandler;
//        private ManageUser _manageUser;
//        #endregion

//        #region Constructor
//        /// <summary>
//        /// Constructor with dependency injection for user controller.
//        /// Follows modern ASP.NET Core patterns for testable, maintainable code.
//        /// All dependencies are injected as interfaces for loose coupling and testability.
//        /// </summary>
//        public UserController(
//            IManagePersona managePersona,
//            IManagePerson managePerson,
//            IManageProduct manageProduct,
//            IManageOrganization manageOrganization,
//            IManageUnifiedSettings manageSettings,
//            IProductRepository productRepository,
//            IUserRepository userRepository,
//            IManageSecurity manageSecurity,
//            IManageProductPanel manageProductPanel,
//            IIntegrationTypeFactory integrationTypeFactory,
//            IManageUserLogin userLoginLogic,
//            IManageProductUser manageProductUser,
//            ISamlRepository samlRepository,
//            IUserClaimsAccessor userClaimsAccessor,
//            IManageCustomFields manageCustomFields,
//            IManageProductOps manageProductOps, 
//            UnifiedLogin.DataAccess.IRepository repository, 
//            HttpMessageHandler messageHandler,
//            SharedObjects.Landing.DefaultUserClaim userClaims, 
//            IOneSiteProductService oneSiteProductService)
//        {

//            _repository = repository;
//            _messageHandler = messageHandler;
//            _oneSiteProductService = oneSiteProductService;
//            var productInternalSettingRepository = new ProductInternalSettingRepository(repository);
//            var manageBlueBook = new ManageBlueBook(userClaims, repository, productInternalSettingRepository, messageHandler);
//            var personaRightRepository = new PersonaRightRepository(null);//TOD):pass Sqlconnection.
//            var manageUnifiedLogin = new ManageUnifiedLogin(repository, userClaims, messageHandler);
//            var manageProductOneSite = new ManageProductOneSite(repository, userClaims, messageHandler, oneSiteProductService);
//            _managePersona = new ManagePersona(repository, userClaims, messageHandler);
//            managePerson = new ManagePerson(repository);
//            _manageOrganization = new ManageOrganization(repository, userClaims, messageHandler);
//            _manageSettings = new ManageUnifiedSettings(repository, userClaims, messageHandler);
//            _manageProduct = new ManageProduct(repository, userClaims, messageHandler);
//            _manageProductPanel = new ManageProductPanel(userClaims, repository, manageBlueBook, messageHandler, manageProductOneSite);
//            _productRepository = new ProductRepository(repository, userClaims);
//            _userClaimsAccessor = userClaimsAccessor;
//            _userRepository = new UserRepository(repository, userClaims, messageHandler);
//            _manageSecurityLogic = new ManageSecurity(personaRightRepository, userClaimsAccessor);
//            _integrationTypeFactory = new IntegrationTypeFactory(_manageProduct, manageUnifiedLogin, manageProductOneSite, _productRepository, productInternalSettingRepository, userClaims);
//            _userManagement = new UserManagement(userClaims);
//            _manageUser = new ManageUser(repository, userClaims, messageHandler);
//            _userLoginLogic = new ManageUserLogin(repository, userClaims, messageHandler);

//            _manageProductUser = new ManageProductUser(repository, userClaims, messageHandler, oneSiteProductService);
//            _samlRepository = new SamlRepository(repository);
//        }
//        #endregion

//        #region API Endpoints - Example refactored methods

//        /// <summary>
//        /// Get User role and asset details
//        /// </summary>
//        [SwaggerResponse(400, Description = "Bad request")]
//        [SwaggerResponse(401, Description = "Unauthorized")]
//        [SwaggerResponse(404, Description = "Not Found")]
//        [SwaggerResponse(500, Description = "Internal Server Error")]
//        [SwaggerResponse(200, Description = "A list of User(s)", Type = typeof(UserRoleAssetDto))]
//        [SwaggerResponseExamples(typeof(UserRoleAssetDto), typeof(EnterpriseGetUserRoleAssetExample))]
//        [Route("user/{realPageId}/product/{productCode}")]
//        [AuthorizeScope("enterpriseapi")]
//        [HttpGet]
//        public ActionResult GetUserRoleAsset(Guid realPageId, string productCode)
//        {
//            var userRoleAssetDto = new UserRoleAssetDto();
//            var userRoleAssetDtoList = new List<UserRoleAssetDto>();
//            var response = new PagedResponse { Meta = new Meta() };
//            var error = new ErrorResponse { Errors = new List<Error>() };

//            // Get person information
//            var person = _managePerson.GetPerson(realPageId);
//            if (person == null)
//            {
//                return NotFound();
//            }

//            // Get persona for the user in the current organization
//            var persona = _managePersona.GetFirstAvailablePersonaByCompany(
//                realPageId,
//                _userClaimsAccessor.OrganizationPartyId);

//            // Verify user belongs to same company
//            if (persona == null || persona.OrganizationPartyId != _userClaimsAccessor.OrganizationPartyId)
//            {
//                return NotFound();
//            }

//            // Get product information
//            var products = _productRepository.GetAllProducts();
//            var productId = ProductEnumHelper.GetProductIdByProductCode(productCode, products);

//            ListResponse listResponse = null;

//            switch (productId)
//            {
//                case (int)ProductEnum.OpsBuyer:
//                    var productList = _samlRepository.ListActiveProductsByPersonaId(
//                        persona.PersonaId,
//                        (int)ProductEnum.OpsBuyer,
//                        null);

//                    if (productList.Any(p => p.ProductStatus == (int)ProductBatchStatusType.Success))
//                    {
//                        listResponse = _manageProductOps.GetRoles(
//                            _userClaimsAccessor.PersonaId,
//                            persona.PersonaId,
//                            "",
//                            null);

//                        userRoleAssetDto.ProductRole = listResponse.Records
//                            .Cast<ProductRole>()
//                            .Where(p => p.IsAssigned)
//                            .ToList();

//                        listResponse = _manageProductOps.GetCompanyAssets(
//                            _userClaimsAccessor.PersonaId,
//                            persona.PersonaId,
//                            false,
//                            null);

//                        userRoleAssetDto.AssetGroups = listResponse.Records
//                            .Cast<AssetGroup>()
//                            .Where(p => p.IsAssigned)
//                            .ToList();

//                        userRoleAssetDtoList.Add(userRoleAssetDto);
//                    }
//                    break;

//                default:
//                    error.Errors.Add(new Error
//                    {
//                        Title = "Bad request",
//                        Detail = "No valid product code could be found",
//                        Source = "/user",
//                        StatusCode = ""
//                    });
//                    return BadRequest(error);
//            }

//            if (listResponse != null && !listResponse.IsError)
//            {
//                response.Data = userRoleAssetDtoList.Cast<object>().ToList();
//                response.Meta.CurrentPage = 1;
//                response.Meta.TotalRows = userRoleAssetDtoList.Count;
//                response.Meta.RowsPerPage = userRoleAssetDtoList.Count;
//                return Ok(response);
//            }

//            error.Errors.Add(new Error
//            {
//                Title = "Error",
//                Detail = listResponse?.ErrorReason,
//                Source = "/user",
//                StatusCode = ""
//            });
//            return BadRequest(error);
//        }

//        /// <summary>
//        /// List User Custom Fields
//        /// </summary>
//        [SwaggerResponse(401, Description = "Unauthorized")]
//        [SwaggerResponse(500, Description = "Internal Server Error")]
//        [SwaggerResponse(200, Description = "Gets the User Custom Fields", Type = typeof(ICustomFieldValue))]
//        [SwaggerResponseExamples(typeof(ICustomFieldValue), typeof(UserCustomFieldsExample))]
//        [Route("customfieldsmaster")]
//        [AuthorizeScope("enterpriseapi")]
//        [HttpGet]
//        public ActionResult UserCustomFields(long? userLoginPersonaId = null)
//        {
//            var customFieldValueList = _manageCustomFields.GetCustomFieldsValues(
//                organizationPartyId: _userClaimsAccessor.OrganizationPartyId,
//                userLoginPersonaId: userLoginPersonaId,
//                enabled: true);

//            var response = new ListResponse
//            {
//                Records = customFieldValueList.Cast<object>().ToList(),
//                TotalRows = customFieldValueList.Count,
//                RowsPerPage = customFieldValueList.Count,
//                ErrorReason = string.Empty,
//                TotalPages = 1
//            };

//            return Ok(response);
//        }

//        /// <summary>
//        /// Get Saml product attributes by ProductId
//        /// </summary>
//        [SwaggerResponse(401, Description = "Unauthorized")]
//        [SwaggerResponse(500, Description = "Internal Server Error")]
//        [SwaggerResponse(200, Description = "Get information about the user", Type = typeof(SamlProductAttributes))]
//        [SwaggerResponseExamples(typeof(SamlProductAttributes), typeof(SamlProductAttributesExample))]
//        [Authorize]
//        [Route("user/productuser/attributes")]
//        [HttpGet]
//        public IList<SamlProductAttributes> GetSamlProductAttributes(int ProductId)
//        {
//            return _samlRepository.GetSamlProductAttributes(ProductId);
//        }

//        /// <summary>
//        /// Update details for a Realpage product user
//        /// </summary>
//        [SwaggerResponse(401, Description = "Unauthorized")]
//        [SwaggerResponse(500, Description = "Internal Server Error")]
//        [SwaggerResponse(200, Description = "Update successful", Type = typeof(HttpResponseMessage))]
//        [SwaggerResponse(400, Description = "Bad request")]
//        [Route("user/productuser/details")]
//        [AuthorizeScope("internalapi")]
//        [HttpPut]
//        public ActionResult UpdateProductUserAccountDetails([FromBody] ProductUserAccountDetails productUser)
//        {
//            if (productUser == null)
//            {
//                return BadRequest("productUser null.");
//            }

//            if (productUser.ProductId <= 0)
//            {
//                return BadRequest("ProductName empty.");
//            }

//            var result = _manageProductUser.UpdateProductUserAccountDetails(productUser, true);
//            return Ok(string.IsNullOrEmpty(result) ? "Success" : result);
//        }

//        /// <summary>
//        /// Delete details for a Realpage product user
//        /// </summary>
//        [SwaggerResponse(401, Description = "Unauthorized")]
//        [SwaggerResponse(500, Description = "Internal Server Error")]
//        [SwaggerResponse(200, Description = "Update successful", Type = typeof(HttpResponseMessage))]
//        [SwaggerResponse(400, Description = "Bad request")]
//        [Route("user/productuser/details")]
//        [AuthorizeScope("internalapi")]
//        [HttpDelete]
//        public ActionResult DeleteSamlUserProductInfoAndStatus([FromBody] ProductUserAccountDetails productUser)
//        {
//            if (productUser == null)
//            {
//                return BadRequest("productUser null.");
//            }

//            if (productUser.ProductId <= 0)
//            {
//                return BadRequest("ProductName empty.");
//            }

//            var result = _manageProductUser.DeleteSamlUserProductInfoAndStatus(productUser, true);
//            return Ok(string.IsNullOrEmpty(result) ? "Success" : result);
//        }

//        /// <summary>
//        /// Gets the list of rights for the current authenticated user
//        /// </summary>
//        [SwaggerResponse(401, Description = "Unauthorized")]
//        [SwaggerResponse(500, Description = "Internal Server Error")]
//        [SwaggerResponse(200, Description = "Get the users UnifiedLogin rights")]
//        [Route("user/rights/current")]
//        [HttpGet]
//        [AuthorizeScope("userinfoapi", "landingapi")]
//        public ActionResult GetCurrentUserRights()
//        {
//            var userClaim = _userClaimsAccessor.GetUserClaim();
//            return Ok(userClaim.Rights);
//        }

//        #endregion

//        #region Private Helper Methods

//        /// <summary>
//        /// Used to write to the central log
//        /// </summary>
//        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
//        {
//            try
//            {
//                var logger = Log.Logger;
//                if (logData?.Keys != null)
//                {
//                    logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
//                }
//                logger = logger.ForContext("ProductModule", this.GetType());
//                logger = logger.ForContext("CorrelationId", _userClaimsAccessor.CorrelationId.ToString());

//                logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
//            }
//            catch
//            {
//                /*ignored*/
//            }
//        }

//        #endregion

//        #region Swagger Examples

//        [ExcludeFromCodeCoverage]
//        public class SamlProductAttributesExample : IProvideExamples
//        {
//            public object GetExamples()
//            {
//                return new List<SamlProductAttributes>
//                {
//                    new SamlProductAttributes
//                    {
//                        DisplayName = "Username",
//                        ProductID = 1,
//                        SamlAttributeName = "productUsername"
//                    },
//                    new SamlProductAttributes
//                    {
//                        DisplayName = "PMC ID",
//                        ProductID = 1,
//                        SamlAttributeName = "PMCID"
//                    }
//                };
//            }
//        }

//        [ExcludeFromCodeCoverage]
//        public class UserCustomFieldsExample : IProvideExamples
//        {
//            public object GetExamples()
//            {
//                var customFieldValueList = new List<CustomFieldValue>
//                {
//                    new CustomFieldValue
//                    {
//                        FieldValueId = 1,
//                        UserLoginPersonaId = 1,
//                        Value = "12345",
//                        FieldId = 15,
//                        OrganizationId = 350,
//                        Enabled = true,
//                        Name = "Employee ID",
//                        FieldTypeId = 1,
//                        FieldTypeName = "Alphanumeric",
//                        Required = false,
//                        ReadOnly = false,
//                        Sequence = 1,
//                        MinCharLength = 1,
//                        MaxCharLength = 10
//                    }
//                };

//                return new ListResponse
//                {
//                    Records = customFieldValueList.Cast<object>().ToList(),
//                    TotalRows = customFieldValueList.Count,
//                    RowsPerPage = customFieldValueList.Count,
//                    ErrorReason = string.Empty,
//                    TotalPages = 1
//                };
//            }
//        }
//        /// <summary>
//        /// 
//        /// </summary>

//        [ExcludeFromCodeCoverage]
//        public class EnterpriseGetUserRoleAssetExample : IProvideExamples
//        {
//            public object GetExamples()
//            {
//                var userRoleAssetDtoList = new List<UserRoleAssetDto>
//                {
//                    new UserRoleAssetDto
//                    {
//                        ProductRole = new List<ProductRole>
//                        {
//                            new ProductRole
//                            {
//                                ID = "15088",
//                                Name = "Marketplace Administrator",
//                                IsAssigned = true,
//                                isEditorHasRight = false,
//                                Roletype = "1"
//                            }
//                        },
//                        AssetGroups = new List<AssetGroup>
//                        {
//                            new AssetGroup
//                            {
//                                ID = "1125",
//                                Name = "[G] CF Real Estate Services",
//                                Status = "active",
//                                GroupType = "company",
//                                AssetID = "204955",
//                                IsAssigned = true
//                            }
//                        }
//                    }
//                };

//                return new PagedResponse
//                {
//                    Meta = new Meta { TotalRows = 1, CurrentPage = 1, RowsPerPage = 1 },
//                    Data = userRoleAssetDtoList.Cast<object>().ToList()
//                };
//            }
//        }

//        #endregion
//    }
//}
