using System;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Enterprise
{
	public class UserProductDetails
	{
		public Guid EditorRealPageId { get; set; }

		public UserData UserProfileDetails { get; set; }

		public IList<ProductDetail> ProductList { get; set; }
	}
}