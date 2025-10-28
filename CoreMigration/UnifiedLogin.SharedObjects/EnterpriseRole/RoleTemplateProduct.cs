using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.EnterpriseRole
{
	public class RoleTemplateProduct
	{
		/// <summary>
		/// RoleTemplateProductId
		/// </summary>
		public int RoleTemplateProductId { get; set; }

		/// <summary>
		/// ProductId
		/// </summary>
		public int ProductId { get; set; }

		public List<RoleTemplateRoles> Roles { get; set; }

		public List<AdditionalAttributes> AdditionalAttributes { get; set; }

		public List<RemovedRoles> RemovedRoleList { get; set; }

		public List<RemovedAdditionalAttributes> RemovedAdditionalAttributes { get; set; }
	}
}
