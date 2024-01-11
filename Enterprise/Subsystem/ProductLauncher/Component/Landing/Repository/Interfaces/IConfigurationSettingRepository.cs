using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces
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
		/// Adds a use primary properties master configuration setting value
		/// </summary>
		/// <param name="masterSetting">Master Configuration setting object</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse CreateUsePrimaryPropertyMasterConfigurationSetting(MasterConfigurationSetting masterSetting);

		/// <summary>
		/// Adds a Enterprise Role master configuration setting value
		/// </summary>
		/// <param name="masterSetting">Master Configuration setting object</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse CreateEnterpriseRoleMasterConfigurationSetting(MasterConfigurationSetting masterSetting);


    }
}