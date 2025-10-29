using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for Configuration Setting Repository
	/// </summary>
	public interface IConfigurationSettingRepository
	{
		/// <summary>
		/// Get a list of User Configuration Setting
		/// </summary>
		/// <param name="PartyId">Unique partyID (Person, Organization,...)</param>
		/// <param name="SettingName">Setting Name (DarkNavigation)</param>
		/// <returns>List of User Configuration Settings</returns>
		IList<ConfigurationSetting> ListUserLoginConfigurationSetting(long PartyId, string SettingName);

        /// Get a list of Organization Configuration Setting
		/// <param name="PartyId">Unique partyID (Person, Organization,...)</param>
		/// <param name="SettingName">Setting Name (DarkNavigation)</param>
		/// <returns>List of Organization Configuration Settings</returns>
		IList<ConfigurationSetting> ListOrganizationConfigurationSetting(long PartyId, string SettingName);
        

        /// <summary>
        /// Update Configuration setting value
        /// </summary>
        /// <param name="configurationSetting">Master Configuration setting object</param>
        /// <returns>Repository response object</returns>
        RepositoryResponse UpdateConfigurationSetting(ConfigurationSetting configurationSetting);

        /// <summary>
        /// Add a new master configuration setting value
        /// </summary>
        /// <param name="masterSetting">Master Configuration setting object</param>
        /// <returns>Repository response object</returns>
        RepositoryResponse CreateMasterConfigurationSetting(MasterConfigurationSetting masterSetting);
		/// <summary>
		/// Gets master configuration id for given org
		/// </summary>
		/// <param name="masterSetting">Master Configuration setting object</param>
		/// <returns>Repository response object</returns>
		int GetOrganizationMasterConfigurationId(string Name, long PartyId);

		/// <summary>
		/// Add or update a primary properties or enterprise role master configuration setting value
		/// </summary>
		/// <param name="masterSetting">Master Configuration setting object</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSetting(MasterConfigurationSetting masterSetting);




    }
}