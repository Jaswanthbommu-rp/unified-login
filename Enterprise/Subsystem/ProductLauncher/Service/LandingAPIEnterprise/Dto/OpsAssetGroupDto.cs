using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Dto
{
	public class OpsAssetGroupDto : AssetGroup
    {
		/// <summary>
		/// Exclude the group type of the asset group
		/// </summary>
		[JsonIgnoreAttribute]
		public bool IsAssigned { get; set; }
	}
}