using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

public sealed class ManageMicrosoftAzureAsync : IManageMicrosoftAzureAsync
{
    private readonly IProductInternalSettingRepositoryAsync _productInternalSettingRepo;
    private readonly ITokenHelperAsync                      _tokenHelper;
    private readonly IHttpClientFactory                     _httpClientFactory;
    private readonly IUserClaimsAccessor                    _userClaims;
    private readonly ILogger<ManageMicrosoftAzureAsync>     _logger;

    public ManageMicrosoftAzureAsync(
        IProductInternalSettingRepositoryAsync productInternalSettingRepo,
        ITokenHelperAsync                      tokenHelper,
        IHttpClientFactory                     httpClientFactory,
        IUserClaimsAccessor                    userClaims,
        ILogger<ManageMicrosoftAzureAsync>     logger)
    {
        _productInternalSettingRepo = productInternalSettingRepo ?? throw new ArgumentNullException(nameof(productInternalSettingRepo));
        _tokenHelper                = tokenHelper                ?? throw new ArgumentNullException(nameof(tokenHelper));
        _httpClientFactory          = httpClientFactory          ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _userClaims                 = userClaims                 ?? throw new ArgumentNullException(nameof(userClaims));
        _logger                     = logger                     ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AzureUser?> GetADUserInfoAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await _productInternalSettingRepo
                .GetProductInternalSettingsAsync(3, cancellationToken)
                .ConfigureAwait(false);

            var tokenAddress = GetRequired(settings, "AzureTokenAddress");
            var clientSecret = GetRequired(settings, "AzureUnifiedLoginUserClientSecret");
            var clientId     = GetRequired(settings, "AzureUnifiedLoginUserClientId");
            var scopes       = GetRequired(settings, "AzureUnifiedLoginUserClientScopes");
            var graphApiBase = GetRequired(settings, "AzureUserGraphAPI");

            var token = await _tokenHelper
                .GetExternalClientCredentialServerTokenAsync(
                    $"{tokenAddress}/token", clientId, clientSecret, scopes, cancellationToken)
                .ConfigureAwait(false);

            var requestUrl = $"{graphApiBase}/v1.0/users" +
                $"?$filter=userPrincipalName eq '{userName}'" +
                $"&$select=userPrincipalName,onPremisesSamAccountName,displayName,mail,id";

            _logger.LogDebug("{CorrelationId} GetADUserInfoAsync Url={Url}",
                _userClaims.CorrelationId, requestUrl);

            using var client  = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var json     = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                var userInfo = JsonConvert.DeserializeObject<AzureUser>(json);

                if (userInfo?.value?.FirstOrDefault()
                        ?.userPrincipalName.Equals(userName, StringComparison.OrdinalIgnoreCase) == true)
                    return userInfo;
            }
            else
            {
                _logger.LogWarning("{CorrelationId} GetADUserInfoAsync non-success StatusCode={StatusCode} UserName={UserName}",
                    _userClaims.CorrelationId, response.StatusCode, userName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "{CorrelationId} GetADUserInfoAsync failed UserName={UserName}",
                _userClaims.CorrelationId, userName);
        }

        return null;
    }

    // ── Private static helpers ─────────────────────────────────────────────

    private static string GetRequired(IList<ProductInternalSetting> settings, string name)
    {
        var value = settings.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;
        return value ?? throw new InvalidOperationException(
            $"Required Azure setting '{name}' is missing from product internal settings (productId=3).");
    }
}
