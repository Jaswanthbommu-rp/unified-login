using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Attributes;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Security;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.OneSite;
 
namespace UnifiedLogin.LandingAPIEnterprise.Controllers
{
    /// <summary>
    /// User management controller for UnifiedLogin
    /// </summary>
    [ApiController]
    [Route("api")]
    public class UserController : BaseApiController
    {
        private readonly IRepository? _repository;
        private readonly IOneSiteProductService? _oneSiteProductService;
        private readonly HttpMessageHandler? _messageHandler;

        // Default parameterless constructor used by ASP.NET Core DI if no services are wired yet
        public UserController() : base() { }

        /// <summary>
        /// Unit test / extended constructor (optional DI registration) retaining legacy dependencies.
        /// </summary>
        public UserController(IRepository repository, HttpMessageHandler messageHandler, DefaultUserClaim userClaims, IOneSiteProductService oneSiteProductService)
            : base(repository, messageHandler, userClaims)
        {
            _repository = repository;
            _messageHandler = messageHandler;
            _oneSiteProductService = oneSiteProductService;

            // Instantiate legacy logic objects only if needed in future endpoints
            var productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            var manageBlueBook = new ManageBlueBook(userClaims, repository, productInternalSettingRepository, messageHandler);
            var personaRightRepository = new PersonaRightRepository(repository);
            var manageUnifiedLogin = new ManageUnifiedLogin(repository, userClaims, messageHandler);
            var manageProductOneSite = new ManageProductOneSite(repository, userClaims, messageHandler, oneSiteProductService);
        }

        /// <summary>
        /// Get the current user's UnifiedLogin rights
        /// </summary>
        /// <returns>A list of the user's rights</returns>
        /// <response code="200">Successfully retrieved user rights</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="500">Internal server error occurred</response>
        [HttpGet("user/rights/current")]
        [Authorize] // Replace AuthorizeScope with standard Authorize or configure policy-based authorization
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetCurrentUserRights()
        {
            try
            {
                List<string> userRights = _userClaims.Rights ?? new List<string>();
                return Ok(userRights);
            }
            catch (Exception ex)
            {
                WriteToErrorLog("Error retrieving user rights", 
                    messageProperties: new object[] { "GetCurrentUserRights", ex.Message },
                    exception: ex);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "An error occurred while retrieving user rights" });
            }
        }
    }
}