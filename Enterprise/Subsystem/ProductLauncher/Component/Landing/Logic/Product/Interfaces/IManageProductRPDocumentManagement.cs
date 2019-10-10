using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces
{
	public interface IManageProductRPDocumentManagement
	{
		/// <summary>
		/// Used to get roles for a company or user
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		ListResponse GetRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="roleId"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		ListResponse GetRoleClassifierDataset(long editorPersonaId, long userPersonaId, string roleId, RequestParameter datafilter);

		/// <summary>
		/// Updated to create/update a user
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId"></param>
		/// <param name="rolePropertyEntityList">The role, property or department to assign the user</param>
		/// <returns></returns>
		string ManageRPDMUser(long editorPersonaId, long userPersonaId, RolePropertyList rolePropertyEntityList);

		/// <summary>
		/// Used to unassign a user from the product
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <returns></returns>
		string UnassignUser(long editorPersonaId, long userPersonaId);

		/// <summary>
		/// Used to get the domain for a user persona
		/// </summary>
		/// <param name="personaId"></param>
		/// <returns>The domain of the company</returns>
		ListResponse GetDomain(long personaId);
	}
}