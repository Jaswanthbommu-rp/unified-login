using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// RelationshipType Controller to hold all RelationshipType management related APIs
    /// </summary>
    [ApiController]
    [Authorize]
    public class RelationshipTypeController : ControllerBase
    {
        private readonly IUserClaimsAccessor _userClaimsAccessor;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public RelationshipTypeController(IUserClaimsAccessor userClaimsAccessor)
        {
            _userClaimsAccessor = userClaimsAccessor;
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
        public async Task<IActionResult> ListRelationshipType(string relationshipTypeName)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                IManageRelationshipType relationshipTypeLogic = new ManageRelationshipType(userClaim);

                IList<RelationshipType> relationshipTypeList = relationshipTypeLogic.GetRelationshipType(relationshipTypeName);

                if (relationshipTypeList != null)
                {
                    ObjectListOutput<RelationshipType, IErrorData> output = new ObjectListOutput<RelationshipType, IErrorData>() { list = relationshipTypeList };
                    return Ok(output);
                }

                // When trying to get a list of relationshipTypes that doesn't exist
                return NoContent();
            });
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
        public async Task<IActionResult> ListUserRelationTypes()
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                IManageRelationshipType manageRelationshipType = new ManageRelationshipType(userClaim);

                var userRelationships = manageRelationshipType.GetUserRelationShipTypes();

                if (userRelationships != null)
                {
                    ObjectListOutput<UserRelationShipType, IErrorData> output = new ObjectListOutput<UserRelationShipType, IErrorData>() { list = userRelationships };
                    return Ok(output);
                }
                return NoContent();
            });
        }
    }
}
