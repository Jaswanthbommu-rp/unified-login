using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.IntegrationMarketplace;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces
{
	/// <summary>
	/// IManageProductIntegrationMarketplace
	/// </summary>
	public interface IManageProductIntegrationMarketplace
	{
		/// <summary>
		/// Returns Roles (Roles in GB)
		/// </summary>
		ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId);

		/// <summary>
		/// Un-assign user
		/// </summary> 
		string UnassignUser(long editorPersonaId, long userPersonaId,
			IntegrationMarketplacePropertyRole userAssignProductPropertyRole);

		/// <summary>
		/// Create Update User
		/// </summary> 
		string ManageIntegrationMarketplaceUser(long editorPersonaId, long userPersonaId,
			IntegrationMarketplacePropertyRole userAssignProductPropertyRole,
			BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser);

		/// <summary>
		/// Change user type 
		/// </summary>
		string ChangeIntegrationMarketplaceUserType(long createUserPersonaId, long assignUserPersonaId,
			IntegrationMarketplacePropertyRole rpList, BatchProcessType batchProcessType);

		/// <summary>
		/// Get all IM roles
		/// </summary>
		List<IntegrationMarketplaceRole> GetIntegrationMarketplaceRoles();
	}
}
