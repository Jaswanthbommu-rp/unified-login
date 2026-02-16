using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Unified Settings Controller
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class UnifiedSettingsController : BaseController
    {
        #region Private variables
        private readonly IRepositoryResponse _repositoryResponse;
        private readonly IManageOrganization _manageOrganization;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userClaimsAccessor">User claims accessor</param>
        public UnifiedSettingsController(IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {

            var userClaim = _userClaimsAccessor.GetUserClaim();
            _repositoryResponse = new RepositoryResponse();
            _manageOrganization = new ManageOrganization(userClaim);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get Settings Details
        /// </summary>
        /// <param name="category">Setting category (e.g. Security, CustomFields)</param>
        /// <param name="companyId">Organization Id</param>
        /// <param name="includes">filter</param>
        /// <returns>A Settings Details based on category</returns>
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(IList<Setting>), (int)HttpStatusCode.OK)]
        [HttpGet("companies/{companyId}/settings")]
        public async Task<IActionResult> GetSettings(string category, Guid companyId, [FromQuery] string[] includes = null)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                Organization organization = new Organization();
                IApiError apiError;

                if (companyId != Guid.Empty)
                {
                    Organization org = _manageOrganization.GetOrganization(companyId);
                    if (org == null)
                    {
                        apiError = new ApiError()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Status = (short)HttpStatusCode.BadRequest,
                            Title = "Company not found.",
                            Detail = $"Company not found for Id: {companyId}",
                            Links = string.Empty,
                            Code = "Settings.GetSettings.2",
                            Source = new ApiErrorSource()
                            {
                                JsonPointer = string.Empty,
                                Parameter = string.Empty
                            }
                        };
                        return BadRequest(apiError);
                    }

                    bool IsValid = _manageOrganization.ValidateOrganization(userClaim.OrganizationMasterId, userClaim.UserRealPageGuid, org.RealPageId);
                    if (!IsValid)
                    {
                        apiError = new ApiError()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Status = (short)HttpStatusCode.BadRequest,
                            Title = "User is not authorized.",
                            Detail = $"Logged in user is not authorized to view security settings for {org.Name}.",
                            Links = string.Empty,
                            Code = "Settings.GetSettings.3",
                            Source = new ApiErrorSource()
                            {
                                JsonPointer = string.Empty,
                                Parameter = string.Empty
                            }
                        };
                        return BadRequest(apiError);
                    }

                    UnifiedSetting unfiedSetting = new UnifiedSetting();
                    IManageUnifiedSettings manageSettings = new ManageUnifiedSettings(userClaim);
                    var settingList = manageSettings.GetUnifiedSettings(category, org.PartyId);

                    if (settingList == null)
                    {
                        //When trying to get a Security Settings that don't exists
                        apiError = new ApiError()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Status = (short)HttpStatusCode.NotFound,
                            Title = "Unified settings not found.",
                            Detail = $"Unified settings not found for {organization.Name}",
                            Links = string.Empty,
                            Code = "Settings.GetSettings.4",
                            Source = new ApiErrorSource()
                            {
                                JsonPointer = string.Empty,
                                Parameter = string.Empty
                            }
                        };
                        return BadRequest(apiError);
                    }
                    unfiedSetting.keys = (List<Setting>)settingList;
                    return Ok(unfiedSetting);
                }
                else
                {
                    apiError = new ApiError()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Status = (short)HttpStatusCode.BadRequest,
                        Title = "Null Companyd.",
                        Detail = $"Empty Company parameter passed",
                        Links = string.Empty,
                        Code = "Settings.GetSettings.2",
                        Source = new ApiErrorSource()
                        {
                            JsonPointer = string.Empty,
                            Parameter = string.Empty
                        }
                    };
                    return BadRequest(apiError);
                }
            });
        }
        #endregion
    }
}
