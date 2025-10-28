using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.RentersInsurance
{
	/// <summary>
	/// Inteface for RentersInsuranceRoleAndPropertyList.
	/// Used to grant a Renters Insurance user roles and properties
	/// </summary>
	public interface IRentersInsuranceRoleAndPropertyList
	{
		/// <summary>
		/// Is product assigned or removed
		/// </summary>
		bool IsAssigned { get; set; }

		/// <summary>
		/// A list of properties to assign to the user
		/// </summary>
		List<string> PropertyList { get; set; }

		/// <summary>
		/// Role assigned to the user
		/// </summary>
		List<string> RoleList { get; set; }
	}
}