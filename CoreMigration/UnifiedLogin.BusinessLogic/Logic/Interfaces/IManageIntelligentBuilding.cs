using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.IntelligentBuilding;
using System.Collections.Generic;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
	public interface IManageIntelligentBuilding
	{
		/// <summary>
		/// Used to update a user roles and rights for IntelligentBuilding
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="userAssignProductPropertyRole"></param>
		/// <returns></returns>
		string ManageIntelligentBuildingUser(long editorPersonaId, long userPersonaId, IBPropertyRole userAssignProductPropertyRole);

		/// <summary>
		/// Used to unassign a user from IntelligentBuilding
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="userAssignProductPropertyRole"></param>
		/// <returns></returns>
		string UnassignUser(long editorPersonaId, long userPersonaId, IBPropertyRole userAssignProductPropertyRole);

		/// <summary>
		/// Returns Roles for the given user and company
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="partyId"></param>
		/// <returns></returns>
		ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId);

		/// <summary>
		/// Returns Rights with selected rights for a roleId
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="partyId"></param>
		/// <param name="roleId"></param>
		/// <returns></returns>
		ListResponse GetRightsByRole(long editorPersonaId, long partyId, long roleId);

		/// <summary>
		/// Used to get the list of properties for the company or for the given user
		/// </summary>		
		/// <param name="userPersonaId"></param>		
		/// <param name="include"></param>
		/// <returns></returns>
		ListResponse GetUPFMProperties(long userPersonaId, string include = null);
	}
}
