using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers
{
    /// <summary>
    /// Used to communicate with external APIs (synchronous wrappers over async calls for backward compatibility).
    /// </summary>
    public class ApiIntegration
    {
        #region Private Members & Ctor

        private readonly HttpClient _client;
        private readonly string _baseUrlAndQuery;
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public ApiIntegration(HttpClient client, string baseUrlAndQuery)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _baseUrlAndQuery = baseUrlAndQuery ?? throw new ArgumentNullException(nameof(baseUrlAndQuery));
        }

        #endregion

        #region Public Methods (sync facade)

        public T? GetEntityFromApi<T>(bool isThrowOnError = true) where T : class
        {
            try
            {
                var response = _client.GetAsync(_baseUrlAndQuery).Result;
                return HandleEntityResponse<T>(response, isThrowOnError);
            }
            catch (Exception) when (!isThrowOnError)
            {
                return null;
            }
        }

        public ApiResponse PostEntity<T>(object? jsonToPost, bool isThrowOnError = true)
        {
            return SendWithBody(HttpMethod.Post, jsonToPost, isThrowOnError);
        }

        public ApiResponse PutEntity<T>(object? jsonToPost, bool isThrowOnError = true) where T : class
        {
            return SendWithBody(HttpMethod.Put, jsonToPost, isThrowOnError);
        }

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

        public ApiResponse PatchEntity<T>(object? jsonToPost, bool isThrowOnError = true) where T : class
        {
            return SendWithBody(new HttpMethod("PATCH"), jsonToPost, isThrowOnError);
        }

        #endregion

        #region Private Helpers

        private ApiResponse SendWithBody(HttpMethod method, object? body, bool isThrowOnError)
        {
            try
            {
                using var request = new HttpRequestMessage(method, _baseUrlAndQuery);
                if (body != null)
                {
                    string json = JsonSerializer.Serialize(body, _jsonOptions);
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

        private static T? HandleEntityResponse<T>(HttpResponseMessage response, bool isThrowOnError) where T : class
        {
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = response.Content.ReadAsStringAsync().Result;
                return JsonSerializer.Deserialize<T>(jsonContent, _jsonOptions);
            }
            else if (isThrowOnError)
            {
                var error = response.Content.ReadAsStringAsync().Result;
                throw new Exception($"Error in GET. Status {(int)response.StatusCode} {response.StatusCode}. Body: {error}");
            }
            return null;
        }

        private static ApiResponse ProcessApiResponse(HttpResponseMessage response)
        {
            var result = new ApiResponse { StatusCode = (int)response.StatusCode, IsSuccessStatusCode = response.IsSuccessStatusCode };
            var jsonContent = response.Content.ReadAsStringAsync().Result;
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    result.Content = JsonSerializer.Deserialize<object>(jsonContent, _jsonOptions) ?? jsonContent;
                }
                catch
                {
                    result.Content = jsonContent; // fallback to raw text
                }
            }
            else
            {
                result.Content = jsonContent;
            }
            return result;
        }

        #endregion
    }

    /// <summary>
    /// Holds information about API response
    /// </summary>
    public class ApiResponse
    {
        public bool IsSuccessStatusCode { get; set; }
        public int StatusCode { get; set; }
        public dynamic? Content { get; set; }
    }
}