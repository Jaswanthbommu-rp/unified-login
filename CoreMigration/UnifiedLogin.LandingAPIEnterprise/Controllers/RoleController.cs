using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.Swagger.Annotations;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using UnifiedLogin.BusinessLogic.Attributes;
using UnifiedLogin.LandingAPIEnterprise.Services.Role;
using UnifiedLogin.SharedObjects.Attribute;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Rum;
using UnifiedLogin.SharedObjects.ResponseObject;
using UnifiedLogin.SharedObjects.Swagger;

namespace UnifiedLogin.LandingAPIEnterprise.Controllers
{
    /// <summary>
    /// Used to get product roles
    /// </summary>
    [ApiController]
    [Route("")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleQueryService _roleQueryService;
        private readonly IClientCredentialAuthenticator _clientCredentialAuthenticator;
       // private readonly DefaultUserClaim _userClaims;
        private readonly IUserClaimsAccessor _userClaimsAccessor;
        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public RoleController(
            IRoleQueryService roleQueryService,
            IClientCredentialAuthenticator clientCredentialAuthenticator,
           // DefaultUserClaim userClaims, 
            IUserClaimsAccessor userClaimsAccessor)
        {
            _roleQueryService = roleQueryService ?? throw new ArgumentNullException(nameof(roleQueryService));
            _clientCredentialAuthenticator = clientCredentialAuthenticator ?? throw new ArgumentNullException(nameof(clientCredentialAuthenticator));
            //_userClaims = userClaims ?? throw new ArgumentNullException(nameof(userClaims));
            _userClaimsAccessor = userClaimsAccessor;
        }

        /// <summary>
        /// Get a list of roles for the given user and product
        /// </summary>
        /// <param name="realPageId">The guid for the user being requested</param>
        /// <param name="productCode">The code for the product being requested. All Products are supported</param>
        /// <param name="upfmId">UPFM company id, can only be used with client credential token and internalapi scope</param>
        /// <returns>A list of product roles</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Not Found")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles", Type = typeof(ProductRole))]
        [SwaggerResponseExamples(typeof(ProductRole), typeof(EnterpriseRoleExample))]
       
        [AuthorizeScope("enterpriseapi")]
        [HttpGet("user/{realPageId}/product/{productCode}/roles")]
        public ActionResult GetUserProductRoles(Guid realPageId, string productCode, Guid? upfmId = null)
        {
            var userClaims = _userClaimsAccessor.GetUserClaim();
            var authResult = _clientCredentialAuthenticator.Authenticate(User, userClaims, upfmId);
            if (authResult.IsError)
            {
                return BadRequest(authResult.ErrorResponse);
            }

            var result = _roleQueryService.GetUserProductRoles(realPageId, productCode);
            return ToActionResult(result);
        }

        /// <summary>
        /// Get a list of roles for a product
        /// </summary>
        /// <param name="productCode">The code for the product being requested. All Products are supported</param>
        /// <param name="upfmId">UPFM company id, can only be used with client credential token and internalapi scope</param>
        /// <returns>A list of product roles</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Not Found")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles", Type = typeof(ProductRole))]
        [SwaggerResponseExamples(typeof(ProductRole), typeof(EnterpriseRoleExample))]
        
        [AuthorizeScope("enterpriseapi")]
        [HttpGet("product/{productCode}/roles")]
        public ActionResult GetProductRoles(string productCode, Guid? upfmId = null)
        {
            var userClaims = _userClaimsAccessor.GetUserClaim();
            var authResult = _clientCredentialAuthenticator.Authenticate(User, userClaims, upfmId);
            if (authResult.IsError)
            {
                return BadRequest(authResult.ErrorResponse);
            }

            var result = _roleQueryService.GetProductRoles(productCode);
            return ToActionResult(result);
        }

        /// <summary>
        /// Get a list of Rights for a Role
        /// </summary>
        /// <param name="productCode">The code for the product being requested. Only Unified Login controlled products are supported</param>
        /// <param name="roleId">roleId is being requested</param>
        /// <param name="upfmId">UPFM company id, can only be used with client credential token and internalapi scope</param>
        /// <returns>A list of rights for a Role</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Not Found")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles", Type = typeof(ProductRole))]
        [SwaggerResponseExamples(typeof(ProductRole), typeof(EnterpriseRoleExample))]
       
        [AuthorizeScope("enterpriseapi")]
        [HttpGet("product/{productCode}/roles/{roleId}/rights")]
        public ActionResult GetRightsforRole(string productCode, int roleId, Guid? upfmId = null)
        {
            var userClaims = _userClaimsAccessor.GetUserClaim();
            var authResult = _clientCredentialAuthenticator.Authenticate(User, userClaims, upfmId);
            if (authResult.IsError)
            {
                return BadRequest(authResult.ErrorResponse.Errors);
            }

            var result = _roleQueryService.GetRightsForRole(productCode, roleId);
            return ToActionResult(result);
        }

        private ActionResult ToActionResult(ActionResultEnvelope result)
        {
            return result.Kind switch
            {
                ActionResultKind.Ok => Ok(result.Value),
                ActionResultKind.NotFound => NotFound(),
                ActionResultKind.BadRequest => BadRequest(result.Value),
                _ => BadRequest(result.Value)
            };
        }

        #region GetExamples

        /// <summary>
        /// Used to document examples of the webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class EnterpriseRoleExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>EnterpriseRole example</returns>
            public object GetExamples()
            {
                IList<object> list = new List<object>
                {
                    new UnifiedLogin.SharedObjects.Product.ProductRole
                    {
                        ID = "1",
                        Name = "UnifiedLogin Test Role",
                        Description = "UnifiedLogin Test Description",
                        IsAssigned = false,
                        Roletype = "System",
                        Alias = "BasicUser"
                    },
                    new UnifiedLogin.SharedObjects.Product.ProductRole
                    {
                        ID = "21",
                        Name = "Ops Test Role",
                        Description = "Ops Test Description",
                        IsAssigned = false,
                        Roletype = "Not Used"
                    }
                };

                return new PagedResponse
                {
                    Meta = new Meta { TotalRows = list.Count, CurrentPage = 1, RowsPerPage = list.Count },
                    Data = list.Cast<object>().ToList()
                };
            }
        }

        #endregion
    }
}