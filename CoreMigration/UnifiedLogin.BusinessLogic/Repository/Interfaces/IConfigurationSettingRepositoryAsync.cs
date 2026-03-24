using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for Configuration Setting Repository
	/// </summary>
	public interface IConfigurationSettingRepositoryAsync
	{
		/// <summary>
		/// Get a list of User Configuration Setting
		/// </summary>
		/// <param name="PartyId">Unique partyID (Person, Organization,...)</param>
		/// <param name="SettingName">Setting Name (DarkNavigation)</param>
		/// <returns>List of User Configuration Settings</returns>
		Task<IList<ConfigurationSetting>> ListUserLoginConfigurationSettingAsync(long PartyId, string SettingName, CancellationToken cancellationToken = default);

        /// Get a list of Organization Configuration Setting
		/// <param name="PartyId">Unique partyID (Person, Organization,...)</param>
		/// <param name="SettingName">Setting Name (DarkNavigation)</param>
		/// <returns>List of Organization Configuration Settings</returns>
		Task<IList<ConfigurationSetting>> ListOrganizationConfigurationSettingAsync(long PartyId, string SettingName, CancellationToken cancellationToken = default	);
        

        /// <summary>
        /// Update Configuration setting value
        /// </summary>
        /// <param name="configurationSetting">Master Configuration setting object</param>
        /// <returns>Repository response object</returns>
        Task<RepositoryResponse> UpdateConfigurationSettingAsync(ConfigurationSetting configurationSetting, CancellationToken cancellationToken = default);

        /// <summary>
        /// Add a new master configuration setting value
        /// </summary>
        /// <param name="masterSetting">Master Configuration setting object</param>
        /// <returns>Repository response object</returns>
        Task<RepositoryResponse> CreateMasterConfigurationSettingAsync(MasterConfigurationSetting masterSetting, CancellationToken cancellationToken = default);
		/// <summary>
		/// Gets master configuration id for given org
		/// </summary>
		/// <param name="masterSetting">Master Configuration setting object</param>
		/// <returns>Repository response object</returns>
		Task<int> GetOrganizationMasterConfigurationIdAsync(string Name, long PartyId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Add or update a primary properties or enterprise role master configuration setting value
		/// </summary>
		/// <param name="masterSetting">Master Configuration setting object</param>
		/// <returns>Repository response object</returns>
		Task<RepositoryResponse> CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSettingAsync(MasterConfigurationSetting masterSetting, CancellationToken cancellationToken = default);




    }
}