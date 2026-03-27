using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.LandingAPI.Attributes;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Profile Controller to hold (Person, UserLogin, Contact) management related APIs
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class ProfileController : BaseController
    {
        private readonly IManageProfileAsync _manageProfileAsync;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProfileController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageProfileAsync manageProfileAsync) : base(userClaimsAccessor)
        {
            _manageProfileAsync = manageProfileAsync;
        }

        /// <summary>
        /// Get a user detail
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
        /// <returns>Profile object</returns>
        [HttpGet("profiles/{realPageId}")]
        [ProducesResponseType(typeof(ObjectOutput<IProfile, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfile(Guid realPageId, [FromQuery] string ContactMechanismUsageTypeName = "", CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var output = new ObjectOutput<IProfile, IErrorData>();
            var errorStatus = new Status<IErrorData>();

            realPageId = (realPageId == Guid.Empty) ? userClaim.UserRealPageGuid : realPageId;
            if (realPageId == Guid.Empty)
            {
                output.obj = new Profile();
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Profile.GetProfile.1";
                errorStatus.ErrorMsg = "Invalid parameter: realPageId";
                output.Status = errorStatus;
                return BadRequest(output);
            }

            var profile = await _manageProfileAsync.GetProfileAsync(realPageId, ContactMechanismUsageTypeName, cancellationToken);
            if (profile != null)
            {
                output.obj = profile;
                output.Status = errorStatus;
                return Ok(output);
            }

            output.obj = new Profile();
            errorStatus.Success = false;
            errorStatus.ErrorCode = "Profile.GetProfile.1";
            errorStatus.ErrorMsg = "Invalid realPageId";
            output.Status = errorStatus;
            return BadRequest(output);
        }

        /// <summary>
        /// Get a user Profile detail
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="roleTypeFrom">Person Role Type name in the Relationship (Optional)</param>
        /// <param name="roleTypeTo">Organization Role Type name in the Relationship (Optional)</param>
        /// <param name="relationshipType">Parties Relationship type name (Optional)</param>
        /// <param name="contactMechanismUsageTypeName">Contact Mechanism UsageType Name (Optional)</param>
        /// <returns>ProfileDetail object</returns>
        [HttpGet("profiles/{realPageId}/organizations")]
        [ProducesResponseType(typeof(ObjectOutput<IProfile, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfileDetail(Guid realPageId, [FromQuery] string roleTypeFrom = null, [FromQuery] string roleTypeTo = null, [FromQuery] string relationshipType = null, [FromQuery] string contactMechanismUsageTypeName = null, CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var output = new ObjectOutput<IProfile, IErrorData>();
            var errorStatus = new Status<IErrorData>();

            realPageId = (realPageId == Guid.Empty) ? userClaim.UserRealPageGuid : realPageId;
            if (realPageId == Guid.Empty)
            {
                output.obj = new Profile();
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Profile.GetProfileDetail.1";
                errorStatus.ErrorMsg = "Invalid parameter: realPageId";
                errorStatus.ErrorData = null;
                output.Status = errorStatus;
                return BadRequest(output);
            }

            var found = await _manageProfileAsync.GetProfileDetailOrganizationsAsync(realPageId, roleTypeFrom, roleTypeTo, relationshipType, contactMechanismUsageTypeName, cancellationToken);
            if (found)
            {
                // Bug preserved: returns empty Profile, not profileDetail
                output.obj = new Profile();
                output.Status = errorStatus;
                return Ok(output);
            }

            output.obj = new Profile();
            errorStatus.Success = false;
            errorStatus.ErrorCode = "Prifile.GetProfileDetail.2";
            errorStatus.ErrorMsg = "Invalid realPageId";
            output.Status = errorStatus;
            return BadRequest(output);
        }

        /// <summary>
        /// Update Person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="profile">profile object of the parameter values</param>
        /// <returns>Response with Success Message</returns>
        [HttpPut("profiles/{realPageId}")]
        [AuthorizeRight("editotherprofile", "editownprofile")]
        [ProducesResponseType(typeof(ObjectOutput<IProfile, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProfile(Guid realPageId, [FromBody] Profile profile, CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var output = new ObjectOutput<IProfile, IErrorData>();
            var errorStatus = new Status<IErrorData>();
            output.obj = profile;

            realPageId = ((realPageId == Guid.Empty) || (realPageId == null)) ? userClaim.UserRealPageGuid : realPageId;
            if ((realPageId == Guid.Empty) || (realPageId == null))
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Profile.UpdateProfile.1";
                errorStatus.ErrorMsg = "Update Profile: Invalid parameter realPageId";
                output.Status = errorStatus;
                return Ok(output);
            }

            if (profile == null)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Profile.UpdateProfile.2";
                errorStatus.ErrorMsg = "Update Profile: Invalid parameter Profile";
                output.Status = errorStatus;
                return Ok(output);
            }
            else if (profile.IsFirstNameNullOrWhiteSpace)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Profile.UpdateProfile.4";
                errorStatus.ErrorMsg = "First name is required.";
                output.Status = errorStatus;
                return Ok(output);
            }
            else if (profile.IsLastNameNullOrWhiteSpace)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Profile.UpdateProfile.5";
                errorStatus.ErrorMsg = "Last name is required.";
                output.Status = errorStatus;
                return Ok(output);
            }

            var repositoryResponse = await _manageProfileAsync.UpdateProfileAsync(realPageId, profile, cancellationToken);
            if (repositoryResponse.Id == 0)
            {
                output.obj = profile;
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Profile.UpdateProfile.3";
                errorStatus.ErrorMsg = repositoryResponse.ErrorMessage;
                output.Status = errorStatus;
                return Ok(output);
            }

            output.Status = errorStatus;
            return Ok(output);
        }

        /// <summary>
        /// Get a user Profile detail
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <returns>ProfileDetail object</returns>
        [HttpGet("profiles/details")]
        [ProducesResponseType(typeof(ObjectOutput<IProfileDetail, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfileDetail([FromQuery] Guid? realPageId = null, CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var resolvedId = (realPageId == Guid.Empty || realPageId == null) ? userClaim.UserRealPageGuid : realPageId.Value;

            var profileDetail = await _manageProfileAsync.GetProfileDetailAsync(realPageId: resolvedId, orgPartyId: userClaim.OrganizationPartyId, cancellationToken: cancellationToken);
            var output = new ObjectOutput<IProfileDetail, IErrorData>() { obj = profileDetail };
            return Ok(output);
        }
    }
}
