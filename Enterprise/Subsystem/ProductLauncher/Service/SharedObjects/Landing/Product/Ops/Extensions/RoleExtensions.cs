using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;
using System.Collections.Generic;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.Product.Ops.Extensions
{
	/// <summary>
	/// 
	/// </summary>
	public static class RoleExtensions
    {
        /// <summary>
        /// Used to convert a product role into a GreenBook role to be used by the UI
        /// </summary>
        /// <param name="roles">The list of roles to convert</param>
        /// <returns></returns>
        public static IList<ProductRole> ToGBRoles(this IList<Role> roles)
        {
            if (roles == null) return null;
            IList<ProductRole> results = new List<ProductRole>();
            foreach (Role role in roles)
            {
                results.Add(new ProductRole
                {
                    ID = role.Id.ToString(),
                    Name = role.Name,
                    Roletype = role.IsMarketPlaceAdmin
                });
            }

	        results = results.OrderBy(p => p.Name).ToList();
            return results;
        }
    }
}
