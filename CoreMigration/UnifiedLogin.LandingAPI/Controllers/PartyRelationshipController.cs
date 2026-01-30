using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
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
    public class PartyRelationshipController : ControllerBase
    {
        private readonly IPartyRelationshipRepository _partyRelationshipRepository;
        private readonly IUserClaimsAccessor _userClaimsAccessor;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public PartyRelationshipController(
            IPartyRelationshipRepository partyRelationshipRepository,
            IUserClaimsAccessor userClaimsAccessor)
        {
            _partyRelationshipRepository = partyRelationshipRepository;
            _userClaimsAccessor = userClaimsAccessor;
        }

        /// <summary>
        /// Link a Organization to an Organization
        /// </summary>
        /// <param name="RealPageIdFrom">From Organization unique identifier</param>
        /// <param name="partyRelationship">To Party Unique Identifier and From/To Relationship RoleType</param>
        /// <returns>Response with Success Message</returns>
        [HttpPost("organizations/{RealPageIdFrom}/relationships/organizations")]
        [ProducesResponseType(typeof(PartyRelationship.PartyRelationshipOutputResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LinkOrganizationToOrganization(Guid RealPageIdFrom, [FromBody] PartyRelationship partyRelationship)
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

            var repositoryResponse = await Task.Run(() =>
                _partyRelationshipRepository.LinkOrganizationToOrganization(RealPageIdFrom, partyRelationship));

            if (repositoryResponse.Id == 0)
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }

            PartyRelationship.PartyRelationshipOutputResult result = new PartyRelationship.PartyRelationshipOutputResult
            {
                NewPartyRelationshipId = repositoryResponse.Id
            };

            return Ok(result);
        }

        /// <summary>
        /// Link a Person to an Organization
        /// </summary>
        /// <param name="RealPageIdFrom">Person unique identifier</param>
        /// <param name="partyRelationship">To Party Unique Identifier and From/To Relationship RoleType</param>
        /// <returns>Response with Success Message</returns>
        [HttpPost("persons/{RealPageIdFrom}/relationships/organizations")]
        [ProducesResponseType(typeof(PartyRelationship.PartyRelationshipOutputResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LinkPersonToOrganization(Guid RealPageIdFrom, [FromBody] PartyRelationship partyRelationship)
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

            var repositoryResponse = await Task.Run(() =>
                _partyRelationshipRepository.LinkPersonToOrganization(RealPageIdFrom, partyRelationship));

            if (repositoryResponse.Id == 0)
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }

            PartyRelationship.PartyRelationshipOutputResult result = new PartyRelationship.PartyRelationshipOutputResult
            {
                NewPartyRelationshipId = repositoryResponse.Id
            };

            return Ok(result);
        }
    }
}
