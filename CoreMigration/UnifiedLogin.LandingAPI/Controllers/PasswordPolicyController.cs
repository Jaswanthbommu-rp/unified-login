using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Password Policy Controller
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class PasswordPolicyController : BaseController
    {
        private readonly IManagePasswordPolicyAsync _managePasswordPolicy;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public PasswordPolicyController(
            IManagePasswordPolicyAsync managePasswordPolicy,
            IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
            _managePasswordPolicy = managePasswordPolicy ?? throw new ArgumentNullException(nameof(managePasswordPolicy));
        }

        /// <summary>
        /// Create a Password Policy
        /// </summary>
        /// <param name="passwordPolicy">Password Policy object of the parameter values</param>
        /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
        /// <returns>Created password policy ID</returns>
        [HttpPost("passwordpolicies")]
        [ProducesResponseType(typeof(PasswordPolicy.PasswordPolicyOutputResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePasswordPolicy(
            [FromBody] PasswordPolicy passwordPolicy,
            CancellationToken cancellationToken = default)
        {
            if (passwordPolicy == null)
            {
                return BadRequest("Null parameter: Password Policy.");
            }

            var userClaim = _userClaimsAccessor.GetUserClaim();
            passwordPolicy.UserId = userClaim?.UserId ?? 0;

            var repositoryResponse = await _managePasswordPolicy.CreatePasswordPolicyAsync(passwordPolicy, cancellationToken);

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
        /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
        /// <returns>A list of Password Polic(y|ies) Details</returns>
        [HttpGet("passwordpolicies/{PartyId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ObjectOutput<IPasswordPolicy, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPasswordPolicy(
            long PartyId,
            CancellationToken cancellationToken = default)
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

            var passwordPolicy = await _managePasswordPolicy.GetPasswordPolicyAsync(PartyId, cancellationToken);

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
        /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
        /// <returns>Updated password policy</returns>
        [HttpPut("passwordpolicies")]
        [ProducesResponseType(typeof(PasswordPolicy), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePasswordPolicy(
            [FromBody] PasswordPolicy passwordPolicy,
            CancellationToken cancellationToken = default)
        {
            if (passwordPolicy == null)
            {
                return BadRequest("Null parameter: Password Policy.");
            }

            var userClaim = _userClaimsAccessor.GetUserClaim();
            passwordPolicy.UserId = userClaim?.UserId ?? 0;

            var repositoryResponse = await _managePasswordPolicy.UpdatePasswordPolicyAsync(passwordPolicy, cancellationToken);

            if (repositoryResponse.Id == 0)
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }

            return Ok(passwordPolicy);
        }
    }
}
