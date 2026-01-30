using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Password Policy Controller
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class PasswordPolicyController : ControllerBase
    {
        private readonly IManagePasswordPolicy _managePasswordPolicy;
        private readonly IUserClaimsAccessor _userClaimsAccessor;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public PasswordPolicyController(
            IManagePasswordPolicy managePasswordPolicy,
            IUserClaimsAccessor userClaimsAccessor)
        {
            _managePasswordPolicy = managePasswordPolicy;
            _userClaimsAccessor = userClaimsAccessor;
        }

        /// <summary>
        /// Create a Password Policy
        /// </summary>
        /// <param name="passwordPolicy">Password Policy object of the parameter values</param>
        /// <returns>Created password policy ID</returns>
        [HttpPost("passwordpolicies")]
        [ProducesResponseType(typeof(PasswordPolicy.PasswordPolicyOutputResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePasswordPolicy([FromBody] PasswordPolicy passwordPolicy)
        {
            if (passwordPolicy == null)
            {
                return BadRequest("Null parameter: Password Policy.");
            }

            var userClaim = _userClaimsAccessor.GetUserClaim();
            passwordPolicy.UserId = userClaim?.UserId ?? 0;

            var repositoryResponse = await Task.Run(() => _managePasswordPolicy.CreatePasswordPolicy(passwordPolicy));

            if (repositoryResponse.Id == 0)
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }

            PasswordPolicy.PasswordPolicyOutputResult result = new PasswordPolicy.PasswordPolicyOutputResult
            {
                NewPasswordPolicyId = repositoryResponse.Id
            };

            return Ok(result);
        }

        /// <summary>
        /// Get/List Password Polic(y|ies) Details
        /// </summary>
        /// <param name="PartyId">Party ID (Organization ID)</param>
        /// <returns>A list of Password Polic(y|ies) Details</returns>
        [HttpGet("passwordpolicies/{PartyId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ObjectOutput<IPasswordPolicy, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPasswordPolicy(long PartyId)
        {
            ObjectOutput<IPasswordPolicy, IErrorData> output = new ObjectOutput<IPasswordPolicy, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            if (PartyId <= 0)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "PasswordPolicy.GetPasswordPolicy.1";
                errorStatus.ErrorMsg = "Invalid parameter: Company PartyId";
                output.Status = errorStatus;
                return Ok(output);
            }

            var passwordPolicy = await Task.Run(() => _managePasswordPolicy.GetPasswordPolicy(PartyId));

            if (passwordPolicy != null)
            {
                output.obj = passwordPolicy;
                output.Status = errorStatus;
                return Ok(output);
            }

            // When trying to get a Password Policy that doesn't exist / deleted
            errorStatus.Success = false;
            errorStatus.ErrorCode = "PasswordPolicy.GetPasswordPolicy.2";
            errorStatus.ErrorMsg = "Get PasswordPolicy details: No data.";
            output.Status = errorStatus;
            return Ok(output);
        }

        /// <summary>
        /// Update Password Policy
        /// </summary>
        /// <param name="passwordPolicy">Password Policy object of the parameter values</param>
        /// <returns>Updated password policy</returns>
        [HttpPut("passwordpolicies")]
        [ProducesResponseType(typeof(PasswordPolicy), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePasswordPolicy([FromBody] PasswordPolicy passwordPolicy)
        {
            if (passwordPolicy == null)
            {
                return BadRequest("Null parameter: Password Policy.");
            }

            var userClaim = _userClaimsAccessor.GetUserClaim();
            passwordPolicy.UserId = userClaim?.UserId ?? 0;

            var repositoryResponse = await Task.Run(() => _managePasswordPolicy.UpdatePasswordPolicy(passwordPolicy));

            if (repositoryResponse.Id == 0)
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }

            return Ok(passwordPolicy);
        }
    }
}
