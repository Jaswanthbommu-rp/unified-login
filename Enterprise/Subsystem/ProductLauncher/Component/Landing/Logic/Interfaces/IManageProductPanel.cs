using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
	/// <summary>
	/// Interface for Manages Product panel
	/// </summary>
	public interface IManageProductPanel
	{
		/// <summary>
		/// Get Product Properties
		/// </summary> 
		/// <param name="editorPersonaId">editorPersonaId</param>		
		/// <param name="productId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="assignedOnly"></param>
		/// <param name="datafilter"></param>
		/// <returns>String.empty if success else error</returns>
		ListResponse GetProductProperties(long editorPersonaId, long userPersonaId, int productId, RequestParameter datafilter, bool assignedOnly = false);
		/// <summary>
		/// Get Product Roles
		/// </summary> 
		/// <param name="editorPersonaId">editorPersonaId</param>
		/// <param name="partyId"></param>
		/// <param name="productId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="assignedOnly"></param>
		/// <param name="datafilter"></param>
		/// <returns>String.empty if success else error</returns>
		ListResponse GetProductRoles(long editorPersonaId, long userPersonaId, long partyId, int productId, RequestParameter datafilter, bool assignedOnly = false);
		/// <summary>
		///Get Product Rights For Role
		/// </summary> 
		/// <param name="editorPersonaId">editorPersonaId</param>
		/// <param name="roleId"></param>
		/// <param name="partyId"></param>
		/// <param name="productId"></param>	
		/// <param name="datafilter"></param>
		/// <param name="assignedToRoleOnly"></param>		
		/// <returns>String.empty if success else error</returns>
		ListResponse GetProductRightsForRole(long editorPersonaId, int roleId, long partyId, int productId, RequestParameter datafilter, bool assignedToRoleOnly = false);
		/// <summary>
		/// Get Product PropertyGroups
		/// </summary> 
		/// <param name="editorPersonaId">editorPersonaId</param>		
		/// <param name="productId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="assignedOnly"></param>
		/// <param name="datafilter"></param>
		/// <returns>String.empty if success else error</returns>
		ListResponse GetProductPropertyGroups(long editorPersonaId, long userPersonaId, int productId, RequestParameter datafilter, bool assignedOnly = false);
	}
}
