using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Logic
{
	/// <summary>
	/// Interface for ManageProvider
	/// </summary>
    public interface IManageProvider
    {
		/// <summary>
		/// Get the Identity Provider Type detail by User login name
		/// </summary>
		/// <param name="enterpriseUserName">User login Name.</param>
		/// <returns>Identity Provider Type object</returns>
		IdentityProviderType GetProviderByEnterpriseUserName(string enterpriseUserName);

		/// <summary>
		/// Get Provider configurations by name
		/// </summary>
		/// <param name="providerName">Provider Name</param>
		/// <returns>Provider Configuration object</returns>
		IList<ProviderConfiguration> GetProviderConfigurationByName(string providerName);
	}
}