using Moq;
using Moq.Protected;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Extensions
{
	[ExcludeFromCodeCoverage]
	public static class MockHttpMessageHandlerExtensions
    {
        public static void Setup(this Mock<HttpMessageHandler> messageHander, HttpMethod method, string url, HttpResponseMessage response)
        {
            messageHander.Protected()
              .Setup<Task<HttpResponseMessage>>(
                  "SendAsync"
                  , ItExpr.Is<HttpRequestMessage>(message =>
                        string.Equals(message.RequestUri.ToString(), url, StringComparison.OrdinalIgnoreCase) && message.Method == method)
                  , ItExpr.IsAny<CancellationToken>()
              )
              .Returns(Task.FromResult(response))
              .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
              {
                  Assert.Equal(method, r.Method);
              });
        }
    }
}
