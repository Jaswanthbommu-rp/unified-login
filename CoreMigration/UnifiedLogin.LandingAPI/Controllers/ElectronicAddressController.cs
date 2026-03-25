using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Electronic Address Controller to hold all electronic address management related APIs
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class ElectronicAddressController : BaseController
    {
        private readonly IElectronicAddressRepositoryAsync _electronicAddressRepository;
        private readonly IContactMechanismRepositoryAsync _contactMechanismRepository;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ElectronicAddressController(
            IElectronicAddressRepositoryAsync electronicAddressRepository,
            IContactMechanismRepositoryAsync contactMechanismRepository,
            IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
            _electronicAddressRepository = electronicAddressRepository ?? throw new ArgumentNullException(nameof(electronicAddressRepository));
            _contactMechanismRepository = contactMechanismRepository ?? throw new ArgumentNullException(nameof(contactMechanismRepository));
        }

        /// <summary>
        /// Link an Electronic Address to a person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="linkElectronicAddress">Person's Electronic Address parameter values</param>
        /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
        /// <returns>Response with Success Message</returns>
        [HttpPost("persons/{realPageId}/electronicaddress")]
        [ProducesResponseType(typeof(ElectronicAddress.ElectronicAddressOutputResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LinkElectronicAddress(Guid realPageId, [FromBody] LinkElectronicAddress linkElectronicAddress, CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;

            realPageId = (realPageId == Guid.Empty) ? realPageUserId : realPageId;
            if (realPageId == Guid.Empty)
                return BadRequest("Invalid parameter: realPageId");

            if (linkElectronicAddress == null)
                return BadRequest("Null parameter: linkElectronicAddress.");

            // Create the Contact Mechanism
            var repositoryResponse = await _contactMechanismRepository.CreateContactMechanismAsync(cancellationToken);
            if (repositoryResponse.Id == 0)
                return BadRequest(repositoryResponse.ErrorMessage);

            int contactMechanismId = Convert.ToInt32(repositoryResponse.Id);

            // Associate the Contact Mechanism to a Party
            IPartyContactMechanism partyContactMechanism = linkElectronicAddress.PartyContactMechanism;
            partyContactMechanism.ContactMechanismId = contactMechanismId;
            repositoryResponse = await _contactMechanismRepository.LinkContactMechanismToPartyAsync(realPageId, partyContactMechanism, cancellationToken);
            if (repositoryResponse.Id == 0)
                return BadRequest(repositoryResponse.ErrorMessage);

            // Assign a usage type to the Contact Mechanism
            partyContactMechanism.PartyContactMechanismId = repositoryResponse.Id;
            repositoryResponse = await _contactMechanismRepository.LinkUsageTypeToPartyContactMechanismAsync(
                partyContactMechanism.PartyContactMechanismId,
                linkElectronicAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId,
                cancellationToken);
            if (repositoryResponse.Id == 0)
                return BadRequest(repositoryResponse.ErrorMessage);

            // Add an ElectronicAddress and link it to a person
            IElectronicAddress electronicAddress = linkElectronicAddress.ElectronicAddress;
            electronicAddress.ContactMechanismId = contactMechanismId;
            repositoryResponse = await _electronicAddressRepository.CreateElectronicAddressAsync(electronicAddress, cancellationToken);
            if (repositoryResponse.Id == 0)
                return BadRequest(repositoryResponse.ErrorMessage);

            return Ok(new ElectronicAddress.ElectronicAddressOutputResult { ContactMechanismId = contactMechanismId });
        }

        /// <summary>
        /// Update an Electronic Address to a person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="linkElectronicAddress">Person's Electronic Address parameter values</param>
        /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
        /// <returns>Response with Success Message</returns>
        [HttpPut("persons/{realPageId}/electronicaddress")]
        [ProducesResponseType(typeof(ElectronicAddress.ElectronicAddressOutputResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateElectronicAddress(Guid realPageId, [FromBody] LinkElectronicAddress linkElectronicAddress, CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;

            realPageId = (realPageId == Guid.Empty) ? realPageUserId : realPageId;
            if (realPageId == Guid.Empty)
                return BadRequest("Invalid parameter: realPageId");

            if (linkElectronicAddress == null)
                return BadRequest("Null parameter: linkElectronicAddress.");

            int contactMechanismId = linkElectronicAddress.PartyContactMechanism?.ContactMechanismId ?? 0;

            var repositoryResponse = await _contactMechanismRepository.UpdateContactMechanismUsageForPartyAsync(
                linkElectronicAddress.PartyContactMechanism.PartyContactMechanismId,
                linkElectronicAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId,
                cancellationToken);
            if (repositoryResponse.Id == 0)
                return BadRequest(repositoryResponse.ErrorMessage);

            // Add an ElectronicAddress and link it to a person
            IElectronicAddress electronicAddress = linkElectronicAddress.ElectronicAddress;
            electronicAddress.ContactMechanismId = contactMechanismId;
            repositoryResponse = await _electronicAddressRepository.CreateElectronicAddressAsync(electronicAddress, cancellationToken);
            if (repositoryResponse.Id == 0)
                return BadRequest(repositoryResponse.ErrorMessage);

            return Ok(new ElectronicAddress.ElectronicAddressOutputResult { ContactMechanismId = contactMechanismId });
        }

        /// <summary>
        /// List Electronic Address details for a Person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
        /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
        /// <returns>A list of Electronic Address Details for a person</returns>
        [HttpGet("persons/{realPageId}/electronicaddress")]
        [ProducesResponseType(typeof(ObjectListOutput<ElectronicAddress, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListElectronicAddressForPerson(Guid realPageId, string ContactMechanismUsageTypeName = "", CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;

            realPageId = (realPageId == Guid.Empty) ? realPageUserId : realPageId;
            if (realPageId == Guid.Empty)
                return BadRequest("Invalid parameter: realPageId");

            var electronicAddressList = await _electronicAddressRepository.ListElectronicAddressForPersonAsync(realPageId, ContactMechanismUsageTypeName, cancellationToken);

            if (electronicAddressList != null && electronicAddressList.Any())
            {
                return Ok(new ObjectListOutput<ElectronicAddress, IErrorData> { list = electronicAddressList });
            }

            // When trying to get a list of electronic address for a Person that doesn't exist
            return NoContent();
        }
    }
}
