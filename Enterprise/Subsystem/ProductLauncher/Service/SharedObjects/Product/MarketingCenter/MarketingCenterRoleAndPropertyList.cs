using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.MarketingCenter
{
	/// <summary>
	/// Used to grant a user roles and properties
	/// </summary>
	public class MarketingCenterRoleAndPropertyList
	{
		/// <summary>
		/// A list of roles to assign to the user
		/// </summary>
		public List<int> RoleList;

		/// <summary>
		/// A list of properties to assign to the user
		/// </summary>
		public List<string> PropertyList;

		/// <summary>
		/// Is product assigned or removed
		/// </summary>
		public bool IsAssigned { get; set; }
	}
}
