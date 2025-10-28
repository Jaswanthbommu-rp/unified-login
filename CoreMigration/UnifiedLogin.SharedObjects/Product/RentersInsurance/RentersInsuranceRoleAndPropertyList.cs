using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.RentersInsurance
{
	/// <summary>
	/// Used to grant a Renters Insurance user roles and properties
	/// </summary>
	public class RentersInsuranceRoleAndPropertyList : IRentersInsuranceRoleAndPropertyList
	{
		/// <summary>
		/// Is product assigned or removed
		/// </summary>
		public bool IsAssigned { get; set; } = true;

		/// <summary>
		/// A list of properties to assign to the user
		/// </summary>
		public List<string> PropertyList { get; set; }

		/// <summary>
		/// Role assigned to the user
		/// </summary>
		public List<string> RoleList { get; set; }
	}
}
