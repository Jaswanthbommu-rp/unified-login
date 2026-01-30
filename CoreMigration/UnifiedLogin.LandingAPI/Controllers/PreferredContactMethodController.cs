using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Preferred Contact Method Controller to hold all contact mechanism management related APIs
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class PreferredContactMethodController : ControllerBase
    {
        private readonly IPreferredContactMethodRepository _preferredContactMethodRepository;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public PreferredContactMethodController(IPreferredContactMethodRepository preferredContactMethodRepository)
        {
            _preferredContactMethodRepository = preferredContactMethodRepository;
        }

        /// <summary>
        /// List Preferred Contact Methods details
        /// </summary>
        /// <returns>A list of Preferred Contact Methods details</returns>
        [HttpGet("preferredcontactmethods")]
        [ProducesResponseType(typeof(ObjectListOutput<PreferredContactMethod, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListPreferredContactMethod()
        {
            var preferredContactMethodList = await Task.Run(() =>
                _preferredContactMethodRepository.ListPreferredContactMethod());

            if (preferredContactMethodList != null && preferredContactMethodList.Any())
            {
                ObjectListOutput<PreferredContactMethod, IErrorData> output = new ObjectListOutput<PreferredContactMethod, IErrorData>
                {
                    list = preferredContactMethodList
                };
                return Ok(output);
            }

            // When trying to get a list of Contact Mechanism UsageTypes that doesn't exist
            return NoContent();
        }
    }
}
