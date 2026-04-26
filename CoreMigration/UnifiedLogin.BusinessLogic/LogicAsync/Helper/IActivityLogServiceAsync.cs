using System.Security.Claims;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Helper;

/// <summary>
/// Async-first, DI-injectable activity logging service.
/// <para>
/// Replaces the static <c>LogActivity</c> helper which used
/// <c>new ProductInternalSettingRepository()</c>, synchronous
/// <c>HttpClient.Send</c> / <c>ReadAsStringAsync().Result</c> anti-patterns,
/// and non-async <c>lock</c>-based token refresh.
/// </para>
/// <para>
/// <b>DI registration:</b> register as <c>Singleton</c> — the service owns its
/// settings cache and bearer-token lifecycle.
/// </para>
/// </summary>
public interface IActivityLogServiceAsync
{
    /// <summary>
    /// Writes an activity entry. Async replacement for <c>LogActivity.WriteActivity</c>.
    /// </summary>
    Task WriteActivityAsync(
        ActivityDetails activityDetails,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes an activity entry sourced from a <see cref="ClaimsPrincipal"/>.
    /// Messages longer than 400 characters are automatically split into chunks.
    /// </summary>
    Task<bool> AddActivityRecordAsync(
        string activityType,
        LogActivityCategoryType activityCategory,
        ClaimsPrincipal user,
        string message,
        string toUserFirstName,
        string toUserLastName,
        long? toUserLoginId,
        string toUserLoginName,
        string toUserRealpageId,
        string productName,
        List<AdditionalParameters>? additionalInformation = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes an activity entry without a <see cref="ClaimsPrincipal"/> (batch / system callers).
    /// Messages longer than 400 characters are automatically split into chunks.
    /// </summary>
    Task<bool> AddActivityRecordWithoutClaimsAsync(
        string activityType,
        LogActivityCategoryType activityCategory,
        string message,
        string firstName,
        string lastName,
        string loginName,
        long userId,
        Guid realPageId,
        long booksMasterOrganizationId,
        long organizationPartyId,
        string toUserFirstName,
        string toUserLastName,
        long? toUserLoginId,
        string toUserLoginName,
        string toUserRealpageId,
        string productName,
        List<AdditionalParameters>? additionalInformation = null,
        CancellationToken cancellationToken = default);
}
