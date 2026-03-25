using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.Services.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Used to record an activity for the given user
    /// </summary>
    [Authorize]
    [Route("")]
    [ApiController]
    public class ActivityController : BaseController
    {
        private readonly IProductService _productService;

        /// <summary>
        /// Constructor with dependency injection for user claims accessor and product service
        /// </summary>
        public ActivityController(IUserClaimsAccessor userClaimsAccessor, IProductService productService) : base(userClaimsAccessor)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

        /// <summary>
        /// Record an activity for the given user
        /// </summary>
        /// <param name="activityType">Type of activity to record (e.g., "logout")</param>
        /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
        /// <returns>Success or error response</returns>
        [HttpPost("activity/{activityType}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RecordActivity(string activityType, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(activityType))
                return BadRequest("Activity type cannot be empty");

            switch (activityType.ToLowerInvariant())
            {
                case "logout":
                    await LogSignoutActivityAsync(cancellationToken);
                    break;

                default:
                    return BadRequest("Invalid activity type");
            }

            return Ok("Success");
        }

        private async Task LogSignoutActivityAsync(CancellationToken cancellationToken)
        {
            try
            {
                var userId = _userClaimsAccessor.UserId;
                var firstName = _userClaimsAccessor.FirstName;
                var lastName = _userClaimsAccessor.LastName;

                if (userId != 0 && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                {
                    var products = await _productService.ListProductsAsync(productId: (int)ProductEnum.UnifiedPlatform, cancellationToken: cancellationToken);
                    var booksProductDetail = products.FirstOrDefault();

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
