using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.Core;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// OmniChannel Controller for product management
    /// </summary>
    [ApiController]
    [Route("")]
    [Authorize]
    public class ProductOmniChannelController : BaseController
    {
        private readonly IManageProductOmniChannel _manageProductOmniChannel;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ProductOmniChannelController(IUserClaimsAccessor userClaimsAccessor)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            _manageProductOmniChannel = new ManageProductOmniChannel(userClaim);
        }

        /// <summary>
        /// Returns Properties
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <param name="datafilter">A datafilter used to filter the properties</param>
        /// <returns>List of properties</returns>
        [HttpGet("products/omnichannel/user/properties")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProperties(long editorPersonaId, long userPersonaId, [FromQuery] RequestParameter datafilter)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (editorPersonaId == 0)
                    return BadRequest("editorPersonaId not supplied.");

                var userClaim = _userClaimsAccessor.GetUserClaim();
                if (userClaim == null || userClaim.UserRealPageGuid == Guid.Empty)
                    return BadRequest("RealPageId empty.");

                var result = _manageProductOmniChannel.GetProperties(editorPersonaId, userPersonaId, datafilter);

                return Ok(result);
            });
        }

        /// <summary>
        /// Returns Roles by PartyID
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <param name="partyId">Organization PartyID</param>
        /// <param name="datafilter">Data filter parameters</param>
        /// <returns>List of roles by party id</returns>
        [HttpGet("products/omnichannel/roles")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRoles(long editorPersonaId, long userPersonaId, long partyId, [FromQuery] RequestParameter datafilter)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (partyId == 0)
                    return BadRequest("partyId not supplied.");

                var result = _manageProductOmniChannel.GetRoles(editorPersonaId, userPersonaId, partyId);

                return Ok(result);
            });
        }
    }
}
