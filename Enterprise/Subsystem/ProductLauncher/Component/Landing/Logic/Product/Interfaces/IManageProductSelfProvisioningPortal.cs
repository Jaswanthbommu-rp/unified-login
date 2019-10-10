using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.SelfProvisioningPortal;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces
{
	/// <summary>
	/// Interface for ManageProductSelfProvisioningPortal
	/// </summary>
	public interface IManageProductSelfProvisioningPortal
	{
		/// <summary>
		/// Assign User in GreenBook access to Self Provisioning Portal
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <param name="selfProvisioningPortal">Used to grant access to Self Provisioning Portal Product.</param>
		/// <returns>ObjectOutput object</returns>
		ObjectOutput<ISelfProvisioningPortal, IErrorData> ManageSelfProvisioningPortalUser(long editorPersonaId, long userPersonaId, SelfProvisioningPortal selfProvisioningPortal);

		/// <summary>
		/// Unassign User in GreenBook from Self Provisioning Portal
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <param name="selfProvisioningPortal">Used to remove access to Self Provisioning Portal Product.</param>
		/// <returns>ObjectOutput object</returns>
		ObjectOutput<ISelfProvisioningPortal, IErrorData> UnassignSelfProvisioningPortalUser(long editorPersonaId, long userPersonaId, SelfProvisioningPortal selfProvisioningPortal);
	}
}