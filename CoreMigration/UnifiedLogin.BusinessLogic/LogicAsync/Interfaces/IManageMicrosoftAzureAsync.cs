using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for Microsoft Azure AD user lookups.
/// </summary>
public interface IManageMicrosoftAzureAsync
{
    /// <summary>
    /// Returns Azure AD user information for the given user principal name,
    /// or <c>null</c> when the user is not found or the request fails.
    /// </summary>
    Task<AzureUser?> GetADUserInfoAsync(
        string userName,
        CancellationToken cancellationToken = default);
}
