using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Controller for all product management related APIs
    /// </summary>
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class ProductResidentPortalController : ControllerBase
    {
        private readonly IUserClaimsAccessor _userClaimsAccessor;

        /// <summary>
        /// Constructor for ProductResidentPortalController
        /// </summary>
        /// <param name="userClaimsAccessor">User claims accessor</param>
        public ProductResidentPortalController(IUserClaimsAccessor userClaimsAccessor)
        {
            _userClaimsAccessor = userClaimsAccessor;
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
        public async Task<IActionResult> ListProperties(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (editorPersonaId == 0)
                {
                    return Ok("editorPersonaId not supplied.");
                }

                var userClaim = _userClaimsAccessor.GetUserClaim();
                if ((userClaim.UserRealPageGuid == Guid.Empty) || (userClaim.UserRealPageGuid == null))
                {
                    return Ok("RealPageId empty.");
                }

                ListResponse listResponse = new ListResponse();
                ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(userClaim);
                listResponse = manageProductResidentPortal.ListProperties(editorPersonaId, userPersonaId, datafilter);

                return Ok(listResponse);
            });
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
        public async Task<IActionResult> GetNotificationSettings(long editorPersonaId, long userPersonaId = 0)
        {
            return await Task.Run<IActionResult>(() =>
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

                INotifications notifications = new Notifications();
                ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(userClaim);

                notifications = manageProductResidentPortal.GetNotificationSettings(editorPersonaId, userPersonaId);
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
            });
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
        public async Task<IActionResult> GetResidentPortalUser(long editorPersonaId, long userPersonaId)
        {
            return await Task.Run<IActionResult>(() =>
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

                IResidentPortalUser residentPortalUser = new ResidentPortalUser();
                ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(userClaim);

                residentPortalUser = manageProductResidentPortal.GetUser(editorPersonaId, userPersonaId);
                if (residentPortalUser == null)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "ProductResidentPortal.GetResidentPortalUser.4";
                    errorStatus.ErrorMsg = $"Product Resident Portal - Get user: Invalid User Id for PersonaId- {userPersonaId}";
                    output.Status = errorStatus;
                    return Ok(output);
                }
                residentPortalUser = manageProductResidentPortal.SetLevelAndGroupObjects(editorPersonaId, userPersonaId, residentPortalUser);
                output.obj = residentPortalUser;
                return Ok(output);
            });
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
        public async Task<IActionResult> ListLevels(long editorPersonaId, long userPersonaId = 0)
        {
            return await Task.Run<IActionResult>(() =>
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

                List<ILevel> levelList = new List<ILevel>();
                ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(userClaim);

                levelList = manageProductResidentPortal.ListLevels(editorPersonaId, userPersonaId);
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
            });
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
        public async Task<IActionResult> ListMessagingGroups(long editorPersonaId, long userPersonaId = 0)
        {
            return await Task.Run<IActionResult>(() =>
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

                List<IMessagingGroups> messageGroupsList = new List<IMessagingGroups>();
                ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(userClaim);

                messageGroupsList = manageProductResidentPortal.ListMessageGroups(editorPersonaId, userPersonaId);
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
            });
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
        public async Task<IActionResult> ListTitles(long editorPersonaId, long userPersonaId = 0)
        {
            return await Task.Run<IActionResult>(() =>
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

                List<ITitle> titleList = new List<ITitle>();
                ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(userClaim);

                titleList = manageProductResidentPortal.ListTitles(editorPersonaId, userPersonaId);
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
            });
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
        public async Task<IActionResult> ListResidentPortalMigrationUsers(long editorPersonaId, [FromQuery] RequestParameter datafilter)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");

                var userClaim = _userClaimsAccessor.GetUserClaim();
                ManagePersona managePersona = new ManagePersona();
                var persona = managePersona.GetPersona(editorPersonaId);
                if (persona == null)
                    return BadRequest("editorPersonaId not found.");

                userClaim.UserRealPageGuid = persona.RealPageId;
                var manageProductResidentPortal = new ManageProductResidentPortal(userClaim);

                var result = manageProductResidentPortal.GetMigrationUsers(editorPersonaId, datafilter);
                if (!result.IsError)
                    return Ok(result);
                else
                    return StatusCode(StatusCodes.Status403Forbidden, result);
            });
        }

        /// <summary>
        /// Update migration status of users.
        /// </summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut("products/residentportal/migrate-users")]
        public async Task<IActionResult> UpdateUsersMigrationStatus(IList<MigrateUser> migrateUsers)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageProductResidentPortal = new ManageProductResidentPortal(userClaim);
                return Ok(manageProductResidentPortal.UpdateUsersMigrationStatus(userClaim.PersonaId, migrateUsers));
            });
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
        public async Task<IActionResult> UpdateResidentPortalUserStatus(ProductUser produtUser)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageProductResidentPortal = new ManageProductResidentPortal(userClaim);
                if (!manageProductResidentPortal.DeleteUser(userClaim.PersonaId, produtUser.UserId, produtUser.UserName))
                {
                    return BadRequest("Deleteing ResidentPortal user failed.");
                }
                return Ok("Successfully disabled product user.");
            });
        }

        #endregion
    }
}
