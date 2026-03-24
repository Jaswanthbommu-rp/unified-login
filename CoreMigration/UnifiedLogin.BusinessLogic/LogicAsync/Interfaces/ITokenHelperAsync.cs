namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for OAuth2 client-credentials token acquisition.
/// Replaces: sync <see cref="ITokenHelper"/> + blocking <c>_httpClient.Send()</c> calls.
/// </summary>
public interface ITokenHelperAsync
{
    /// <summary>
    /// Acquires a client-credentials token for the Unified Login server using settings
    /// stored in the product internal settings table.
    /// Result is cached for 5 minutes under a key derived from <c>clientId</c> + <paramref name="scopes"/>.
    /// </summary>
    Task<string> GetUnifiedLoginServerTokenAsync(
        string scopes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Acquires a client-credentials token from the issuer configured in
    /// <c>ConfigReader.GetIssuerUri</c>.
    /// Result is cached for 5 minutes.
    /// </summary>
    Task<string> GetClientCredentialServerTokenAsync(
        string clientId, string clientSecret, string scopes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Acquires a client-credentials token from an arbitrary <paramref name="tokenUri"/>.
    /// Result is cached for 5 minutes.
    /// </summary>
    Task<string> GetExternalClientCredentialServerTokenAsync(
        string tokenUri, string clientId, string clientSecret, string scopes,
        CancellationToken cancellationToken = default);
}