using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.EnterpriseRole;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
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
		/// <param name="datafilter"></param>
		/// <returns>String.empty if success else error</returns>
		ListResponse GetProductProperties(long editorPersonaId, long userPersonaId, int productId, RequestParameter datafilter);
		/// <summary>
		/// Get Product Roles
		/// </summary> 
		/// <param name="editorPersonaId">editorPersonaId</param>
		/// <param name="partyId"></param>
		/// <param name="productId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="datafilter"></param>
		/// <param name="accessType"></param>
		/// <returns>String.empty if success else error</returns>
		ListResponse GetProductRoles(long editorPersonaId, long userPersonaId, long partyId, int productId, RequestParameter datafilter, AccessType? accessType);
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
		///Get Product Rights For Role
		/// </summary> 
		/// <param name="editorPersonaId">editorPersonaId</param>
		/// <param name="roleId"></param>
		/// <param name="partyId"></param>
		/// <param name="productId"></param>	
		/// <param name="datafilter"></param>
		/// <param name="assignedToRoleOnly"></param>		
		/// <returns>String.empty if success else error</returns>
		ListResponse GetProductRightsForRole(long editorPersonaId, string roleId, long partyId, int productId, RequestParameter datafilter, bool assignedToRoleOnly = false);
		/// <summary>
		/// Get Product PropertyGroups
		/// </summary> 
		/// <param name="editorPersonaId">editorPersonaId</param>		
		/// <param name="productId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="assignedOnly"></param>
		/// <param name="datafilter"></param>
		/// <param name="userLoginName"></param>
		/// <returns>String.empty if success else error</returns>
		ListResponse GetProductPropertyGroups(long editorPersonaId, long userPersonaId, int productId, RequestParameter datafilter, bool assignedOnly = false, string userLoginName = "");
		/// <summary>
		/// Get Product Group Properties
		/// </summary> 
		/// <param name="editorPersonaId">editorPersonaId</param>		
		/// <param name="productId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="propertyGroupId"></param>
		/// <param name="datafilter"></param>		
		/// <returns>String.empty if success else error</returns>
		ListResponse GetProductGroupProperties(long editorPersonaId, long userPersonaId, int productId, string propertyGroupId, RequestParameter datafilter);

		/// <summary>
		/// Get Product Rights
		/// </summary> 
		/// <param name="editorPersonaId">editorPersonaId</param>
		/// <param name="partyId"></param>
		/// <param name="productId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="datafilter"></param>			
		/// <returns>String.empty if success else error</returns>
		ListResponse GetProductRights(long editorPersonaId, long userPersonaId, long partyId, int productId, RequestParameter datafilter);

		/// <summary>
		/// Get Product Rights
		/// </summary> 
		/// <param name="editorPersonaId">editorPersonaId</param>
		/// <param name="userPersonaId"></param>
		/// <param name="productId"></param>
		/// <param name="organizationRoleId"></param>
		/// <param name="organizationType"></param>			
		/// <returns>String.empty if success else error</returns>
		ListResponse GetProductOrganizations(long editorPersonaId, long userPersonaId, int productId, string organizationRoleId, string organizationType);

		/// <summary>
		/// Get Product Location Groups
		/// </summary>
		/// <param name="editorPersonaId">editorPersonaId</param>
		/// <param name="productId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="assignedOnly"></param>
		/// <param name="datafilter"></param>
		/// <param name="userLoginName"></param>
		/// <returns>String.empty if success else error</returns>
		ListResponse GetProductLocationGroups(long editorPersonaId, long userPersonaId, int productId, RequestParameter datafilter, bool assignedOnly = false, string userLoginName = "");

		/// <summary>
		/// Get unity migrated users for the specified product.
		/// </summary>
		/// <param name="editorPersonaId">editorPersonaId</param>
		/// <param name="productId">productId used to dispatch to the matching product implementation</param>
		/// <param name="datafilter">A datafilter used to paginate results.</param>
		/// <returns>ListResponse whose Records contain UnifiedMigratedUser objects</returns>
		ListResponse GetUnityMigratedUsers(long editorPersonaId, int productId, RequestParameter datafilter);

		/// <summary>
		/// Compare Product and Primary properties
		/// </summary>
		/// <param name="upfmProperty"></param>
		/// <param name="productId"></param>
		/// <param name="listResponse"></param>
		/// <returns></returns>
		ListResponse CompareProductAndPrimaryProperties(UPFMProperty upfmProperty, int productId, ListResponse listResponse);

		/// <summary>
		/// Get translated product properties
		/// </summary>
		/// <param name="upfmProperty"></param>
		/// <param name="productId"></param>
		/// <returns></returns>
		UPFMProperty TranslateProductProperties(UPFMProperty upfmProperty, int productId);

		/// <summary>
		/// GetUserProductRoles
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="partyId"></param>
		/// <returns></returns>
		RoleTemplateProductRoleMapping GetUserProductRoles(long editorPersonaId, long userPersonaId, long partyId);

		/// <summary>
		/// Get Persona Product Primary Properties
		/// </summary>
		/// <param name="userPersonaId"></param>
		/// <returns></returns>
		List<PersonaProductProperty> GetPersonaProductPrimaryProperties(long userPersonaId);

		/// <summary>
		/// Get Product USer Groups.
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="partyId"></param>
		/// <param name="productId"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		ListResponse GetProductUserGroups(long editorPersonaId, long userPersonaId, long partyId, int productId, RequestParameter datafilter);

        /// <summary>
        /// Get Product User Data
        /// </summary>
        /// <param name="editorPersonaId">editorPersonaId</param>
        /// <param name="productId">productId</param>
        /// <param name="datafilter">datafilter</param>
        /// <returns>String.empty if success else error</returns>
        ListResponse GetProductUserData(long editorPersonaId, int productId, RequestParameter datafilter);
    }
}
