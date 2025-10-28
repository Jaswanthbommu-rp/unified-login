using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Landing
{
	public class ProductSamlDetails
	{
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public string ProductDescription { get; set; }
		public string ProductStatus { get; set; }
		public string UserID { get; set; }
		public string ProductUserName { get; set; }
		public string PMCID { get; set; }
		public string RoleType { get; set; }
		public string PortalId { get; set; }
		public string OrganizationId { get; set; }
		public string NWPUserType { get; set; }
        public string LearnerId { get; set; }
        public string ManagerId { get; set; }
        public string DualRole { get; set; }
        public int ParentProductTypeId { get; set; }
		public int ProductEnabled { get; set; }
		public List<ProductDetails> Products { get; set; }
	}

	public class SamlUserProductDetails
	{
		public IList<ProductSamlDetails> ProductSamlDetails { get; set; }

		public List<GbProductMap> AOProducts { get; set; }
	}

	public class ProductDetails
	{
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public string Status { get; set; }
	}
}
