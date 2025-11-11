using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers
{
    /// <summary>
    /// Extension methods for HttpClient to support authentication headers
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Sets the Basic authentication header for the HTTP client
        /// </summary>
        /// <param name="client">The HttpClient instance</param>
        /// <param name="username">The username for basic authentication</param>
        /// <param name="password">The password for basic authentication</param>
        public static void SetBasicAuthentication(this HttpClient client, string username, string password)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            var credentials = $"{username}:{password}";
            var encodedCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
        }

        /// <summary>
        /// Sets the Bearer token authentication header for the HTTP client
        /// </summary>
        /// <param name="client">The HttpClient instance</param>
        /// <param name="token">The bearer token</param>
        public static void SetBearerToken(this HttpClient client, string token)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be null or empty", nameof(token));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
