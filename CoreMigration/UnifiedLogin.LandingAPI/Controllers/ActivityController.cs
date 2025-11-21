using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.BusinessLogic.Logic.Helper;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Used to record an activity for the given user
    /// </summary>
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class ActivityController : ControllerBase
    {
        private readonly IUserClaimsAccessor _userClaimsAccessor;
        private readonly IManageProduct _manageProduct;

        /// <summary>
        /// Constructor with dependency injection for user claims accessor and product management
        /// </summary>
        /// <param name="userClaimsAccessor">Accessor for current authenticated user's claims</param>
        /// <param name="manageProduct">Service for managing product operations</param>
        public ActivityController(IUserClaimsAccessor userClaimsAccessor, IManageProduct manageProduct)
        {
            _userClaimsAccessor = userClaimsAccessor ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
            _manageProduct = manageProduct ?? throw new ArgumentNullException(nameof(manageProduct));
        }

        /// <summary>
        /// Record an activity for the given user
        /// </summary>
        /// <param name="activityType">Type of activity to record (e.g., "logout")</param>
        /// <returns>Success or error response</returns>
        [HttpPost("{activityType}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RecordActivity(string activityType)
        {
            if (string.IsNullOrWhiteSpace(activityType))
            {
                return BadRequest("Activity type cannot be empty");
            }

            switch (activityType.ToLowerInvariant())
            {
                case "logout":
                    await LogSignoutActivityAsync();
                    break;

                default:
                    return BadRequest("Invalid activity type");
            }

            return Ok("Success");
        }

        /// <summary>
        /// Gets the Books Master product detail for a specific product
        /// </summary>
        /// <param name="gbProductId">Green Book product ID</param>
        /// <returns>Product mapping or null if not found</returns>
        private GbProductMap GetBooksMasterProductDetail(int gbProductId)
        {
            var gbProductMap = _manageProduct.ListProducts()
                                             .FirstOrDefault(x => x.ProductId == gbProductId);
            return gbProductMap;
        }

        /// <summary>
        /// Used to record a log out activity for a user
        /// </summary>
        private async Task LogSignoutActivityAsync()
        {
            try
            {
                var userId = _userClaimsAccessor.UserId;
                var firstName = _userClaimsAccessor.FirstName;
                var lastName = _userClaimsAccessor.LastName;

                if (userId != 0 && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                {
                    GbProductMap booksProductDetail = GetBooksMasterProductDetail((int)ProductEnum.UnifiedPlatform);

                    await Task.Run(() =>
                    {
                        LogActivity.WriteActivity(new ActivityDetails
                        {
                            LogActivityTypeName = LogActivityTypeConstants.SIGNOUT,
                            LogCategoryName = LogActivityCategoryType.Security.ToString(),
                            CorrelationId = _userClaimsAccessor.CorrelationId.ToString(),
                            BooksMasterOrganizationId = _userClaimsAccessor.OrganizationMasterId,
                            OrganizationPartyId = _userClaimsAccessor.OrganizationPartyId,
                            Message = $"User {firstName} {lastName} signout successfully.",
                            FromUserLoginName = _userClaimsAccessor.LoginName,
                            FromUserLoginId = userId,
                            FromUserFirstName = firstName,
                            FromUserLastName = lastName,
                            FromUserRealpageId = _userClaimsAccessor.UserRealPageGuid.ToString(),
                            ToUserLoginName = null,
                            ToUserLoginId = null,
                            BooksProductCode = booksProductDetail?.BooksProductCode
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                string message = $"User {_userClaimsAccessor.LoginName} failed to signout.";
                Log.Error(ex, message);
            }
        }
    }
}
