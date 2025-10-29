using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.UnifiedAmenities;

namespace UnifiedLogin.BusinessLogic.Logic.Product.Interfaces
{
	/// <summary>
	/// Interface
	/// </summary>
	public interface IManageUnifiedAmenities
	{
		/// <summary>
		/// Used to update a user roles and rights for Unified Amenities
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="userAssignProductPropertyRole"></param>
		/// <returns></returns>
		string ManageUnifiedAmenitiesUser(long editorPersonaId, long userPersonaId, UnifiedAmenitiesPropertyRole userAssignProductPropertyRole);

		/// <summary>
		/// Used to unassign a user from Unified Amenities
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="userAssignProductPropertyRole"></param>
		/// <returns></returns>
		string UnassignUser(long editorPersonaId, long userPersonaId, UnifiedAmenitiesPropertyRole userAssignProductPropertyRole);

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
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="assignedOnly"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		ListResponse GetProperties(long editorPersonaId, long userPersonaId, bool assignedOnly, RequestParameter datafilter);
	}
}