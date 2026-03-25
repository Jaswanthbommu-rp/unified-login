using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Postal Address Controller to hold all postal address management related APIs
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class PostalAddressController : BaseController
    {
        private readonly IManageContactMechanismAsync _manageContactMechanism;
        private readonly IManageStreetAddressAsync _manageStreetAddress;
        private readonly IManageGeographicBoundaryAsync _manageGeographicBoundary;
        private readonly IManagePostalAddressAsync _managePostalAddress;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public PostalAddressController(
            IManageContactMechanismAsync manageContactMechanism,
            IManageStreetAddressAsync manageStreetAddress,
            IManageGeographicBoundaryAsync manageGeographicBoundary,
            IManagePostalAddressAsync managePostalAddress,
            IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
            _manageContactMechanism = manageContactMechanism ?? throw new ArgumentNullException(nameof(manageContactMechanism));
            _manageStreetAddress = manageStreetAddress ?? throw new ArgumentNullException(nameof(manageStreetAddress));
            _manageGeographicBoundary = manageGeographicBoundary ?? throw new ArgumentNullException(nameof(manageGeographicBoundary));
            _managePostalAddress = managePostalAddress ?? throw new ArgumentNullException(nameof(managePostalAddress));
        }

        /// <summary>
        /// Link a Postal Address to a person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="linkPostalAddress">Person's Postal Address parameter values</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response with Success Message</returns>
        [HttpPost("persons/{realPageId}/postaladdress")]
        [ProducesResponseType(typeof(PostalAddress.PostalAddressOutputResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LinkPostalAddress(Guid realPageId, [FromBody] LinkPostalAddress linkPostalAddress, CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;

            realPageId = (realPageId == Guid.Empty) ? realPageUserId : realPageId;
            if (realPageId == Guid.Empty)
            {
                return BadRequest("Invalid parameter: realPageId");
            }

            if (linkPostalAddress == null)
            {
                return BadRequest("Null parameter: linkPostalAddress.");
            }

            // Create the Contact Mechanism
            var repositoryResponse = await _manageContactMechanism.CreateContactMechanismAsync(cancellationToken);
            if (repositoryResponse.Id == 0)
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }
            int contactMechanismId = Convert.ToInt32(repositoryResponse.Id);

            // Associate the Contact Mechanism to a Party
            IPartyContactMechanism partyContactMechanism = linkPostalAddress.PartyContactMechanism;
            partyContactMechanism.ContactMechanismId = contactMechanismId;
            repositoryResponse = await _manageContactMechanism.LinkContactMechanismToPartyAsync(realPageId, partyContactMechanism, cancellationToken);
            if (repositoryResponse.Id == 0)
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }

            // Assign a usage type to the Contact Mechanism
            partyContactMechanism.PartyContactMechanismId = repositoryResponse.Id;
            repositoryResponse = await _manageContactMechanism.LinkUsageTypeToPartyContactMechanismAsync(
                partyContactMechanism.PartyContactMechanismId,
                linkPostalAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId,
                cancellationToken);
            if (repositoryResponse.Id == 0)
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }

            // Create the Street Address
            IStreetAddress streetAddress = linkPostalAddress.StreetAddress;
            streetAddress.ContactMechanismId = contactMechanismId;
            repositoryResponse = await _manageStreetAddress.CreateStreetAddressAsync(streetAddress, cancellationToken);
            if (repositoryResponse.Id == 0)
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }

            // Create Geographic Boundaries and link them to Contact Mechanism
            IContactMechanismBoundary contactMechanismBoundary = linkPostalAddress.ContactMechanismBoundary;
            contactMechanismBoundary.ContactMechanismId = contactMechanismId;

            foreach (var geographicBoundary in linkPostalAddress.GeographicBoundary)
            {
                repositoryResponse = await _manageGeographicBoundary.CreateGeographicBoundaryAsync(geographicBoundary, cancellationToken);
                if (repositoryResponse.Id == 0)
                {
                    return BadRequest(repositoryResponse.ErrorMessage);
                }

                contactMechanismBoundary.GeographicBoundaryId = Convert.ToInt32(repositoryResponse.Id);
                repositoryResponse = await _manageContactMechanism.LinkGeographicBoundaryToContactMechanismAsync(contactMechanismBoundary, cancellationToken);
                if (repositoryResponse.Id == 0)
                {
                    return BadRequest(repositoryResponse.ErrorMessage);
                }
            }

            return Ok(new PostalAddress.PostalAddressOutputResult { ContactMechanismId = contactMechanismId });
        }

        /// <summary>
        /// Update a Postal Address for a person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="linkPostalAddress">Person's Postal Address parameter values</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response with Success Message</returns>
        [HttpPut("persons/{realPageId}/postaladdress")]
        [ProducesResponseType(typeof(PostalAddress.PostalAddressOutputResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePostalAddress(Guid realPageId, [FromBody] LinkPostalAddress linkPostalAddress, CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;

            realPageId = (realPageId == Guid.Empty) ? realPageUserId : realPageId;
            if (realPageId == Guid.Empty)
            {
                return BadRequest("Invalid parameter: realPageId");
            }

            if (linkPostalAddress == null)
            {
                return BadRequest("Null parameter: linkPostalAddress.");
            }

            int contactMechanismId = linkPostalAddress.PartyContactMechanism?.ContactMechanismId ?? 0;

            // Expire existing associated Contact Mechanism to a Party
            IPartyContactMechanism partyContactMechanism = linkPostalAddress.PartyContactMechanism;
            partyContactMechanism.ContactMechanismId = contactMechanismId;
            var repositoryResponse = await _manageContactMechanism.LinkContactMechanismToPartyAsync(realPageId, partyContactMechanism, cancellationToken);
            if (repositoryResponse.Id == 0)
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }

            // Create the new Contact Mechanism
            repositoryResponse = await _manageContactMechanism.CreateContactMechanismAsync(cancellationToken);
            if (repositoryResponse.Id == 0)
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }
            contactMechanismId = Convert.ToInt32(repositoryResponse.Id);

            // Associate the new Contact Mechanism to a Party
            linkPostalAddress.PartyContactMechanism.PartyContactMechanismId = 0;
            partyContactMechanism = linkPostalAddress.PartyContactMechanism;
            partyContactMechanism.ContactMechanismId = contactMechanismId;
            repositoryResponse = await _manageContactMechanism.LinkContactMechanismToPartyAsync(realPageId, partyContactMechanism, cancellationToken);
            if (repositoryResponse.Id == 0)
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }

            // Assign a usage type to the Contact Mechanism
            partyContactMechanism.PartyContactMechanismId = repositoryResponse.Id;
            repositoryResponse = await _manageContactMechanism.LinkUsageTypeToPartyContactMechanismAsync(
                partyContactMechanism.PartyContactMechanismId,
                linkPostalAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId,
                cancellationToken);
            if (repositoryResponse.Id == 0)
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }

            // Create the Street Address
            IStreetAddress streetAddress = linkPostalAddress.StreetAddress;
            streetAddress.ContactMechanismId = contactMechanismId;
            repositoryResponse = await _manageStreetAddress.CreateStreetAddressAsync(streetAddress, cancellationToken);
            if (repositoryResponse.Id == 0)
            {
                return BadRequest(repositoryResponse.ErrorMessage);
            }

            // Create Geographic Boundaries and link them to Contact Mechanism
            IContactMechanismBoundary contactMechanismBoundary = linkPostalAddress.ContactMechanismBoundary;
            contactMechanismBoundary.ContactMechanismId = contactMechanismId;

            foreach (var geographicBoundary in linkPostalAddress.GeographicBoundary)
            {
                repositoryResponse = await _manageGeographicBoundary.CreateGeographicBoundaryAsync(geographicBoundary, cancellationToken);
                if (repositoryResponse.Id == 0)
                {
                    return BadRequest(repositoryResponse.ErrorMessage);
                }

                contactMechanismBoundary.GeographicBoundaryId = Convert.ToInt32(repositoryResponse.Id);
                repositoryResponse = await _manageContactMechanism.LinkGeographicBoundaryToContactMechanismAsync(contactMechanismBoundary, cancellationToken);
                if (repositoryResponse.Id == 0)
                {
                    return BadRequest(repositoryResponse.ErrorMessage);
                }
            }

            return Ok(new PostalAddress.PostalAddressOutputResult { ContactMechanismId = contactMechanismId });
        }

        /// <summary>
        /// List Postal Address details for a Person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of Postal Address Details for a person</returns>
        [HttpGet("persons/{realPageId}/postaladdress")]
        [ProducesResponseType(typeof(ObjectListOutput<PostalAddress, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListPostalAddressForPerson(Guid realPageId, string ContactMechanismUsageTypeName = "", CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;

            realPageId = (realPageId == Guid.Empty) ? realPageUserId : realPageId;
            if (realPageId == Guid.Empty)
            {
                return BadRequest("Invalid parameter: realPageId");
            }

            var postalAddressList = await _managePostalAddress.ListPostalAddressForPersonAsync(realPageId, ContactMechanismUsageTypeName, cancellationToken);

            if (postalAddressList != null)
            {
                return Ok(new ObjectListOutput<PostalAddress, IErrorData>
                {
                    list = postalAddressList
                });
            }

            return NoContent();
        }
    }
}
