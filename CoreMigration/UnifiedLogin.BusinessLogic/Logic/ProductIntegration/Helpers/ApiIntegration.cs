using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using System.Text;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers
{
    /// <summary>
    /// Used to communicate with external APIs.
    /// Uses Newtonsoft.Json exclusively — consistent with the rest of the codebase.
    /// </summary>
    public class ApiIntegration
    {
        #region Private Members & Ctor

        private readonly HttpClient _client;
        private readonly string _baseUrlAndQuery;

        // DefaultContractResolver respects [JsonProperty] attribute names exactly as written.
        // Used for BOTH serialize and deserialize so product API field names are honoured.
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            ContractResolver        = new DefaultContractResolver(),
            NullValueHandling       = NullValueHandling.Ignore,
            MissingMemberHandling   = MissingMemberHandling.Ignore,
            ReferenceLoopHandling   = ReferenceLoopHandling.Ignore
        };

        public ApiIntegration(HttpClient client, string baseUrlAndQuery)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _baseUrlAndQuery = baseUrlAndQuery ?? throw new ArgumentNullException(nameof(baseUrlAndQuery));
        }

        #endregion

        #region Public Methods

        public T GetEntityFromApi<T>(bool isThrowOnError = true) where T : class
        {
            try
            {
                var response = _client.GetAsync(_baseUrlAndQuery).Result;

                if (!response.IsSuccessStatusCode && !isThrowOnError)
                    return null;

                response.EnsureSuccessStatusCode();

                var content = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<T>(content, _settings);
            }
            catch (JsonException ex)
            {
                if (isThrowOnError)
                    throw new Exception($"Failed to deserialize response from {_baseUrlAndQuery}: {ex.Message}", ex);
                return null;
            }
        }

        public ApiResponse PostEntity<T>(object jsonToPost, bool isThrowOnError = true)
            => SendWithBody(HttpMethod.Post, jsonToPost, isThrowOnError);

        public ApiResponse PutEntity<T>(object jsonToPost, bool isThrowOnError = true) where T : class
            => SendWithBody(HttpMethod.Put, jsonToPost, isThrowOnError);

        public ApiResponse DeleteEntity<T>(bool isThrowOnError = true) where T : class
        {
            try
            {
                var response = _client.DeleteAsync(_baseUrlAndQuery).Result;
                return ProcessApiResponse(response);
            }
            catch (Exception) when (!isThrowOnError)
            {
                return new ApiResponse { IsSuccessStatusCode = false, StatusCode = 0, Content = "Delete request failed" };
            }
        }

        public ApiResponse PatchEntity<T>(object jsonToPost, bool isThrowOnError = true) where T : class
            => SendWithBody(new HttpMethod("PATCH"), jsonToPost, isThrowOnError);

        #endregion

        #region Private Helpers

        private ApiResponse SendWithBody(HttpMethod method, object body, bool isThrowOnError)
        {
            try
            {
                using var request = new HttpRequestMessage(method, _baseUrlAndQuery);
                if (body != null)
                {
                    var json = JsonConvert.SerializeObject(body, _settings);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
                var response = _client.SendAsync(request).Result;
                return ProcessApiResponse(response);
            }
            catch (Exception ex) when (!isThrowOnError)
            {
                return new ApiResponse { IsSuccessStatusCode = false, StatusCode = 0, Content = ex.Message };
            }
        }

        private static ApiResponse ProcessApiResponse(HttpResponseMessage response)
        {
            var result = new ApiResponse
            {
                StatusCode         = (int)response.StatusCode,
                IsSuccessStatusCode = response.IsSuccessStatusCode
            };

            var jsonContent = response.Content.ReadAsStringAsync().Result;

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

        #endregion
    }

    public class ApiResponse
    {
        public bool IsSuccessStatusCode { get; set; }
        public int StatusCode { get; set; }
        public dynamic Content { get; set; }
    }
}