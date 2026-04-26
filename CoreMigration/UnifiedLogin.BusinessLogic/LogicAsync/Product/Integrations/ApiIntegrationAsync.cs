using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;
using System.Text;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations;

/// <summary>
/// Native-async HTTP integration helper. Replaces <see cref="ApiIntegration"/>.
/// <para>
/// <b>IHttpClientFactory pattern</b>: a fresh <see cref="HttpClient"/> is obtained from
/// <see cref="IHttpClientFactory"/> on every call. This lets the factory rotate the
/// underlying <see cref="System.Net.Http.HttpMessageHandler"/> to avoid DNS-stale sockets
/// while still benefiting from handler pooling — eliminating the socket-exhaustion risk
/// of <c>new HttpClient()</c> per request.
/// </para>
/// <para>
/// <b>Authentication</b>: pass an <see cref="AuthenticationHeaderValue"/> to the constructor
/// (built via <see cref="AuthenticationHeaderValue"/> with <c>"Basic"</c> or <c>"Bearer"</c>
/// scheme). It is applied to the per-call client's <c>DefaultRequestHeaders</c> — safe because
/// the factory returns a new client instance on every <c>CreateClient()</c> call.
/// </para>
/// <para>
/// <b>Thread-safety</b>: no mutable instance state beyond the constructor-assigned fields.
/// The same <see cref="ApiIntegrationAsync"/> instance may be called concurrently.
/// </para>
/// </summary>
public sealed class ApiIntegrationAsync : IApiIntegrationAsync
{
    // ── Named client registered in DI as:
    //   services.AddHttpClient(ApiIntegrationAsync.ClientName);
    public const string ClientName = "ProductIntegration";

    // ── Shared serialiser settings (thread-safe after initialisation) ──────────
    // DefaultContractResolver honours [JsonProperty] names exactly — consistent with
    // the sync ApiIntegration and with product APIs that use camelCase property names.
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        ContractResolver      = new DefaultContractResolver(),
        NullValueHandling     = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    private readonly IHttpClientFactory                      _httpClientFactory;
    private readonly string                                  _baseUrlAndQuery;
    private readonly AuthenticationHeaderValue?              _authHeader;
    private readonly IReadOnlyDictionary<string, string>?    _additionalHeaders;
    private readonly ILogger<ApiIntegrationAsync>            _logger;

    /// <param name="httpClientFactory">
    ///   DI-managed factory — register the named client
    ///   <see cref="ClientName"/> (<c>"ProductIntegration"</c>) in <c>Startup</c>.
    /// </param>
    /// <param name="baseUrlAndQuery">Fully-qualified URL (with query string if needed).</param>
    /// <param name="authHeader">
    ///   Optional pre-built auth header (Basic or Bearer).
    ///   Use <c>AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes("user:pass")))</c>
    ///   or <c>AuthenticationHeaderValue("Bearer", token)</c>.
    /// </param>
    /// <param name="additionalHeaders">
    ///   Optional product-specific headers (e.g. <c>apikey</c>, <c>company-id</c>) applied
    ///   to every factory-created client alongside the auth header.
    /// </param>
    public ApiIntegrationAsync(
        IHttpClientFactory                     httpClientFactory,
        string                                 baseUrlAndQuery,
        ILogger<ApiIntegrationAsync>           logger,
        AuthenticationHeaderValue?             authHeader        = null,
        IReadOnlyDictionary<string, string>?   additionalHeaders = null)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrlAndQuery);
        ArgumentNullException.ThrowIfNull(logger);

        _httpClientFactory = httpClientFactory;
        _baseUrlAndQuery   = baseUrlAndQuery;
        _authHeader        = authHeader;
        _additionalHeaders = additionalHeaders;
        _logger            = logger;
    }

    // ── Public interface ──────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<T?> GetEntityFromApiAsync<T>(
        bool isThrowOnError = true,
        CancellationToken ct = default) where T : class
    {
        var client = CreateClient();
        try
        {
            var response = await client.GetAsync(_baseUrlAndQuery, ct);

            if (!response.IsSuccessStatusCode && !isThrowOnError)
                return null;

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonConvert.DeserializeObject<T>(content, JsonSettings);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "{Action} - Deserialisation failed. Url={Url}", nameof(GetEntityFromApiAsync), _baseUrlAndQuery);
            if (isThrowOnError)
                throw new InvalidOperationException(
                    $"Failed to deserialise response from {_baseUrlAndQuery}: {ex.Message}", ex);
            return null;
        }
    }

    /// <inheritdoc/>
    public Task<ApiResponse> PostEntityAsync<T>(
        object jsonToPost,
        bool isThrowOnError = true,
        CancellationToken ct = default)
        => SendWithBodyAsync(HttpMethod.Post, jsonToPost, isThrowOnError, ct);

    /// <inheritdoc/>
    public Task<ApiResponse> PutEntityAsync<T>(
        object jsonToPost,
        bool isThrowOnError = true,
        CancellationToken ct = default) where T : class
        => SendWithBodyAsync(HttpMethod.Put, jsonToPost, isThrowOnError, ct);

    /// <inheritdoc/>
    public async Task<ApiResponse> DeleteEntityAsync<T>(
        bool isThrowOnError = true,
        CancellationToken ct = default) where T : class
    {
        var client = CreateClient();
        try
        {
            var response = await client.DeleteAsync(_baseUrlAndQuery, ct);
            return await BuildApiResponseAsync(response, ct);
        }
        catch (Exception ex) when (!isThrowOnError)
        {
            _logger.LogWarning(ex, "{Action} - Delete failed (suppressed). Url={Url}", nameof(DeleteEntityAsync), _baseUrlAndQuery);
            return new ApiResponse { IsSuccessStatusCode = false, StatusCode = 0, Content = "Delete request failed" };
        }
    }

    /// <inheritdoc/>
    public Task<ApiResponse> PatchEntityAsync<T>(
        object jsonToPost,
        bool isThrowOnError = true,
        CancellationToken ct = default) where T : class
        => SendWithBodyAsync(HttpMethod.Patch, jsonToPost, isThrowOnError, ct);

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Creates a named <see cref="HttpClient"/> from the factory and applies the optional
    /// auth header. Safe to call concurrently — each call gets its own client instance.
    /// </summary>
    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient(ClientName);
        if (_authHeader is not null)
            client.DefaultRequestHeaders.Authorization = _authHeader;
        if (_additionalHeaders is not null)
            foreach (var (key, value) in _additionalHeaders)
                client.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
        return client;
    }

    private async Task<ApiResponse> SendWithBodyAsync(
        HttpMethod method, object body, bool isThrowOnError, CancellationToken ct)
    {
        var client = CreateClient();
        try
        {
            using var request = new HttpRequestMessage(method, _baseUrlAndQuery);
            if (body is not null)
            {
                var json = JsonConvert.SerializeObject(body, JsonSettings);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            var response = await client.SendAsync(request, ct);
            return await BuildApiResponseAsync(response, ct);
        }
        catch (Exception ex) when (!isThrowOnError)
        {
            _logger.LogWarning(ex, "{Action} - {Method} failed (suppressed). Url={Url}",
                nameof(SendWithBodyAsync), method.Method, _baseUrlAndQuery);
            return new ApiResponse { IsSuccessStatusCode = false, StatusCode = 0, Content = ex.Message };
        }
    }

    /// <summary>
    /// Reads response body asynchronously and populates <see cref="ApiResponse"/>.
    /// Replaces the sync <c>ProcessApiResponse</c> which called <c>ReadAsStringAsync().Result</c>.
    /// </summary>
    private static async Task<ApiResponse> BuildApiResponseAsync(
        HttpResponseMessage response, CancellationToken ct)
    {
        var jsonContent = await response.Content.ReadAsStringAsync(ct);

        var result = new ApiResponse
        {
            StatusCode          = (int)response.StatusCode,
            IsSuccessStatusCode = response.IsSuccessStatusCode
        };

        if (response.IsSuccessStatusCode)
        {
            try   { result.Content = JsonConvert.DeserializeObject(jsonContent) ?? jsonContent; }
            catch { result.Content = jsonContent; }
        }
        else
        {
            result.Content = jsonContent;
        }

        return result;
    }
}
