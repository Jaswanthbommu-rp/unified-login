using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UPFMProduct;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
	public interface IManageUPFMProductsIntegration
	{
		/// <summary>
		/// Used to update a user roles and rights for IntelligentBuilding
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="userAssignProductPropertyRole"></param>
		/// <returns></returns>
		string ManageUPFMProductUser(long editorPersonaId, long userPersonaId, UPFMProductPropertyRole userAssignProductPropertyRole, ProductEnum product);

		/// <summary>
		/// Used to unassign a user from IntelligentBuilding
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="userAssignProductPropertyRole"></param>
		/// <returns></returns>
		string UnassignUser(long editorPersonaId, long userPersonaId, UPFMProductPropertyRole userAssignProductPropertyRole, ProductEnum product);

		/// <summary>
		/// Returns Roles for the given user and company
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="partyId"></param>
		/// <returns></returns>
		ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId, ProductEnum product);

		/// <summary>
		/// Returns Rights with selected rights for a roleId
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="partyId"></param>
		/// <param name="roleId"></param>
		/// <returns></returns>
		ListResponse GetRightsByRole(long editorPersonaId, long partyId, long roleId, ProductEnum product);

		/// <summary>
		/// Used to get the list of properties for the company or for the given user
		/// </summary>		
		/// <param name="userPersonaId"></param>	
		/// <param name="product"></param>	
		/// <param name="include"></param>
		/// <param name="flag"></param>
		/// <param name="multiCompanyRealPageId"></param>
		/// <returns></returns>
		ListResponse GetUPFMProperties(long userPersonaId, ProductEnum product, string include = null, string flag = null,string multiCompanyRealPageId = null);

		/// <summary>
		/// Get a list of UPFM property instances for the give user
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="assignedOnly"></param>
		/// <param name="product"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		ListResponse GetUPFMProperties(long editorPersonaId, long userPersonaId, bool assignedOnly, ProductEnum product, RequestParameter datafilter);
	}
}
