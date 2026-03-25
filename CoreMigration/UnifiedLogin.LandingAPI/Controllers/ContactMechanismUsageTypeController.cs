using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Contact Mechanism UsageType Controller to hold all Contact Mechanism UsageType management related APIs
    /// </summary>
    [Route("")]
    [ApiController]
    public class ContactMechanismUsageTypeController : BaseController
    {
        private readonly IContactMechanismUsageTypeRepositoryAsync _contactMechanismUsageTypeRepository;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ContactMechanismUsageTypeController(IContactMechanismUsageTypeRepositoryAsync contactMechanismUsageTypeRepository, IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
            _contactMechanismUsageTypeRepository = contactMechanismUsageTypeRepository ?? throw new ArgumentNullException(nameof(contactMechanismUsageTypeRepository));
        }

        /// <summary>
        /// List contact mechanism usage type details
        /// </summary>
        /// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
        /// <param name="cancellationToken">Propagates notification that the request has been cancelled.</param>
        /// <returns>A list of contact mechanism usage type details</returns>
        [HttpGet("contactmechanismusagetypes")]
        [ProducesResponseType(typeof(ObjectListOutput<ContactMechanismUsageType, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListContactMechanismUsageType(string ContactMechanismUsageTypeName = null, CancellationToken cancellationToken = default)
        {
            var contactMechanismUsageTypeList = await _contactMechanismUsageTypeRepository
                .ListContactMechanismUsageTypeAsync(ContactMechanismUsageTypeName, cancellationToken);

            if (contactMechanismUsageTypeList != null && contactMechanismUsageTypeList.Any())
            {
                return Ok(new ObjectListOutput<ContactMechanismUsageType, IErrorData>
                {
                    list = contactMechanismUsageTypeList
                });
            }

            return NoContent();
        }
    }
}
