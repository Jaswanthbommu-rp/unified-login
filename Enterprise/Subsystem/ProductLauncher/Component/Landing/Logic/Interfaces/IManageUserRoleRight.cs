using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
	/// <summary>
	/// Used to get role/right information for the given persona, product id
	/// </summary>
	public interface IManageUserRoleRight
	{
		/// <summary>
		/// Used to get a list of roles assigned to the given persona for the given product id
		/// </summary>
		/// <param name="productId">The product id</param>
		/// <param name="userPersonaId">The persona id</param>
		/// <param name="organizationPartyId">Optional Organization PartyId</param>
		/// <returns>A list of roles</returns>
		IList<Role> GetAssignedRoleForPersona(ProductEnum productId, long? userPersonaId = null, long? organizationPartyId = null);
	}
}