namespace UnifiedLogin.SharedObjects.Product.SelfProvisioningPortal
{
	/// <summary>
	/// Interface for Self Provisioning Portal
	/// </summary>
	public interface ISelfProvisioningPortal
	{
		/// <summary>
		/// Is product assigned or removed
		/// </summary>
		bool IsAssigned { get; set; }
	}
}