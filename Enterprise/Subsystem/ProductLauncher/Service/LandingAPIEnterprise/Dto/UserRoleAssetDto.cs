using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Dto
{
	/// <summary>
	/// User Role and Assets
	/// </summary>
	public class UserRoleAssetDto
	{
		/// <summary>
		/// User Product Roles
		/// </summary>
		[JsonProperty(PropertyName = "ProductRole")]
		public IList<ProductRole> ProductRole { get; set; }

		/// <summary>
		/// User Asset Group  
		/// </summary>
		[JsonProperty(PropertyName = "AssetGroup")]
		public IList<AssetGroup> AssetGroups { get; set; }
	}
}