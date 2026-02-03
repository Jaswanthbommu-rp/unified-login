using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// RoleType Controller to hold all RoleType management related APIs
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class RoleTypeController : BaseController
    {

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public RoleTypeController(IUserClaimsAccessor userClaimsAccessor) : base(userClaimsAccessor)
        {
        }

        /// <summary>
        /// List Role type details
        /// </summary>
        /// <param name="roleTypeName">RoleType Name</param>
        /// <param name="loginName">Optional User LoginName</param>
        /// <param name="includeRelationShips">Include relationship types</param>
        /// <returns>A list of Role type details</returns>
        [HttpGet("roletypes")]
        [ProducesResponseType(typeof(ObjectListOutput<RoleType, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListRoleType(string roleTypeName = null, string loginName = null, bool includeRelationShips = false)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var manageRoleType = new ManageRoleType();
                var managePersona = new ManagePersona(userClaim);
                var profileRepository = new ProfileRepository(userClaim);
                var manageRelationshipType = new ManageRelationshipType(userClaim);

                var roleTypeList = new List<RoleType>();

                // see if the caller is authenticated and if so use the organization of the user to get the type list
                if (userClaim.OrganizationPartyId != 0 && roleTypeName != null && roleTypeName.Equals("User Role", StringComparison.OrdinalIgnoreCase))
                {
                    var persona = managePersona.GetPersona(userClaim.PersonaId);
                    if (persona == null)
                    {
                        return BadRequest("editorPersonaId not found.");
                    }

                    roleTypeList = (List<RoleType>)manageRoleType.GetRoleTypeDependency(roleTypeId: persona.UserTypeId, partyId: userClaim.OrganizationPartyId, orgMasterId: persona.Organization.BooksCustomerMasterId, loginName: loginName);
                    if (!userClaim.IsRPEmployee && persona.UserTypeId == (int)UserRoleType.ExternalUser)
                    {
                        roleTypeList.RemoveAll(x => x.Name.Equals("SuperUser", StringComparison.OrdinalIgnoreCase));
                        var externalUserRelationship = profileRepository.GetExternalUserRelationship(userClaim.OrganizationPartyId, userClaim.UserId);
                        if (!string.IsNullOrEmpty(externalUserRelationship.OperatorCode) && !string.IsNullOrEmpty(externalUserRelationship.OperatorValue))
                        {
                            roleTypeList.RemoveAll(x => x.PartyRoleTypeId != 405);
                        }
                    }
                }
                else
                {
                    roleTypeList = (List<RoleType>)manageRoleType.GetRoleType(roleTypeName: roleTypeName, partyId: null, orgMasterId: null, loginName: loginName);
                }

                // remove the RealPage employee role from showing for unauthenticated requests
                if (userClaim.OrganizationPartyId == 0)
                {
                    roleTypeList.RemoveAll(x => x.Name.Equals("RealPage Employee", StringComparison.OrdinalIgnoreCase));
                }

                if (roleTypeList == null) return NoContent();

                if (includeRelationShips)
                {
                    var userRelationshipTypes = manageRelationshipType.GetUserRelationShipTypes();

                    foreach (var r in roleTypeList)
                    {
                        r.UserRelationShipTypes = userRelationshipTypes.Where(c => c.PartyRoleTypeId == r.PartyRoleTypeId).ToList();
                    }
                }

                var output = new ObjectListOutput<RoleType, IErrorData>() { list = roleTypeList };
                return Ok(output);
            });
        }
    }
}
