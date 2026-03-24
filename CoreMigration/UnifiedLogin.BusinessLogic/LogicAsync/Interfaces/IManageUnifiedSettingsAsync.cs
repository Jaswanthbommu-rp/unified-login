using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for all Unified Settings operations.
/// Replaces: sync <see cref="IManageUnifiedSettings"/> + blocking <c>.Result</c> calls.
/// </summary>
public interface IManageUnifiedSettingsAsync
{
    /// <summary>
    /// Returns company settings, serving from a 120-second memory cache on repeated calls.
    /// </summary>
    Task<IList<Setting>> GetUnifiedSettingsCachedAsync(
        long partyId, string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns company settings directly from the repository (no cache).
    /// </summary>
    Task<IList<Setting>> GetUnifiedSettingsAsync(
        long partyId, string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a company instance in the Settings API.
    /// <paramref name="requestType"/> should be <see cref="HttpMethod.Post"/> or <see cref="HttpMethod.Put"/>.
    /// </summary>
    Task<bool> CreateUpdateCompanyInSettingAsync(
        UnifiedSettingCompanyPropertyPayload upfmCompany, HttpMethod requestType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a property instance in the Settings API.
    /// <paramref name="requestType"/> should be <see cref="HttpMethod.Post"/> or <see cref="HttpMethod.Put"/>.
    /// </summary>
    Task<bool> CreateUpdatePropertyInSettingAsync(
        UnifiedSettingCompanyPropertyPayload upfmProperties, HttpMethod requestType,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a property instance from the Settings API by its instance ID.</summary>
    Task<bool> DeletePropertyInSettingAsync(
        string settingsPropertyInstanceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches company-level internal settings via the Kong gateway.
    /// Returns an empty <see cref="InternalSettingResponse"/> when the settings are not found
    /// or the required configuration is missing.
    /// </summary>
    Task<InternalSettingResponse> GetCompanyInternalSettingsAsync(
        Guid companyId, string source, string settingType,
        CancellationToken cancellationToken = default);
}