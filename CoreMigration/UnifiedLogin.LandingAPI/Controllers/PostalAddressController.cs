using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
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
    public class PostalAddressController : ControllerBase
    {
        private readonly IPostalAddressRepository _postalAddressRepository;
        private readonly IManageContactMechanism _manageContactMechanism;
        private readonly IManageStreetAddress _manageStreetAddress;
        private readonly IManageGeographicBoundary _manageGeographicBoundary;
        private readonly IManagePostalAddress _managePostalAddress;
        private readonly IUserClaimsAccessor _userClaimsAccessor;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public PostalAddressController(
            IPostalAddressRepository postalAddressRepository,
            IManageContactMechanism manageContactMechanism,
            IManageStreetAddress manageStreetAddress,
            IManageGeographicBoundary manageGeographicBoundary,
            IManagePostalAddress managePostalAddress,
            IUserClaimsAccessor userClaimsAccessor)
        {
            _postalAddressRepository = postalAddressRepository;
            _manageContactMechanism = manageContactMechanism;
            _manageStreetAddress = manageStreetAddress;
            _manageGeographicBoundary = manageGeographicBoundary;
            _managePostalAddress = managePostalAddress;
            _userClaimsAccessor = userClaimsAccessor;
        }

        /// <summary>
        /// Link an Postal Address to a person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="linkPostalAddress">Person's Postal Address parameter values</param>
        /// <returns>Response with Success Message</returns>
        [HttpPost("persons/{realPageId}/postaladdress")]
        [ProducesResponseType(typeof(PostalAddress.PostalAddressOutputResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LinkPostalAddress(Guid realPageId, [FromBody] LinkPostalAddress linkPostalAddress)
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

            var result = await Task.Run(() =>
            {
                // Add an PostalAddress and link it to a person
                // Create the Contact Mechanism
                var repositoryResponse = _manageContactMechanism.CreateContactMechanism();
                if (repositoryResponse.Id == 0)
                {
                    return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                }
                int contactMechanismId = Convert.ToInt32(repositoryResponse.Id);

                // Associate the Contact Mechanism to a Party
                IPartyContactMechanism partyContactMechanism = linkPostalAddress.PartyContactMechanism;
                partyContactMechanism.ContactMechanismId = contactMechanismId;
                repositoryResponse = _manageContactMechanism.LinkContactMechanismToParty(realPageId, partyContactMechanism);
                if (repositoryResponse.Id == 0)
                {
                    return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                }

                // Assign a usage type to the Contact Mechanism
                partyContactMechanism.PartyContactMechanismId = repositoryResponse.Id;
                repositoryResponse = _manageContactMechanism.LinkUsageTypeToPartyContactMechanism(
                    partyContactMechanism.PartyContactMechanismId,
                    linkPostalAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId);
                if (repositoryResponse.Id == 0)
                {
                    return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                }

                // Create the Street Address
                IStreetAddress streetAddress = linkPostalAddress.StreetAddress;
                streetAddress.ContactMechanismId = contactMechanismId;
                repositoryResponse = _manageStreetAddress.CreateStreetAddress(streetAddress);
                if (repositoryResponse.Id == 0)
                {
                    return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                }

                // Create Geographic Boundaries and link them to Contact Mechanism
                IContactMechanismBoundary contactMechanismBoundry = linkPostalAddress.ContactMechanismBoundary;
                contactMechanismBoundry.ContactMechanismId = contactMechanismId;

                foreach (var geographicBoundary in linkPostalAddress.GeographicBoundary)
                {
                    repositoryResponse = _manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary);
                    if (repositoryResponse.Id == 0)
                    {
                        return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                    }

                    contactMechanismBoundry.GeographicBoundaryId = Convert.ToInt32(repositoryResponse.Id);
                    repositoryResponse = _manageContactMechanism.LinkGeographicBoundaryToContactMechanism(contactMechanismBoundry);
                    if (repositoryResponse.Id == 0)
                    {
                        return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                    }
                }

                return new { Success = true, Error = string.Empty, ContactMechanismId = contactMechanismId };
            });

            if (!result.Success)
            {
                return BadRequest(result.Error);
            }

            var outputResult = new PostalAddress.PostalAddressOutputResult
            {
                ContactMechanismId = result.ContactMechanismId
            };

            return Ok(outputResult);
        }

        /// <summary>
        /// Update an Postal Address to a person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="linkPostalAddress">Person's Postal Address parameter values</param>
        /// <returns>Response with Success Message</returns>
        [HttpPut("persons/{realPageId}/postaladdress")]
        [ProducesResponseType(typeof(PostalAddress.PostalAddressOutputResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePostalAddress(Guid realPageId, [FromBody] LinkPostalAddress linkPostalAddress)
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

            var result = await Task.Run(() =>
            {
                int contactMechanismId = 0;
                if (linkPostalAddress.PartyContactMechanism != null)
                {
                    contactMechanismId = linkPostalAddress.PartyContactMechanism.ContactMechanismId;
                }

                // Expire existing associated Contact Mechanism to a Party
                IPartyContactMechanism partyContactMechanism = linkPostalAddress.PartyContactMechanism;
                partyContactMechanism.ContactMechanismId = contactMechanismId;
                var repositoryResponse = _manageContactMechanism.LinkContactMechanismToParty(realPageId, partyContactMechanism);
                if (repositoryResponse.Id == 0)
                {
                    return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                }

                // Add an PostalAddress and link it to a person
                // Create the Contact Mechanism
                repositoryResponse = _manageContactMechanism.CreateContactMechanism();
                if (repositoryResponse.Id == 0)
                {
                    return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                }
                contactMechanismId = Convert.ToInt32(repositoryResponse.Id);

                // Associate the new Contact Mechanism to a Party
                linkPostalAddress.PartyContactMechanism.PartyContactMechanismId = 0;
                partyContactMechanism = linkPostalAddress.PartyContactMechanism;
                partyContactMechanism.ContactMechanismId = contactMechanismId;
                repositoryResponse = _manageContactMechanism.LinkContactMechanismToParty(realPageId, partyContactMechanism);
                if (repositoryResponse.Id == 0)
                {
                    return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                }

                // Assign a usage type to the Contact Mechanism
                partyContactMechanism.PartyContactMechanismId = repositoryResponse.Id;
                repositoryResponse = _manageContactMechanism.LinkUsageTypeToPartyContactMechanism(
                    partyContactMechanism.PartyContactMechanismId,
                    linkPostalAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId);
                if (repositoryResponse.Id == 0)
                {
                    return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                }

                // Create the Street Address
                IStreetAddress streetAddress = linkPostalAddress.StreetAddress;
                streetAddress.ContactMechanismId = contactMechanismId;
                repositoryResponse = _manageStreetAddress.CreateStreetAddress(streetAddress);
                if (repositoryResponse.Id == 0)
                {
                    return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                }

                // Create Geographic Boundaries and link them to Contact Mechanism
                IContactMechanismBoundary contactMechanismBoundry = linkPostalAddress.ContactMechanismBoundary;
                contactMechanismBoundry.ContactMechanismId = contactMechanismId;

                foreach (var geographicBoundary in linkPostalAddress.GeographicBoundary)
                {
                    repositoryResponse = _manageGeographicBoundary.CreateGeographicBoundary(geographicBoundary);
                    if (repositoryResponse.Id == 0)
                    {
                        return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                    }

                    contactMechanismBoundry.GeographicBoundaryId = Convert.ToInt32(repositoryResponse.Id);
                    repositoryResponse = _manageContactMechanism.LinkGeographicBoundaryToContactMechanism(contactMechanismBoundry);
                    if (repositoryResponse.Id == 0)
                    {
                        return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                    }
                }

                return new { Success = true, Error = string.Empty, ContactMechanismId = contactMechanismId };
            });

            if (!result.Success)
            {
                return BadRequest(result.Error);
            }

            var outputResult = new PostalAddress.PostalAddressOutputResult
            {
                ContactMechanismId = result.ContactMechanismId
            };

            return Ok(outputResult);
        }

        /// <summary>
        /// List Postal Address details for a Person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
        /// <returns>A list of Postal Address Details for a person</returns>
        [HttpGet("persons/{realPageId}/postaladdress")]
        [ProducesResponseType(typeof(ObjectListOutput<PostalAddress, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListPostalAddressForPerson(Guid realPageId, string ContactMechanismUsageTypeName = "")
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;

            realPageId = (realPageId == Guid.Empty) ? realPageUserId : realPageId;
            if (realPageId == Guid.Empty)
            {
                return BadRequest("Invalid parameter: realPageId");
            }

            var postalAddressList = await Task.Run(() =>
                _postalAddressRepository.ListPostalAddressForPerson(realPageId, ContactMechanismUsageTypeName));

            if (postalAddressList != null && postalAddressList.Any())
            {
                ObjectListOutput<PostalAddress, IErrorData> output = new ObjectListOutput<PostalAddress, IErrorData>
                {
                    list = postalAddressList
                };
                return Ok(output);
            }

            // When trying to get a list of postal address for a Person that doesn't exist
            return NoContent();
        }
    }
}
