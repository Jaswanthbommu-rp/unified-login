using Moq;
using Moq.Protected;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnifiedLogin.LandingAPI.Test.Extensions
{
	[ExcludeFromCodeCoverage]
	public static class MockHttpMessageHandlerExtensions
    {
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
        

        private static bool ValidateRequest(HttpRequestMessage message, string url, HttpMethod method)
        {
            return url.Equals(message.RequestUri.ToString(), StringComparison.OrdinalIgnoreCase) && message.Method == method;
        }

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
