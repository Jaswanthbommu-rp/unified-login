namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.SelfProvisioningPortal
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