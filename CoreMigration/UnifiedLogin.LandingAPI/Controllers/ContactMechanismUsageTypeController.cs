using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Contact Mechanism UsageType Controller to hold all Contact Mechanism UsageType management related APIs
    /// </summary>
    [Route("")]
    [ApiController]
    public class ContactMechanismUsageTypeController : ControllerBase
    {
        private readonly IContactMechanismUsageTypeRepository _contactMechanismUsageTypeRepository;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ContactMechanismUsageTypeController(IContactMechanismUsageTypeRepository contactMechanismUsageTypeRepository)
        {
            _contactMechanismUsageTypeRepository = contactMechanismUsageTypeRepository;
        }

        /// <summary>
        /// List contact mechanism usage type details
        /// </summary>
        /// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
        /// <returns>A list of contact mechanism usage type details</returns>
        [HttpGet("contactmechanismusagetypes")]
        [ProducesResponseType(typeof(ObjectListOutput<ContactMechanismUsageType, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListContactMechanismUsageType(string ContactMechanismUsageTypeName = null)
        {
            var contactMechanismUsageTypeList = await Task.Run(() =>
                _contactMechanismUsageTypeRepository.ListContactMechanismUsageType(ContactMechanismUsageTypeName));

            if (contactMechanismUsageTypeList != null && contactMechanismUsageTypeList.Any())
            {
                ObjectListOutput<ContactMechanismUsageType, IErrorData> output = new ObjectListOutput<ContactMechanismUsageType, IErrorData>
                {
                    list = contactMechanismUsageTypeList
                };
                return Ok(output);
            }

            // When trying to get a list of Contact Mechanism UsageTypes that doesn't exist
            return NoContent();
        }
    }
}
