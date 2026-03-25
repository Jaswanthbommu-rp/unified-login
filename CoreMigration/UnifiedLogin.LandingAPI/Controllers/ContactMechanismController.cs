using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Contact Mechanism Controller to hold all contact mechanism management related APIs
    /// </summary>
    [Route("")]
    [ApiController]
    public class ContactMechanismController : BaseController
    {
        private readonly IContactMechanismRepositoryAsync _contactMechanismRepository;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ContactMechanismController(
            IContactMechanismRepositoryAsync contactMechanismRepository,
            IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
            _contactMechanismRepository = contactMechanismRepository ?? throw new ArgumentNullException(nameof(contactMechanismRepository));
        }

        /// <summary>
        /// List Contact Mechanism details for a Person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
        /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
        /// <returns>A list of Contact Mechanism Details for a person</returns>
        [HttpGet("persons/{realPageId}/contactmechanism")]
        [ProducesResponseType(typeof(ObjectListOutput<CommonAddress, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListContactMechanismForPerson(Guid realPageId, string ContactMechanismUsageTypeName = "", CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var realPageUserId = userClaim?.UserRealPageGuid ?? Guid.Empty;

            realPageId = (realPageId == Guid.Empty) ? realPageUserId : realPageId;
            if (realPageId == Guid.Empty)
                return BadRequest("Invalid parameter: realPageId");

            var contactMechanismList = await _contactMechanismRepository.ListContactMechanismForPersonAsync(realPageId, ContactMechanismUsageTypeName, cancellationToken);

            if (contactMechanismList != null && contactMechanismList.Any())
            {
                return Ok(new ObjectListOutput<CommonAddress, IErrorData> { list = contactMechanismList });
            }

            // When trying to get a list of Contact Mechanism(s) for a Person that doesn't exist
            return NoContent();
        }
    }
}
