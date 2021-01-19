using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Serilog;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    /// <summary>
    /// Used to record an activity for the given user
    /// </summary>
    public class ActivityController : BaseApiController
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ActivityController() : base()
        {
        }

        /// <summary>
        /// Record an activity for the given user
        /// </summary>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Success")]
        [Route("activity/{activityType}")]
        [HttpPost]

        public HttpResponseMessage RecordActivity(string activityType)
        {
            switch (activityType.ToLowerInvariant())
            {
                case "logout":
                    LogSignoutActivity();
                    break;

                default:
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid request");
            }

            return Request.CreateResponse(HttpStatusCode.OK, "Success");
        }


        private GbProductMap GetBooksMasterProductDetail(DefaultUserClaim userClaim, int gbProductId)
        {
            var gbProductMap = GetGbProductMap(userClaim).FirstOrDefault(x => x.ProductId == gbProductId);
            return gbProductMap;
        }

        private static IList<GbProductMap> GetGbProductMap(DefaultUserClaim userClaim)
        {
            // Get products
            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = "GB-BB-ProductMap";
            var products = rpcache.GetFromCache<IList<GbProductMap>>(cacheKey, 3600, () =>
            {
                IManageProduct manageProduct = new ManageProduct(userClaim);
                return manageProduct.ListProducts();
            });

            return products;
        }

        /// <summary>
        /// Used to record a log out activity for a user
        /// </summary>
        private void LogSignoutActivity()
        {
            try
            {
                if (_userClaims.UserId != 0 && !string.IsNullOrEmpty(_userClaims.FirstName) && !string.IsNullOrEmpty(_userClaims.LastName))
                {
                    GbProductMap booksProductDetail = GetBooksMasterProductDetail(_userClaims, (int)ProductEnum.UnifiedPlatform);

                    LogActivity.WriteActivity(new ActivityDetails
                    {
                        LogActivityTypeName = LogActivityTypeConstants.SIGNOUT,
                        LogCategoryName = LogActivityCategoryType.Security.ToString(),
                        CorrelationId = _userClaims.CorrelationId.ToString(),
                        BooksMasterOrganizationId = _userClaims.OrganizationMasterId,
                        OrganizationPartyId = _userClaims.OrganizationPartyId,
                        Message = $"User {_userClaims.FirstName} {_userClaims.LastName} signout successfully.",
                        FromUserLoginName = _userClaims.LoginName,
                        FromUserLoginId = _userClaims.UserId,
                        FromUserFirstName = _userClaims.FirstName,
                        FromUserLastName = _userClaims.LastName,
                        FromUserRealpageId = _userClaims.UserRealPageGuid.ToString(),
                        ToUserLoginName = null,
                        ToUserLoginId = null,
                        BooksProductCode = booksProductDetail.BooksProductCode
                    });
                }
            }
            catch (Exception ex)
            {
                string message = $"User {_userClaims.LoginName} failed to signout.";

                Log.Error(ex, message);
            }
        }
    }
}