using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// TelecommunicationNumber Controller to hold all telecommunication number management related APIs
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class TelecommunicationNumberController : BaseController
    {

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public TelecommunicationNumberController(IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
        }

        /// <summary>
        /// Link a Telecommunication Number to a person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="linkTelecommunicationNumber">Person's Telecommunication Number parameter values</param>
        /// <returns>Response with Success Message</returns>
        [HttpPost("persons/{realPageId}/telecommunicationnumber")]
        [ProducesResponseType(typeof(TelecommunicationNumber.TelecommunicationNumberOutputResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LinkTelecommunicationNumber(Guid realPageId, [FromBody] LinkTelecommunicationNumber linkTelecommunicationNumber)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;

            realPageId = (realPageId == Guid.Empty) ? realPageUserId : realPageId;
            if (realPageId == Guid.Empty)
            {
                return BadRequest("Invalid parameter: realPageId");
            }

            if (linkTelecommunicationNumber == null)
            {
                return BadRequest("Null parameter: linkTelecommunicationNumber");
            }

            var result = await Task.Run(() =>
            {
                IManageContactMechanism contactMechanismLogic = new ManageContactMechanism();

                //Add an Telecommunication and link it to a person
                //Create the Contact Mechanism
                IRepositoryResponse repositoryResponse = contactMechanismLogic.CreateContactMechanism();
                if (repositoryResponse.Id == 0)
                {
                    return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                }
                int contactMechanismId = Convert.ToInt32(repositoryResponse.Id);

                //Associate the Contact Mechanism to a Party
                IPartyContactMechanism partyContactMechanism = linkTelecommunicationNumber.PartyContactMechanism;
                partyContactMechanism.ContactMechanismId = contactMechanismId;
                repositoryResponse = contactMechanismLogic.LinkContactMechanismToParty(realPageId, partyContactMechanism);
                if (repositoryResponse.Id == 0)
                {
                    return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                }

                //Assign a usage type to the Contact Mechanism
                partyContactMechanism.PartyContactMechanismId = repositoryResponse.Id;
                repositoryResponse = contactMechanismLogic.LinkUsageTypeToPartyContactMechanism(partyContactMechanism.PartyContactMechanismId, linkTelecommunicationNumber.ContactMechanismUsageType.ContactMechanismUsageTypeId);
                if (repositoryResponse.Id == 0)
                {
                    return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                }

                //Add/Update a TelecommunicationNumber and link it to a person
                IManageTelecommunicationNumber telecommunicationNumberLogic = new ManageTelecommunicationNumber();
                ITelecommunicationNumber telecommunicationNumber = new TelecommunicationNumber();
                telecommunicationNumber = linkTelecommunicationNumber.TelecommunicationNumber;
                telecommunicationNumber.ContactMechanismId = contactMechanismId;
                repositoryResponse = telecommunicationNumberLogic.CreateTelecommunicationNumber(telecommunicationNumber);
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

            var outputResult = new TelecommunicationNumber.TelecommunicationNumberOutputResult
            {
                ContactMechanismId = result.ContactMechanismId
            };

            return Ok(outputResult);
        }

        /// <summary>
        /// Update a Telecommunication Number to a person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="linkTelecommunicationNumber">Person's Telecommunication Number parameter values</param>
        /// <returns>Response with Success Message</returns>
        [HttpPut("persons/{realPageId}/telecommunicationnumber")]
        [ProducesResponseType(typeof(TelecommunicationNumber.TelecommunicationNumberOutputResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTelecommunicationNumber(Guid realPageId, [FromBody] LinkTelecommunicationNumber linkTelecommunicationNumber)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;

            realPageId = (realPageId == Guid.Empty) ? realPageUserId : realPageId;
            if (realPageId == Guid.Empty)
            {
                return BadRequest("Invalid parameter: realPageId");
            }

            if (linkTelecommunicationNumber == null)
            {
                return BadRequest("Null parameter: linkTelecommunicationNumber");
            }

            var result = await Task.Run(() =>
            {
                IManageContactMechanism contactMechanismLogic = new ManageContactMechanism();
                int contactMechanismId = 0;
                if (linkTelecommunicationNumber.PartyContactMechanism != null)
                {
                    contactMechanismId = linkTelecommunicationNumber.PartyContactMechanism.ContactMechanismId;
                }

                IRepositoryResponse repositoryResponse = contactMechanismLogic.UpdateContactMechanismUsageForParty(linkTelecommunicationNumber.PartyContactMechanism.PartyContactMechanismId, linkTelecommunicationNumber.ContactMechanismUsageType.ContactMechanismUsageTypeId);
                if (repositoryResponse.Id == 0)
                {
                    return new { Success = false, Error = repositoryResponse.ErrorMessage, ContactMechanismId = 0 };
                }

                //Add/Update a TelecommunicationNumber and link it to a person
                IManageTelecommunicationNumber telecommunicationNumberLogic = new ManageTelecommunicationNumber();
                ITelecommunicationNumber telecommunicationNumber = new TelecommunicationNumber();
                telecommunicationNumber = linkTelecommunicationNumber.TelecommunicationNumber;
                telecommunicationNumber.ContactMechanismId = contactMechanismId;
                repositoryResponse = telecommunicationNumberLogic.CreateTelecommunicationNumber(telecommunicationNumber);
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

            var outputResult = new TelecommunicationNumber.TelecommunicationNumberOutputResult
            {
                ContactMechanismId = result.ContactMechanismId
            };

            return Ok(outputResult);
        }

        /// <summary>
        /// List Telecommunication Number details for a Person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
        /// <returns>A list of Telecommunication Number Details for a person</returns>
        [HttpGet("persons/{realPageId}/telecommunicationnumber")]
        [ProducesResponseType(typeof(ObjectListOutput<TelecommunicationNumber, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListTelecommunicationNumberForPerson(Guid realPageId, string ContactMechanismUsageTypeName = "")
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;

            realPageId = (realPageId == Guid.Empty) ? realPageUserId : realPageId;
            if (realPageId == Guid.Empty)
            {
                return BadRequest("Invalid parameter: realPageId");
            }

            var telecommunicationNumberList = await Task.Run(() =>
            {
                IManageTelecommunicationNumber telecommunicationNumberLogic = new ManageTelecommunicationNumber();
                return telecommunicationNumberLogic.ListTelecommunicationNumberForPerson(realPageId, ContactMechanismUsageTypeName);
            });

            if (telecommunicationNumberList != null && telecommunicationNumberList.Any())
            {
                ObjectListOutput<TelecommunicationNumber, IErrorData> output = new ObjectListOutput<TelecommunicationNumber, IErrorData>
                {
                    list = telecommunicationNumberList
                };
                return Ok(output);
            }

            //When trying to get a list of telecommunication number for a Person that doesn't exists
            return NoContent();
        }
    }
}
