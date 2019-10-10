using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Dto
{
	/// <summary>
	/// Used when returning Ops user information
	/// </summary>
	public class OpsUserDataDto
	{
		/// <summary>
		/// The user id 
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// The login name
		/// </summary>
		public string LoginName { get; set; }

		/// <summary>
		/// The status
		/// </summary>
		public string Status { get; set; }

		/// <summary>
		/// The user type
		/// </summary>
		public OpsUserType UserType { get; set; }

		/// <summary>
		/// The asset
		/// </summary>
		public OpsAssetGroupDto Asset { get; set; }
	}
}