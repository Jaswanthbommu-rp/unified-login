using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess; // IRepository lives in this namespace
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Logic.Enterprise.Helpers
{
    public interface ITokenHelper
    {
        string GetUnifiedLoginServerToken(string scopes);
        string GetClientCredentialServerToken(string clientId, string clientSecret, string scopes);
        string GetExternalClientCredentialServerToken(string tokenUri, string clientId, string clientSecret, string scopes);
    }

    /// <summary>
    /// Helper to obtain client credential tokens (OAuth2) using raw HTTP calls.
    /// Eliminates dependency on legacy IdentityModel TokenClient API.
    /// </summary>
    public class TokenHelper : ITokenHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly IProductInternalSettingRepository _productRepository;

        public TokenHelper()
        {
            _productRepository = new ProductInternalSettingRepository();
        }

        public TokenHelper(IRepository repository)
        {
            _productRepository = new ProductInternalSettingRepository(repository);
        }

        public string GetUnifiedLoginServerToken(string scopes)
        {
            var settings = GetProductInternalSettings(ProductEnum.UnifiedPlatform);
            try
            {
                string tokenEndPoint = settings.First(a => a.Name.Equals("TokenEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
                string clientId = settings.First(a => a.Name.Equals("UnifiedLoginServerClientName", StringComparison.OrdinalIgnoreCase)).Value;
                string apiSecretRaw = settings.First(a => a.Name.Equals("UnifiedLoginServerClientSecret", StringComparison.OrdinalIgnoreCase)).Value;
                string apiSecret = TryFromBase64(apiSecretRaw);

                RPObjectCache rpCache = new RPObjectCache();
                var cacheKey = $"GetUnifiedLoginServerToken_{clientId}_{scopes}";

                string accessToken = rpCache.GetFromCache<string>(cacheKey, 300, () =>
                {
                    return RequestClientCredentialsToken(tokenEndPoint, clientId, apiSecret, scopes);
                });

                return accessToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in TokenHelper.GetUnifiedLoginServerToken - {ex.Message}");
            }
        }

        public string GetClientCredentialServerToken(string clientId, string clientSecret, string scopes)
        {
            try
            {
                string issuerUri = ConfigReader.GetIssuerUri;
                RPObjectCache rpCache = new RPObjectCache();
                var cacheKey = $"GetClientCredentialServerToken_{issuerUri}_{clientId}_{scopes}";

                string accessToken = rpCache.GetFromCache<string>(cacheKey, 300, () =>
                {
                    var endpoint = issuerUri.TrimEnd('/') + "/connect/token";
                    return RequestClientCredentialsToken(endpoint, clientId, clientSecret, scopes);
                });

                return accessToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in TokenHelper.GetClientCredentialServerToken - {ex.Message}");
            }
        }

        public string GetExternalClientCredentialServerToken(string tokenUri, string clientId, string clientSecret, string scopes)
        {
            try
            {
                RPObjectCache rpCache = new RPObjectCache();
                var issuerHash = tokenUri.GetHashCode();
                var cacheKey = $"GetExternalClientCredentialServerToken_{issuerHash}_{clientId}_{scopes}";

                string accessToken = rpCache.GetFromCache<string>(cacheKey, 300, () =>
                {
                    return RequestClientCredentialsToken(tokenUri, clientId, clientSecret, scopes);
                });

                return accessToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in TokenHelper.GetExternalClientCredentialServerToken - {ex.Message}");
            }
        }

        private static string RequestClientCredentialsToken(string tokenEndpoint, string clientId, string clientSecret, string scopes)
        {
            if (string.IsNullOrWhiteSpace(tokenEndpoint)) throw new ArgumentException("Token endpoint required", nameof(tokenEndpoint));
            if (string.IsNullOrWhiteSpace(clientId)) throw new ArgumentException("ClientId required", nameof(clientId));
            if (string.IsNullOrWhiteSpace(clientSecret)) throw new ArgumentException("ClientSecret required", nameof(clientSecret));

            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var bodyPairs = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials"),
                new("client_id", clientId),
                new("client_secret", clientSecret),
                new("scope", scopes ?? string.Empty)
            };
            request.Content = new FormUrlEncodedContent(bodyPairs);

            using var response = _httpClient.Send(request);
            if (!response.IsSuccessStatusCode)
            {
                var rawError = response.Content.ReadAsStringAsync().Result;
                throw new Exception($"Token request failed {(int)response.StatusCode} {response.ReasonPhrase}. Body: {rawError}");
            }

            var json = response.Content.ReadAsStringAsync().Result;
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("access_token", out var accessTokenElement))
                {
                    throw new Exception("access_token field missing in response");
                }
                var accessToken = accessTokenElement.GetString();
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    throw new Exception("access_token empty");
                }
                return accessToken;
            }
            catch (JsonException jex)
            {
                throw new Exception($"Invalid JSON token response: {jex.Message}. Raw: {json}");
            }
        }

        private List<ProductInternalSetting> GetProductInternalSettings(ProductEnum product)
        {
            var rpcache = new RPObjectCache();
            var cacheKey = $"productInternalSetting_{(int)product}";
            var productInternalSettingList = rpcache.GetFromCache(cacheKey, 120, () =>
            {
                return _productRepository.GetProductInternalSettings((int)product).ToList();
            });
            return productInternalSettingList ?? new List<ProductInternalSetting>();
        }

        private static string TryFromBase64(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw;
            try
            {
                // If not valid Base64, return original
                var bytes = Convert.FromBase64String(raw);
                var decoded = Encoding.UTF8.GetString(bytes);
                // Heuristic: if decoded contains non-control printable characters, use it
                return decoded.Any(ch => char.IsLetterOrDigit(ch)) ? decoded : raw;
            }
            catch
            {
                return raw;
            }
        }
    }
}
