using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

public interface IManageConfigurationSettingAsync
{
    Task<IList<ConfigurationSetting>> ListUserLoginConfigurationSettingAsync(long partyId, string settingName, CancellationToken cancellationToken = default);
    Task<IList<ConfigurationSetting>> ListOrganizationConfigurationSettingAsync(long partyId, string settingName, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> UpdateConfigurationSettingAsync(ConfigurationSetting configurationSetting, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> CreateMasterConfigurationSettingAsync(MasterConfigurationSetting masterConfigurationSetting, CancellationToken cancellationToken = default);
}
