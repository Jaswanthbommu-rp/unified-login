using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using DbConnectionEnum = RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Enum.DbConnectionEnum;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository
{
    public class IdentityProviderRepository : BaseRepository, IIdentityProviderRepository
    {
        #region Ctor

        public IdentityProviderRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }

        #endregion

        #region IIdentityProviderRepository Implementation

        /// <summary>
        /// Get the Identity Provider Type by User login name
        /// </summary>
        /// <param name="enterpriseUserName">User login Name.</param>
        /// <returns>Identity Provider Type object</returns>
        public IdentityProviderType GetProviderByEnterpriseUserName(string enterpriseUserName)
        {
            using (var repository = GetRepository())
            {
                var result = repository.GetOne<string>(StoredProcNameConstants.SP_GetIdentityProviderTypeByLoginName, new { loginName = enterpriseUserName });
                var authType = new IdentityProviderType { AuthenticationType = result };
                return authType;
            }
        }

        /// <summary>
        /// Get Provider configurations by name
        /// </summary>
        /// <param name="providerName">Provider Name</param>
        /// <returns>Provider Configuration object</returns>
        public IList<ProviderConfiguration> GetProviderConfigurationByName(string providerName)
        {
            using (var repository = GetRepository())
            {
                providerName = providerName.ToLower();
                return repository.GetMany<ProviderConfiguration>(StoredProcNameConstants.SP_ListIdentityProviderByIdentityProviderTypeName, new { IdentityProviderTypeName = providerName }).ToList();
            }
        }

		/// <summary>
		/// List Identity Providers by Provider TypeId
		/// </summary>
		/// <param name="IdentityProviderTypeId">Identity Provider TypeId</param>
		/// <returns>Provider Configuration object</returns>
		[Obsolete("This method is no longer used.")]
		public IList<ProviderConfiguration> ListIdentityProviderByIdentityProviderTypeId(int IdentityProviderTypeId)
		{
			dynamic param = new
			{
				IdentityProviderTypeId
			};

			using (var repository = GetRepository())
			{
				var result = repository.GetMany<ProviderConfiguration>(StoredProcNameConstants.SP_ListIdentityProviderByIdentityProviderTypeId, param);
				return result;
			}
		}

		/// <summary>
		/// create a new global type. Currently, we have Azure AD, Okta, and Identity Server
		/// </summary>
		/// <param name="identityProviderSetting">IdentityProviderSetting Object.</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse CreateIdentityProviderSetting(IIdentityProviderSetting identityProviderSetting)
        {
            dynamic param = new
            {
                IdentityProviderSettingTypeId = identityProviderSetting.IdentityProviderSettingTypeId,
                Value = identityProviderSetting.Value
            };

            using (var repository = GetRepository())
            {
                var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateIdentityProviderSettingType, param);
                return result;
            }
        }

        /// <summary>
		/// create a new global type. Currently, we have Azure AD, Okta, and Identity Server
		/// </summary>
		/// <param name="identityProviderSettingType">IdentityProviderSettingType Object.</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse CreateIdentityProviderSettingType(IIdentityProviderSettingType identityProviderSettingType)
        {
            dynamic param = new
            {
                IdentityProviderTypeId = identityProviderSettingType.IdentityProviderTypeId,
                Name = identityProviderSettingType.Name
            };

            using (var repository = GetRepository())
            {
                var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateIdentityProviderSettingType, param);
                return result;
            }
        }

        /// <summary>
		/// create a new global type. Currently, we have Azure AD, Okta, and Identity Server
		/// </summary>
		/// <param name="identityProviderType">IdentityProviderType Object.</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse CreateIdentityProviderType(IIdentityProviderType identityProviderType)
        {
            dynamic param = new
            {
                //Name = identityProviderType.Name,
                //Description = identityProviderType.Description
            };

            using (var repository = GetRepository())
            {
                var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateIdentityProviderType, param);
                return result;
            }
        }
		#endregion
	}
}