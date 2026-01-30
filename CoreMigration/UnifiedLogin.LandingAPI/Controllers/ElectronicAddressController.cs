using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
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
    public class ElectronicAddressController : ControllerBase
    {
        private readonly IElectronicAddressRepository _electronicAddressRepository;
        private readonly IUserClaimsAccessor _userClaimsAccessor;
        private readonly IManageContactMechanism _manageContactMechanism;
        private readonly IManageElectronicAddress _manageElectronicAddress;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ElectronicAddressController(
            IElectronicAddressRepository electronicAddressRepository,
            IUserClaimsAccessor userClaimsAccessor,
            IManageContactMechanism manageContactMechanism,
            IManageElectronicAddress manageElectronicAddress)
        {
            _electronicAddressRepository = electronicAddressRepository;
            _userClaimsAccessor = userClaimsAccessor;
            _manageContactMechanism = manageContactMechanism;
            _manageElectronicAddress = manageElectronicAddress;
        }

        /// <summary>
        /// Link an Electronic Address to a person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="linkElectronicAddress">Person's Electronic Address parameter values</param>
        /// <returns>Response with Success Message</returns>
        [HttpPost("persons/{realPageId}/electronicaddress")]
        [ProducesResponseType(typeof(ElectronicAddress.ElectronicAddressOutputResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LinkElectronicAddress(Guid realPageId, [FromBody] LinkElectronicAddress linkElectronicAddress)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;

            realPageId = (realPageId == Guid.Empty) ? realPageUserId : realPageId;
            if (realPageId == Guid.Empty)
            {
                return BadRequest("Invalid parameter: realPageId");
            }

            if (linkElectronicAddress == null)
            {
                return BadRequest("Null parameter: linkElectronicAddress.");
            }

            var result = await Task.Run(() =>
            {
                // Create the Contact Mechanism
                var repositoryResponse = _manageContactMechanism.CreateContactMechanism();
                if (repositoryResponse.Id == 0)
                {
                    return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                }
                int contactMechanismId = Convert.ToInt32(repositoryResponse.Id);

                // Associate the Contact Mechanism to a Party
                IPartyContactMechanism partyContactMechanism = linkElectronicAddress.PartyContactMechanism;
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
                    linkElectronicAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId);
                if (repositoryResponse.Id == 0)
                {
                    return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                }

                // Add an ElectronicAddress and link it to a person
                IElectronicAddress electronicAddress = linkElectronicAddress.ElectronicAddress;
                electronicAddress.ContactMechanismId = contactMechanismId;
                repositoryResponse = _manageElectronicAddress.CreateElectronicAddress(electronicAddress);
                if (repositoryResponse.Id == 0)
                {
                    return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                }

                return new { Success = true, Error = string.Empty, ContactMechanismId = contactMechanismId };
            });

            if (!result.Success)
            {
                return BadRequest(result.Error);
            }

            var outputResult = new ElectronicAddress.ElectronicAddressOutputResult
            {
                ContactMechanismId = result.ContactMechanismId
            };

            return Ok(outputResult);
        }

        /// <summary>
        /// Update an Electronic Address to a person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="linkElectronicAddress">Person's Electronic Address parameter values</param>
        /// <returns>Response with Success Message</returns>
        [HttpPut("persons/{realPageId}/electronicaddress")]
        [ProducesResponseType(typeof(ElectronicAddress.ElectronicAddressOutputResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateElectronicAddress(Guid realPageId, [FromBody] LinkElectronicAddress linkElectronicAddress)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;

            realPageId = (realPageId == Guid.Empty) ? realPageUserId : realPageId;
            if (realPageId == Guid.Empty)
            {
                return BadRequest("Invalid parameter: realPageId");
            }

            if (linkElectronicAddress == null)
            {
                return BadRequest("Null parameter: linkElectronicAddress.");
            }

            var result = await Task.Run(() =>
            {
                int contactMechanismId = 0;
                if (linkElectronicAddress.PartyContactMechanism != null)
                {
                    contactMechanismId = linkElectronicAddress.PartyContactMechanism.ContactMechanismId;
                }

                var repositoryResponse = _manageContactMechanism.UpdateContactMechanismUsageForParty(
                    linkElectronicAddress.PartyContactMechanism.PartyContactMechanismId,
                    linkElectronicAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId);
                if (repositoryResponse.Id == 0)
                {
                    return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                }

                // Add an ElectronicAddress and link it to a person
                IElectronicAddress electronicAddress = linkElectronicAddress.ElectronicAddress;
                electronicAddress.ContactMechanismId = contactMechanismId;
                repositoryResponse = _manageElectronicAddress.CreateElectronicAddress(electronicAddress);
                if (repositoryResponse.Id == 0)
                {
                    return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                }

                return new { Success = true, Error = string.Empty, ContactMechanismId = contactMechanismId };
            });

            if (!result.Success)
            {
                return BadRequest(result.Error);
            }

            var outputResult = new ElectronicAddress.ElectronicAddressOutputResult
            {
                ContactMechanismId = result.ContactMechanismId
            };

            return Ok(outputResult);
        }

        /// <summary>
        /// List Electronic Address details for a Person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
        /// <returns>A list of Electronic Address Details for a person</returns>
        [HttpGet("persons/{realPageId}/electronicaddress")]
        [ProducesResponseType(typeof(ObjectListOutput<ElectronicAddress, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListElectronicAddressForPerson(Guid realPageId, string ContactMechanismUsageTypeName = "")
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;

            realPageId = (realPageId == Guid.Empty) ? realPageUserId : realPageId;
            if (realPageId == Guid.Empty)
            {
                return BadRequest("Invalid parameter: realPageId");
            }

            var electronicAddressList = await Task.Run(() =>
                _electronicAddressRepository.ListElectronicAddressForPerson(realPageId, ContactMechanismUsageTypeName));

            if (electronicAddressList != null && electronicAddressList.Any())
            {
                ObjectListOutput<ElectronicAddress, IErrorData> output = new ObjectListOutput<ElectronicAddress, IErrorData>
                {
                    list = electronicAddressList
                };
                return Ok(output);
            }

            // When trying to get a list of electronic address for a Person that doesn't exist
            return NoContent();
        }
    }
}
