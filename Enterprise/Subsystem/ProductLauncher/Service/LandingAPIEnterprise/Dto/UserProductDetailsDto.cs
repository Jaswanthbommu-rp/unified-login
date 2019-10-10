using Newtonsoft.Json;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Dto
{
	/// <summary>
	/// UserProductDetails Data Transform Object
	/// </summary>
	public class UserProductDetailsDto
	{
		/// <summary>
		/// UserProfileDetails
		/// </summary>
		public UserDataDto UserProfileDetails { get; set; }

		/// <summary>
		/// ProductList
		/// </summary>
		public IList<ProductDetailDto> ProductList { get; set; }
	}
}