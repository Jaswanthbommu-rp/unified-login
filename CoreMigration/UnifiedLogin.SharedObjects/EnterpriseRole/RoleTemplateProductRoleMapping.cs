using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.EnterpriseRole
{
	public class RoleTemplateProductRoleMapping
	{
		/// <summary>
		/// PartyId
		/// </summary>
		public long PartyId { get; set; }

		/// <summary>
		/// RoleTemplateId
		/// </summary>
		public int RoleTemplateId { get; set; }

		/// <summary>
		/// RoleTemplateName
		/// </summary>
		public string RoleTemplateName { get; set; }

		/// <summary>
		/// RoleTemplateDescription
		/// </summary>
		public string RoleTemplateDescription { get; set; }

		/// <summary>
		/// Products
		/// </summary>
		public List<RoleTemplateProduct> Products { get; set; }

		/// <summary>
		/// RemovedRoleTemplateProducts
		/// </summary>
		public List<RemovedRoleTemplateProducts> RemovedRoleTemplateProducts { get; set; }
		public List<string> ProductsError { get; set; }
	}
}
