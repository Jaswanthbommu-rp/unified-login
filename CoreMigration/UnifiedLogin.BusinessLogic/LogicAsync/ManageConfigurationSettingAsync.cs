using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper around IManageConfigurationSetting.
/// Task.FromResult() will be replaced with truly-async repo calls in a future pass.
/// </summary>
public sealed class ManageConfigurationSettingAsync : IManageConfigurationSettingAsync
{
    private readonly IManageConfigurationSetting _manageConfigurationSetting;

    public ManageConfigurationSettingAsync(IManageConfigurationSetting manageConfigurationSetting)
    {
        _manageConfigurationSetting = manageConfigurationSetting ?? throw new ArgumentNullException(nameof(manageConfigurationSetting));
    }

    public Task<IList<ConfigurationSetting>> ListUserLoginConfigurationSettingAsync(long partyId, string settingName, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageConfigurationSetting.ListUserLoginConfigurationSetting(partyId, settingName));

    public Task<IList<ConfigurationSetting>> ListOrganizationConfigurationSettingAsync(long partyId, string settingName, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageConfigurationSetting.ListOrganizationConfigurationSetting(partyId, settingName));

    public Task<RepositoryResponse> UpdateConfigurationSettingAsync(ConfigurationSetting configurationSetting, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageConfigurationSetting.UpdateConfigurationSetting(configurationSetting));

    public Task<RepositoryResponse> CreateMasterConfigurationSettingAsync(MasterConfigurationSetting masterConfigurationSetting, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageConfigurationSetting.CreateMasterConfigurationSetting(masterConfigurationSetting));
}
