using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Logic
{
	public class ManageProvider : IManageProvider
    {
        #region Private Variables

        private IIdentityProviderRepository _identityProviderRepository;

        #endregion

        #region Ctor

        public ManageProvider()
        {
            _identityProviderRepository = new IdentityProviderRepository();
        }

        public ManageProvider(IIdentityProviderRepository identityProviderRepository)
        {
            _identityProviderRepository = identityProviderRepository;
        }

        #endregion

        #region IManageProvider Implementation

        /// <summary>
        /// Get the Identity Provider Type detail by User login name
        /// </summary>
        /// <param name="enterpriseUserName">User login Name.</param>
        /// <returns>Identity Provider Type object</returns>
        public IdentityProviderType GetProviderByEnterpriseUserName(string enterpriseUserName)
        {
            return _identityProviderRepository.GetProviderByEnterpriseUserName(enterpriseUserName);
        }

        /// <summary>
        /// Get Provider configurations by name
        /// </summary>
        /// <param name="providerName">Provider Name</param>
        /// <returns>Provider Configuration object</returns>
        public IList<ProviderConfiguration> GetProviderConfigurationByName(string providerName)
        {
            return _identityProviderRepository.GetProviderConfigurationByName(providerName);
        }
		#endregion
	}
}