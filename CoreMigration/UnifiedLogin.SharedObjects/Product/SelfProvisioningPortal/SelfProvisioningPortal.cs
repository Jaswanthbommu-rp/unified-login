using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Product.SelfProvisioningPortal
{
	/// <summary>
	/// Used to grant/remove access to Self Provisioning Portal Product.
	/// </summary>
	public class SelfProvisioningPortal : ISelfProvisioningPortal
	{
		/// <summary>
		/// Is product assigned or removed
		/// </summary>
		[JsonProperty(PropertyName = "IsAssigned")]
		public bool IsAssigned { get; set; }
	}
}
