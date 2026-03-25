using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Controller for all product management related APIs
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class ProductResidentPortalController : BaseController
    {
        private readonly IManageProductResidentPortalAsync _manageProductResidentPortalAsync;
        private readonly IManagePersonaAsync _managePersonaAsync;

        /// <summary>
        /// Constructor for ProductResidentPortalController
        /// </summary>
        /// <param name="userClaimsAccessor">User claims accessor</param>
        /// <param name="manageProductResidentPortalAsync">Async service for resident portal operations</param>
        /// <param name="managePersonaAsync">Async service for persona operations</param>
        public ProductResidentPortalController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageProductResidentPortalAsync manageProductResidentPortalAsync,
            IManagePersonaAsync managePersonaAsync) : base(userClaimsAccessor)
        {
            _manageProductResidentPortalAsync = manageProductResidentPortalAsync;
            _managePersonaAsync = managePersonaAsync;
        }

        #region Public Methods
        /// <summary>
        /// Returns Properties
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <param name="datafilter">A datafilter used to filter the properties.</param>
        [ProducesResponseType(typeof(ListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("products/residentportal/properties")]
        public async Task<IActionResult> ListProperties(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return Ok("editorPersonaId not supplied.");

            var userClaim = _userClaimsAccessor.GetUserClaim();
            if ((userClaim.UserRealPageGuid == Guid.Empty) || (userClaim.UserRealPageGuid == null))
                return Ok("RealPageId empty.");

            var listResponse = await _manageProductResidentPortalAsync.ListPropertiesAsync(userClaim, editorPersonaId, userPersonaId, datafilter, cancellationToken);
            return Ok(listResponse);
        }

        /// <summary>
        /// Get Notification Settings
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>Response with Success Message</returns>
        [ProducesResponseType(typeof(INotifications), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("products/residentportal/notifications")]
        public async Task<IActionResult> GetNotificationSettings(long editorPersonaId, long userPersonaId = 0, CancellationToken cancellationToken = default)
        {
            ObjectOutput<INotifications, IErrorData> output = new ObjectOutput<INotifications, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            output.Status = errorStatus;

            if (editorPersonaId == 0)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ProductResidentPortal.GetResidentPortalNotificationSettings.1";
                errorStatus.ErrorMsg = "Invalid parameter - editorPersonaId";
                output.Status = errorStatus;
                return Ok(output);
            }

            var userClaim = _userClaimsAccessor.GetUserClaim();
            if ((userClaim.UserRealPageGuid == Guid.Empty) || (userClaim.UserRealPageGuid == null))
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ProductResidentPortal.GetResidentPortalNotificationSettings.2";
                errorStatus.ErrorMsg = "Invalid - Enterprise User Id";
                output.Status = errorStatus;
                return Ok(output);
            }

            var notifications = await _manageProductResidentPortalAsync.GetNotificationSettingsAsync(userClaim, editorPersonaId, userPersonaId, cancellationToken);
            if (notifications == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ProductResidentPortal.GetResidentPortalNotificationSettings.3";
                errorStatus.ErrorMsg = $"Product Resident Portal - Get notification settings: Invalid User Id for PersonaId- {userPersonaId}";
                output.Status = errorStatus;
                return Ok(output);
            }
            output.obj = notifications;
            return Ok(output);
        }

        /// <summary>
        /// Used to Resident Portal enterprise user details
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>Response with Success Message</returns>
        [ProducesResponseType(typeof(IResidentPortalUser), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("products/residentportal/user")]
        public async Task<IActionResult> GetResidentPortalUser(long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default)
        {
            ObjectOutput<IResidentPortalUser, IErrorData> output = new ObjectOutput<IResidentPortalUser, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            output.Status = errorStatus;

            if (editorPersonaId == 0)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ProductResidentPortal.GetResidentPortalUser.1";
                errorStatus.ErrorMsg = "Invalid parameter - editorPersonaId";
                output.Status = errorStatus;
                return Ok(output);
            }

            if (userPersonaId == 0)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ProductResidentPortal.GetResidentPortalUser.2";
                errorStatus.ErrorMsg = "Invalid parameter - userPersonaId";
                output.Status = errorStatus;
                return Ok(output);
            }

            var userClaim = _userClaimsAccessor.GetUserClaim();
            if ((userClaim.UserRealPageGuid == Guid.Empty) || (userClaim.UserRealPageGuid == null))
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ProductResidentPortal.GetResidentPortalUser.3";
                errorStatus.ErrorMsg = "Invalid - Enterprise User Id";
                output.Status = errorStatus;
                return Ok(output);
            }

            var residentPortalUser = await _manageProductResidentPortalAsync.GetUserAsync(userClaim, editorPersonaId, userPersonaId, cancellationToken);
            if (residentPortalUser == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ProductResidentPortal.GetResidentPortalUser.4";
                errorStatus.ErrorMsg = $"Product Resident Portal - Get user: Invalid User Id for PersonaId- {userPersonaId}";
                output.Status = errorStatus;
                return Ok(output);
            }
            residentPortalUser = await _manageProductResidentPortalAsync.SetLevelAndGroupObjectsAsync(userClaim, editorPersonaId, userPersonaId, residentPortalUser, cancellationToken);
            output.obj = residentPortalUser;
            return Ok(output);
        }

        /// <summary>
        /// List Levels (Resident Poratl roles)
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>Response with Success Message</returns>
        [ProducesResponseType(typeof(ILevel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("products/residentportal/levels")]
        public async Task<IActionResult> ListLevels(long editorPersonaId, long userPersonaId = 0, CancellationToken cancellationToken = default)
        {
            ObjectListOutput<ILevel, IErrorData> output = new ObjectListOutput<ILevel, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            output.Status = errorStatus;

            if (editorPersonaId == 0)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ProductResidentPortal.ListLevel.1";
                errorStatus.ErrorMsg = "Invalid parameter - editorPersonaId";
                output.Status = errorStatus;
                return Ok(output);
            }

            var userClaim = _userClaimsAccessor.GetUserClaim();
            if ((userClaim.UserRealPageGuid == Guid.Empty) || (userClaim.UserRealPageGuid == null))
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ProductResidentPortal.ListLevel.2";
                errorStatus.ErrorMsg = "Invalid - Enterprise User Id";
                output.Status = errorStatus;
                return Ok(output);
            }

            var levelList = await _manageProductResidentPortalAsync.ListLevelsAsync(userClaim, editorPersonaId, userPersonaId, cancellationToken);
            if (levelList == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ProductResidentPortal.ListLevel.3";
                errorStatus.ErrorMsg = "Product Resident Portal - List levels: No data";
                output.Status = errorStatus;
                return Ok(output);
            }
            output.list = levelList;
            return Ok(output);
        }

        /// <summary>
        /// List Messaging groups
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>Response with Success Message</returns>
        [ProducesResponseType(typeof(IMessagingGroups), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("products/residentportal/messagegroups")]
        public async Task<IActionResult> ListMessagingGroups(long editorPersonaId, long userPersonaId = 0, CancellationToken cancellationToken = default)
        {
            ObjectListOutput<IMessagingGroups, IErrorData> output = new ObjectListOutput<IMessagingGroups, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            output.Status = errorStatus;

            if (editorPersonaId == 0)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ProductResidentPortal.ListMessagingGroup.1";
                errorStatus.ErrorMsg = "Invalid parameter - editorPersonaId";
                output.Status = errorStatus;
                return Ok(output);
            }

            var userClaim = _userClaimsAccessor.GetUserClaim();
            if ((userClaim.UserRealPageGuid == Guid.Empty) || (userClaim.UserRealPageGuid == null))
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ProductResidentPortal.ListMessagingGroup.2";
                errorStatus.ErrorMsg = "Invalid - Enterprise User Id";
                output.Status = errorStatus;
                return Ok(output);
            }

            var messageGroupsList = await _manageProductResidentPortalAsync.ListMessageGroupsAsync(userClaim, editorPersonaId, userPersonaId, cancellationToken);
            if (messageGroupsList == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ProductResidentPortal.ListMessagingGroup.3";
                errorStatus.ErrorMsg = "Product Resident Portal - List messaging groups: No data";
                output.Status = errorStatus;
                return Ok(output);
            }
            output.list = messageGroupsList;
            return Ok(output);
        }

        /// <summary>
        /// List Titles
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>Response with Success Message</returns>
        [ProducesResponseType(typeof(ITitle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("products/residentportal/titles")]
        public async Task<IActionResult> ListTitles(long editorPersonaId, long userPersonaId = 0, CancellationToken cancellationToken = default)
        {
            ObjectListOutput<ITitle, IErrorData> output = new ObjectListOutput<ITitle, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            output.Status = errorStatus;

            if (editorPersonaId == 0)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ProductResidentPortal.ListTitles.1";
                errorStatus.ErrorMsg = "Invalid parameter - editorPersonaId";
                output.Status = errorStatus;
                return Ok(output);
            }

            var userClaim = _userClaimsAccessor.GetUserClaim();
            if ((userClaim.UserRealPageGuid == Guid.Empty) || (userClaim.UserRealPageGuid == null))
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ProductResidentPortal.ListTitles.2";
                errorStatus.ErrorMsg = "Invalid - Enterprise User Id";
                output.Status = errorStatus;
                return Ok(output);
            }

            var titleList = await _manageProductResidentPortalAsync.ListTitlesAsync(userClaim, editorPersonaId, userPersonaId, cancellationToken);
            if (titleList == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ProductResidentPortal.ListTitles.3";
                errorStatus.ErrorMsg = "Product Resident Portal - List titles: No data";
                output.Status = errorStatus;
                return Ok(output);
            }
            output.list = titleList;
            return Ok(output);
        }
        #endregion

        #region Migration API
        /// <summary>
        /// Returns product users of an organization for given user.
        /// </summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("products/residentportal/migration-users")]
        public async Task<IActionResult> ListResidentPortalMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter, CancellationToken cancellationToken = default)
        {
            if (editorPersonaId == 0)
                return BadRequest("editorPersonaId not supplied.");

            var persona = await _managePersonaAsync.GetPersonaAsync(editorPersonaId, false, cancellationToken);
            if (persona == null)
                return BadRequest("editorPersonaId not found.");

            var userClaim = _userClaimsAccessor.GetUserClaim();
            userClaim.UserRealPageGuid = persona.RealPageId;

            var result = await _manageProductResidentPortalAsync.GetMigrationUsersAsync(userClaim, editorPersonaId, datafilter, cancellationToken);
            if (!result.IsError)
                return Ok(result);
            else
                return StatusCode(StatusCodes.Status403Forbidden, result);
        }

        /// <summary>
        /// Update migration status of users.
        /// </summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut("products/residentportal/migrate-users")]
        public async Task<IActionResult> UpdateUsersMigrationStatus(IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var result = await _manageProductResidentPortalAsync.UpdateUsersMigrationStatusAsync(userClaim, userClaim.PersonaId, migrateUsers, cancellationToken);
            return Ok(result);
        }

        #endregion

        #region User-Status

        /// <summary>
        /// Deletes the resident portal user.
        /// </summary>
        /// <param name="produtUser">The produt user.</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut("products/residentportal/user/MT/status")]
        public async Task<IActionResult> UpdateResidentPortalUserStatus(ProductUser produtUser, CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            if (!await _manageProductResidentPortalAsync.DeleteUserAsync(userClaim, userClaim.PersonaId, produtUser.UserId, produtUser.UserName, cancellationToken))
                return BadRequest("Deleteing ResidentPortal user failed.");

            return Ok("Successfully disabled product user.");
        }

        #endregion
    }
}
