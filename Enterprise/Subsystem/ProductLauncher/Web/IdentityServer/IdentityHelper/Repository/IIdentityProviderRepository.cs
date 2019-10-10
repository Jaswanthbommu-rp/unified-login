using System;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository
{
    public interface IIdentityProviderRepository
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

		/// <summary>
		/// List Identity Providers by Provider TypeId
		/// </summary>
		/// <param name="IdentityProviderTypeId">Identity Provider TypeId</param>
		/// <returns>Provider Configuration object</returns>
		[Obsolete("This method is no longer used.")]
		IList<ProviderConfiguration> ListIdentityProviderByIdentityProviderTypeId(int IdentityProviderTypeId);

        /// <summary>
		/// create a new global type. Currently, we have Azure AD, Okta, and Identity Server
		/// </summary>
		/// <param name="identityProviderSetting">IdentityProviderSetting Object.</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse CreateIdentityProviderSetting(IIdentityProviderSetting identityProviderSetting);

        /// <summary>
		/// create a new global type. Currently, we have Azure AD, Okta, and Identity Server
		/// </summary>
		/// <param name="identityProviderSettingType">IdentityProviderSettingType Object.</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse CreateIdentityProviderSettingType(IIdentityProviderSettingType identityProviderSettingType);

        /// <summary>
		/// create a new global type. Currently, we have Azure AD, Okta, and Identity Server
		/// </summary>
		/// <param name="identityProviderType">IdentityProviderType Object.</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse CreateIdentityProviderType(IIdentityProviderType identityProviderType);
	}
}