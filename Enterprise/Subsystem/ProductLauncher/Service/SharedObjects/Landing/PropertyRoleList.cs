using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	public class PropertyRoleList
	{
		public string PropertyId { get; set; }
		public List<string> Roles { get; set; }
	}

	public class PAMRolePropertyList
	{
		public string RoleId { get; set; }
		public List<string> PropertyIds { get; set; }
	}
}