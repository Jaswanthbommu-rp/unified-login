using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Native-async HTTP integration helper contract.
/// Replaces <see cref="ApiIntegration"/> — all blocking <c>.Result</c> calls eliminated.
/// <para>
/// <list type="bullet">
///   <item>HTTP client lifetime managed by <c>IHttpClientFactory</c>; avoids socket exhaustion.</item>
///   <item>Authentication header is applied once per factory-created client instance.</item>
///   <item><c>CancellationToken ct = default</c> propagated through every HTTP call.</item>
///   <item><c>T?</c> nullable return on <c>GetEntityFromApiAsync</c> — returns <c>null</c>
///     when <paramref name="isThrowOnError"/> is <c>false</c> and the request fails.</item>
/// </list>
/// </para>
/// </summary>
public interface IApiIntegrationAsync
{
    /// <summary>
    /// Issues a GET and deserializes the response body to <typeparamref name="T"/>.
    /// Returns <c>null</c> on a non-success status when <paramref name="isThrowOnError"/>
    /// is <c>false</c>; throws <see cref="HttpRequestException"/> otherwise.
    /// </summary>
    Task<T?> GetEntityFromApiAsync<T>(
        bool isThrowOnError = true,
        CancellationToken ct = default) where T : class;

    /// <summary>Issues a POST with <paramref name="jsonToPost"/> serialized as JSON.</summary>
    Task<ApiResponse> PostEntityAsync<T>(
        object jsonToPost,
        bool isThrowOnError = true,
        CancellationToken ct = default);

    /// <summary>Issues a PUT with <paramref name="jsonToPost"/> serialized as JSON.</summary>
    Task<ApiResponse> PutEntityAsync<T>(
        object jsonToPost,
        bool isThrowOnError = true,
        CancellationToken ct = default) where T : class;

    /// <summary>Issues a DELETE to the configured URL.</summary>
    Task<ApiResponse> DeleteEntityAsync<T>(
        bool isThrowOnError = true,
        CancellationToken ct = default) where T : class;

    /// <summary>Issues a PATCH with <paramref name="jsonToPost"/> serialized as JSON.</summary>
    Task<ApiResponse> PatchEntityAsync<T>(
        object jsonToPost,
        bool isThrowOnError = true,
        CancellationToken ct = default) where T : class;
}
