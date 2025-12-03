using Moq;
using Moq.Protected;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Extensions
{
    /// <summary>
    /// Extension methods for mocking HttpMessageHandler in unit tests.
    /// Migrated from RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Extensions.MockHttpMessageHandlerExtensions
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class MockHttpMessageHandlerExtensions
    {
        /// <summary>
        /// Sets up a mock HttpMessageHandler to return a specific response for a given HTTP method and URL.
        /// </summary>
        /// <param name="messageHandler">The mock message handler to configure</param>
        /// <param name="method">The HTTP method to match</param>
        /// <param name="url">The URL to match</param>
        /// <param name="response">The HTTP response to return</param>
        public static void Setup(this Mock<HttpMessageHandler> messageHandler, HttpMethod method, string url, HttpResponseMessage response)
        {
            messageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync"
                    , ItExpr.Is<HttpRequestMessage>(message =>
                        url.Equals(message.RequestUri.ToString(), StringComparison.OrdinalIgnoreCase) && message.Method == method)
                    , ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(() =>
                {
                    return response;
                })
                .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
                {
                    Assert.Equal(method, r.Method);
                });
        }

        /// <summary>
        /// Validates that an HTTP request matches the expected URL and method.
        /// </summary>
        /// <param name="message">The HTTP request message to validate</param>
        /// <param name="url">The expected URL</param>
        /// <param name="method">The expected HTTP method</param>
        /// <returns>True if the request matches the expected URL and method</returns>
        private static bool ValidateRequest(HttpRequestMessage message, string url, HttpMethod method)
        {
            return url.Equals(message.RequestUri.ToString(), StringComparison.OrdinalIgnoreCase) && message.Method == method;
        }

        /// <summary>
        /// Sets up a mock HttpMessageHandler to handle PATCH requests.
        /// </summary>
        /// <param name="messageHandler">The mock message handler to configure</param>
        /// <param name="url">The URL to match</param>
        /// <param name="response">The HTTP response to return</param>
        public static void SetupPatch(this Mock<HttpMessageHandler> messageHandler, string url, HttpResponseMessage response)
        {
            messageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync"
                    , ItExpr.Is<HttpRequestMessage>(message =>
                        string.Equals(message.RequestUri.ToString(), url, StringComparison.OrdinalIgnoreCase) && message.Method.ToString() == "PATCH")
                    , ItExpr.IsAny<CancellationToken>()
                )
                .Returns(Task.FromResult(response))
                .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
                {
                    Assert.Equal("PATCH", r.Method.ToString());
                });
        }
    }
}
