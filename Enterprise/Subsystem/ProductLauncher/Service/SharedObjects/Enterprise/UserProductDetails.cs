using System;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise
{
	public class UserProductDetails
	{
		public Guid EditorRealPageId { get; set; }

		public UserData UserProfileDetails { get; set; }

		public IList<ProductDetail> ProductList { get; set; }
	}
}