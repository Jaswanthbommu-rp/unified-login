using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository
{
	public interface IUserRoleRepository
	{
		/// <summary>
		/// Get list of Roles by User Persona ID and product
		/// </summary>
		/// <param name="personaId">Persona ID</param>   
		/// <param name="productId">Product ID</param>   
		/// <returns>Role assigned to Persona</returns>
		IList<ProductRole> GetProductRolesByPersona(long userPersonaId, ProductEnum productId);

		/// <summary>
		/// Get list of Rights id by Party, product id and role id
		/// </summary>
		/// <param name="partyId">Company party id</param>
		/// <param name="productIdList">Company product id list</param>
		/// <param name="productId">Product Id</param>
		/// <param name="roleId">Role Id</param>
		/// <returns>The list of rights for the given role</returns>
		IList<ProductRight> ListRightsByRole(long partyId, IList<int> productIdList, ProductEnum productId, long roleId);
	}
}