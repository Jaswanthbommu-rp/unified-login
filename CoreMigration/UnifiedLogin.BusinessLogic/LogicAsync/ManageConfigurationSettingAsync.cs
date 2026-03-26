using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper around IManageConfigurationSetting.
/// Task.FromResult() will be replaced with truly-async repo calls in a future pass.
/// </summary>
public sealed class ManageConfigurationSettingAsync : IManageConfigurationSettingAsync
{
    #region Private Fields
    private readonly IConfigurationSettingRepositoryAsync _configurationSettingRepository;
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of <see cref="ManageConfigurationSetting"/>.
    /// </summary>
    /// <param name="configurationSettingRepository">Async configuration setting repository</param>
    /// <exception cref="ArgumentNullException">Thrown when repository is null</exception>
    public ManageConfigurationSettingAsync(IConfigurationSettingRepositoryAsync configurationSettingRepository)
    {
        _configurationSettingRepository = configurationSettingRepository
            ?? throw new ArgumentNullException(nameof(configurationSettingRepository));
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// Gets user-level configuration settings for the specified party.
    /// </summary>
    /// <param name="partyId">Unique party ID (Person, Organization, etc.)</param>
    /// <param name="settingName">Setting name (e.g., DarkNavigation). Pass null to retrieve all settings.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user configuration settings</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when partyId is 0 or negative</exception>
    public async Task<IList<ConfigurationSetting>> ListUserLoginConfigurationSettingAsync(
        long partyId,
        string? settingName,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(partyId, 0, nameof(partyId));

        return await _configurationSettingRepository
            .ListUserLoginConfigurationSettingAsync(partyId, settingName, cancellationToken);
    }

    /// <summary>
    /// Gets organization-level configuration settings for the specified party.
    /// </summary>
    /// <param name="partyId">Unique party ID (Organization)</param>
    /// <param name="settingName">Setting name. Pass null to retrieve all settings.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of organization configuration settings</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when partyId is 0 or negative</exception>
    public async Task<IList<ConfigurationSetting>> ListOrganizationConfigurationSettingAsync(
        long partyId,
        string? settingName,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(partyId, 0, nameof(partyId));

        return await _configurationSettingRepository
            .ListOrganizationConfigurationSettingAsync(partyId, settingName, cancellationToken);
    }

    /// <summary>
    /// Updates an existing configuration setting.
    /// </summary>
    /// <param name="configurationSetting">Configuration setting object to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Repository response indicating success or failure</returns>
    /// <exception cref="ArgumentNullException">Thrown when configurationSetting is null</exception>
    public async Task<RepositoryResponse> UpdateConfigurationSettingAsync(
        ConfigurationSetting configurationSetting,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configurationSetting);

        return await _configurationSettingRepository
            .UpdateConfigurationSettingAsync(configurationSetting, cancellationToken);
    }

    /// <summary>
    /// Creates a new master configuration setting.
    /// </summary>
    /// <param name="masterConfigurationSetting">Master configuration setting object to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Repository response indicating success or failure</returns>
    /// <exception cref="ArgumentNullException">Thrown when masterConfigurationSetting is null</exception>
    public async Task<RepositoryResponse> CreateMasterConfigurationSettingAsync(
        MasterConfigurationSetting masterConfigurationSetting,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(masterConfigurationSetting);

        return await _configurationSettingRepository
            .CreateMasterConfigurationSettingAsync(masterConfigurationSetting, cancellationToken);
    }

    #endregion
}
