using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
	/// <summary>
	/// Interface for ManageConfigurationSetting
	/// </summary>
	public interface IManageConfigurationSetting
	{
		/// <summary>
		/// Get a list of Configuration Settings
		/// </summary>
		/// <param name="PartyId">Unique partyID (Person, Organization,...)</param>
		/// <param name="SettingName">Setting Name (DarkNavigation)</param>
		/// <returns>List of User Configuration Settings</returns>
		IList<ConfigurationSetting> ListUserLoginConfigurationSetting(long PartyId, string SettingName);

        /// <summary>
		/// Get a list of Organization Configuration Settings
		/// </summary>
		/// <param name="PartyId">Unique partyID (Person, Organization,...)</param>
		/// <param name="SettingName">Setting Name (DarkNavigation)</param>
		/// <returns>List of Organization Configuration Settings</returns>
		IList<ConfigurationSetting> ListOrganizationConfigurationSetting(long PartyId, string SettingName);
        /// <summary>
        /// Update an existing Configuration Setting
        /// </summary>
        /// <param name="configurationSetting">Master Configuration setting object</param>
        /// <returns>Repository response object</returns>
        RepositoryResponse UpdateConfigurationSetting(ConfigurationSetting configurationSetting);

        /// <summary>
        /// Update an existing Configuration Setting
        /// </summary>
        /// <param name="masterConfigurationSetting">Master Configuration setting object</param>
        /// <returns>Repository response object</returns>
        RepositoryResponse CreateMasterConfigurationSetting(MasterConfigurationSetting masterConfigurationSetting);
    }
}