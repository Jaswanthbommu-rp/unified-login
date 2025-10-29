using System;
using System.Collections.Generic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic
{
	/// <summary>
	/// Manage Configuration Setting repository calls
	/// </summary>
	public class ManageConfigurationSetting : IManageConfigurationSetting
	{
		#region Private Variables
		IConfigurationSettingRepository _configurationSettingRepository;
		#endregion

		#region Constructors
		/// <summary>
		/// ManageConfigurationSetting Constructor
		/// </summary>
		/// <param name="configurationSettingRepository">Configuration Setting Repository</param>
		public ManageConfigurationSetting(IConfigurationSettingRepository configurationSettingRepository)
		{
			_configurationSettingRepository = configurationSettingRepository;
		}

		/// <summary>
		/// Create a basic instance of the ManageConfigurationSetting Controller class
		/// </summary>
		public ManageConfigurationSetting()
		{
			_configurationSettingRepository = new ConfigurationSettingRepository();
		}
		#endregion

		#region Public ManageConfigurationSetting methods
		/// <summary>
		/// Get a list of user Configuration Settings
		/// </summary>
		/// <param name="PartyId">Unique partyID (Person, Organization,...)</param>
		/// <param name="SettingName">Setting Name (DarkNavigation)</param>
		/// <returns>List of User Configuration Settings</returns>
		public IList<ConfigurationSetting> ListUserLoginConfigurationSetting(long PartyId, string SettingName)
		{
			if (PartyId == 0)
			{
				throw new Exception("Invalid parameter PartyId.");
			}

			return _configurationSettingRepository.ListUserLoginConfigurationSetting(PartyId, SettingName);
		}

        /// <summary>
		/// Get a list of user Organization Settings
		/// </summary>
		/// <param name="PartyId">Unique partyID (Person, Organization,...)</param>
		/// <param name="SettingName">Setting Name (DarkNavigation)</param>
		/// <returns>List of Organization Configuration Settings</returns>
		public IList<ConfigurationSetting> ListOrganizationConfigurationSetting(long PartyId, string SettingName)
        {
            if (PartyId == 0)
            {
                throw new Exception("Invalid parameter PartyId.");
            }

            return _configurationSettingRepository.ListOrganizationConfigurationSetting(PartyId, SettingName);
        }

        /// <summary>
        /// Update an existing Configuration Setting
        /// </summary>
        /// <param name="configurationSetting">Master Configuration setting object</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse UpdateConfigurationSetting(ConfigurationSetting configurationSetting)
		{
			if (configurationSetting == null)
			{
				throw new ArgumentNullException(nameof(configurationSetting), "Null ConfigurationSetting.");
			}

			return _configurationSettingRepository.UpdateConfigurationSetting(configurationSetting);
		}

        /// <summary>
        /// Update an existing Configuration Setting
        /// </summary>
        /// <param name="masterConfigurationSetting">Master Configuration setting object</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse CreateMasterConfigurationSetting(MasterConfigurationSetting masterConfigurationSetting)
        {
            if (masterConfigurationSetting == null)
            {
                throw new ArgumentNullException(nameof(masterConfigurationSetting), "Null MasterConfigurationSetting.");
            }

            return _configurationSettingRepository.CreateMasterConfigurationSetting(masterConfigurationSetting);
        }

		#endregion
	}
}