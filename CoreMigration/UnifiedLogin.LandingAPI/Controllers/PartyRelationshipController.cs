using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Party Relationship Controller
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class PartyRelationshipController : BaseController
    {
        private readonly IPartyRelationshipRepositoryAsync _partyRelationshipRepository;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public PartyRelationshipController(
            IPartyRelationshipRepositoryAsync partyRelationshipRepository,
            IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
            _partyRelationshipRepository = partyRelationshipRepository ?? throw new ArgumentNullException(nameof(partyRelationshipRepository));
        }

        /// <summary>
        /// Link a Organization to an Organization
        /// </summary>
        /// <param name="RealPageIdFrom">From Organization unique identifier</param>
        /// <param name="partyRelationship">To Party Unique Identifier and From/To Relationship RoleType</param>
        /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
        /// <returns>Response with Success Message</returns>
        [HttpPost("organizations/{RealPageIdFrom}/relationships/organizations")]
        [ProducesResponseType(typeof(PartyRelationship.PartyRelationshipOutputResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LinkOrganizationToOrganization(
            Guid RealPageIdFrom,
            [FromBody] PartyRelationship partyRelationship,
            CancellationToken cancellationToken = default)
        {
            if (RealPageIdFrom == Guid.Empty)
            {
                return BadRequest("Invalid parameter: RealPageIdFrom.");
            }

            if (partyRelationship == null)
            {
                return BadRequest("Null parameter: partyRelationship.");
            }

            if (partyRelationship.RealPageIdTo == Guid.Empty)
            {
                return BadRequest("Invalid parameter: RealPageIdTo.");
            }

            if (partyRelationship.RoleTypeIdFrom <= 0)
            {
                return BadRequest("Invalid parameter: RoleTypeIdFrom.");
            }

            if (partyRelationship.RoleTypeIdTo <= 0)
            {
                return BadRequest("Invalid parameter: RoleTypeIdTo.");
            }

            var repositoryResponse = await _partyRelationshipRepository
                .LinkOrganizationToOrganizationAsync(RealPageIdFrom, partyRelationship, cancellationToken);

            if (repositoryResponse.Id == 0)
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }

            return Ok(new PartyRelationship.PartyRelationshipOutputResult
            {
                NewPartyRelationshipId = repositoryResponse.Id
            });
        }

        /// <summary>
        /// Link a Person to an Organization
        /// </summary>
        /// <param name="RealPageIdFrom">Person unique identifier</param>
        /// <param name="partyRelationship">To Party Unique Identifier and From/To Relationship RoleType</param>
        /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
        /// <returns>Response with Success Message</returns>
        [HttpPost("persons/{RealPageIdFrom}/relationships/organizations")]
        [ProducesResponseType(typeof(PartyRelationship.PartyRelationshipOutputResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LinkPersonToOrganization(
            Guid RealPageIdFrom,
            [FromBody] PartyRelationship partyRelationship,
            CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;

            RealPageIdFrom = (RealPageIdFrom == Guid.Empty) ? realPageUserId : RealPageIdFrom;
            if (RealPageIdFrom == Guid.Empty)
            {
                return BadRequest("Invalid parameter: PersonRealPageId.");
            }

            if (partyRelationship == null)
            {
                return BadRequest("Null parameter: partyRelationship.");
            }

            if (partyRelationship.RealPageIdTo == Guid.Empty)
            {
                return BadRequest("Invalid parameter: RealPageIdTo");
            }

            if (partyRelationship.RoleTypeIdFrom <= 0)
            {
                return BadRequest("Invalid parameter: RoleTypeIdFrom.");
            }

            if (partyRelationship.RoleTypeIdTo <= 0)
            {
                return BadRequest("Invalid parameter: RoleTypeIdTo.");
            }

            var repositoryResponse = await _partyRelationshipRepository
                .LinkPersonToOrganizationAsync(RealPageIdFrom, partyRelationship, cancellationToken);

            if (repositoryResponse.Id == 0)
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }

            return Ok(new PartyRelationship.PartyRelationshipOutputResult
            {
                NewPartyRelationshipId = repositoryResponse.Id
            });
        }
    }
}
