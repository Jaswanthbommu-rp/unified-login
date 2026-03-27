using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// RelationshipType Controller to hold all RelationshipType management related APIs
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class RelationshipTypeController : BaseController
    {
        private readonly IManageRelationshipTypeAsync _manageRelationshipTypeAsync;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public RelationshipTypeController(
            IUserClaimsAccessor userClaimsAccessor,
            IManageRelationshipTypeAsync manageRelationshipTypeAsync) : base(userClaimsAccessor)
        {
            _manageRelationshipTypeAsync = manageRelationshipTypeAsync;
        }

        /// <summary>
        /// List Role type details
        /// </summary>
        /// <param name="relationshipTypeName">Relationship type name</param>
        /// <returns>A list of Role type details</returns>
        [HttpGet("relationshiptypes/{relationshipTypeName}")]
        [ProducesResponseType(typeof(ObjectListOutput<RelationshipType, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListRelationshipType(string relationshipTypeName, CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var relationshipTypeList = await _manageRelationshipTypeAsync.GetRelationshipTypeAsync(relationshipTypeName, cancellationToken);

            if (relationshipTypeList != null)
            {
                var output = new ObjectListOutput<RelationshipType, IErrorData>() { list = relationshipTypeList };
                return Ok(output);
            }

            return NoContent();
        }

        /// <summary>
        /// List UserRelationShipTypes details
        /// </summary>
        /// <returns>A list of Role type details</returns>
        [HttpGet("userrelationshiptypes")]
        [ProducesResponseType(typeof(ObjectListOutput<UserRelationShipType, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListUserRelationTypes(CancellationToken cancellationToken = default)
        {
            var userClaim = _userClaimsAccessor.GetUserClaim();
            var userRelationships = await _manageRelationshipTypeAsync.GetUserRelationShipTypesAsync(cancellationToken);

            if (userRelationships != null)
            {
                var output = new ObjectListOutput<UserRelationShipType, IErrorData>() { list = userRelationships };
                return Ok(output);
            }

            return NoContent();
        }
    }
}
