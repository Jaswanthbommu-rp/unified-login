using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.Core;
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
    public class PreferredContactMethodController : BaseController
    {
        private readonly IPreferredContactMethodRepositoryAsync _preferredContactMethodRepository;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public PreferredContactMethodController(IPreferredContactMethodRepositoryAsync preferredContactMethodRepository, IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
            _preferredContactMethodRepository = preferredContactMethodRepository ?? throw new ArgumentNullException(nameof(preferredContactMethodRepository));
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
        public async Task<IActionResult> ListPreferredContactMethod(CancellationToken cancellationToken = default)
        {
            var preferredContactMethodList = await _preferredContactMethodRepository
                .ListPreferredContactMethodAsync(cancellationToken);

            if (preferredContactMethodList != null && preferredContactMethodList.Any())
            {
                return Ok(new ObjectListOutput<PreferredContactMethod, IErrorData>
                {
                    list = preferredContactMethodList
                });
            }

            return NoContent();
        }
    }
}
