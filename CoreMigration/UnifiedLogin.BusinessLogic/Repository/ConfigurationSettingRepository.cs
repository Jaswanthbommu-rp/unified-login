using Dapper;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using System.Collections.Generic;
using System.Data;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Repository
{
    /// <summary>
    /// Configuration Setting Repository
    /// </summary>
    public class ConfigurationSettingRepository : BaseRepository, IConfigurationSettingRepository
    {
        #region Constructor
        /// <summary>
        /// Contact Mechanism UsageType base Constructor
        /// </summary>
        public ConfigurationSettingRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        public ConfigurationSettingRepository(IRepository repository) : base(repository)
        {
        }
        #endregion

        #region public Configuration Setting Repository methods
        /// <summary>
        /// Get a list of User Configuration Setting
        /// </summary>
        /// <param name="PartyId">Unique partyID (Person, Organization,...)</param>
        /// <param name="SettingName">Setting Name (DarkNavigation)</param>
        /// <returns>List of User Configuration Settings</returns>
        public IList<ConfigurationSetting> ListUserLoginConfigurationSetting(long PartyId, string SettingName)
        {
            dynamic param = new
            {
                @PartyId = PartyId,
                @SettingName = SettingName
            };

            try
            {
                using (var repository = GetRepository())
                {
                    IList<ConfigurationSetting> result = repository.GetMany<ConfigurationSetting>(StoredProcNameConstants.SP_ListUserLoginSettings, param);
                    return result;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
		/// Get a list of Organization Configuration Setting
		/// </summary>
		/// <param name="PartyId">Unique partyID (Person, Organization,...)</param>
		/// <param name="SettingName">Setting Name (DarkNavigation)</param>
		/// <returns>List of User Configuration Settings</returns>
		public IList<ConfigurationSetting> ListOrganizationConfigurationSetting(long PartyId, string SettingName)
        {
            RPObjectCache rpCache = new RPObjectCache();
            var cacheKey = $"sp_ListOrganizationSettings_{PartyId}_{SettingName}";

            IList<ConfigurationSetting> getSettings = rpCache.GetFromCache<List<ConfigurationSetting>>(cacheKey, 360, () =>
            {
                dynamic param = new
                {
                    @PartyId = PartyId,
                    @SettingName = SettingName
                };

                try
                {
                    using (var repository = GetRepository())
                    {
                        return repository.GetMany<ConfigurationSetting>(StoredProcNameConstants.SP_ListOrganizationSettings, param);
                    }
                }
                catch
                {
                    return null;
                }
            });

            return getSettings;
        }

        /// <summary>
        /// Update Configuration setting value
        /// </summary>
        /// <param name="configurationSetting">Master Configuration setting object</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse UpdateConfigurationSetting(ConfigurationSetting configurationSetting)
        {
            dynamic param = new
            {
                @MasterConfigurationSettingId = configurationSetting.MasterConfigurationSettingId,
                @Value = configurationSetting.Value
            };

            using (var repository = GetRepository())
            {
                var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateMasterConfigurationSetting, param);
                return result;
            }
        }

        /// <summary>
        /// Add a new master configuration setting value
        /// </summary>
        /// <param name="masterSetting">Master Configuration setting object</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse CreateMasterConfigurationSetting(MasterConfigurationSetting masterSetting)
        {
            dynamic param = new
            {
                @MasterConfigurationType = masterSetting.ConfigurationType,
                @MasterSettingType = masterSetting.SettingType,
                @PartyId = masterSetting.PartyId,
                @Value = masterSetting.Value
            };

            using (var repository = GetRepository())
            {
                var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateMasterConfigurationSetting, param);
                return result;
            }
        }

        /// <summary>
        /// Add a use primary properties or enterprise role master configuration setting value
        /// </summary>
        /// <param name="masterSetting">Master Configuration setting object</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSetting(MasterConfigurationSetting masterSetting)
        {
            dynamic param = new
            {
                @PartyId = masterSetting.PartyId,
                @MappingName = masterSetting.MappingName,
                @Value = masterSetting.Value,
                @CreatedBy = masterSetting.CreatedBy
            };

            using (var repository = GetRepository())
            {
                var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSetting, param);
                return result;
            }
        }

        /// <summary>
        /// Returns the id of ProductSettingType
        /// <param name="productSettingName">productSettingName</param>
        /// </summary>
        public int GetOrganizationMasterConfigurationId(string Name, long PartyId)
        {
            using (var repository = GetRepository())
            {
                int masterConfigurationId = 0;

               
                DynamicParameters param = new DynamicParameters();
                param.Add("@PartyId", PartyId, dbType: DbType.Int32, direction: ParameterDirection.Input);
                param.Add("@Name", Name, dbType: DbType.String, direction: ParameterDirection.Input);
                param.Add("@MasterConfigurationId", masterConfigurationId, dbType: DbType.Int32, direction: ParameterDirection.Output);

                try
                {
                    repository.Execute(StoredProcNameConstants.SP_GetOrganizationMasterConfiguration, param);
                    masterConfigurationId = param.Get<int>("@MasterConfigurationId");
                }
                catch
                {
                }

                return masterConfigurationId;
            }
        }
        #endregion
    }
}