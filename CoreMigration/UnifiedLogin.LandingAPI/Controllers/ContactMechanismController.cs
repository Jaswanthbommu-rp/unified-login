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
    /// Contact Mechanism Controller to hold all contact mechanism management related APIs
    /// </summary>
    [ApiController]
    public class ContactMechanismController : ControllerBase
    {
        private readonly IContactMechanismRepository _contactMechanismRepository;
        private readonly IManageContactMechanism _manageContactMechanism;
        private readonly IUserClaimsAccessor _userClaimsAccessor;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ContactMechanismController(
            IContactMechanismRepository contactMechanismRepository,
            IManageContactMechanism manageContactMechanism,
            IUserClaimsAccessor userClaimsAccessor)
        {
            _contactMechanismRepository = contactMechanismRepository;
            _manageContactMechanism = manageContactMechanism;
            _userClaimsAccessor = userClaimsAccessor;
        }

        /// <summary>
        /// List Contact Mechanism details for a Person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
        /// <returns>A list of Contact Mechanism Details for a person</returns>
        [HttpGet("persons/{realPageId}/contactmechanism")]
        [ProducesResponseType(typeof(ObjectListOutput<CommonAddress, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListContactMechanismForPerson(Guid realPageId, string ContactMechanismUsageTypeName = "")
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;

            realPageId = (realPageId == Guid.Empty) ? realPageUserId : realPageId;
            if (realPageId == Guid.Empty)
            {
                return BadRequest("Invalid parameter: realPageId");
            }

            var contactMechanismList = await Task.Run(() =>
                _contactMechanismRepository.ListContactMechanismForPerson(realPageId, ContactMechanismUsageTypeName));

            if (contactMechanismList != null && contactMechanismList.Any())
            {
                ObjectListOutput<CommonAddress, IErrorData> output = new ObjectListOutput<CommonAddress, IErrorData>
                {
                    list = contactMechanismList
                };
                return Ok(output);
            }

            // When trying to get a list of Contact Mechanism(s) for a Person that doesn't exist
            return NoContent();
        }
    }
}
