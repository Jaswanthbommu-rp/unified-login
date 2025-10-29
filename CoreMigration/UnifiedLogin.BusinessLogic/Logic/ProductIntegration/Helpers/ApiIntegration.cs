using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using Newtonsoft.Json;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers
{
    /// <summary>
    /// Used to communicate with external APIs
    /// </summary>
    public class ApiIntegration
    {
        #region Private Members & Ctor

        private readonly HttpClient _client;
        private readonly string _baseUrlAndQuery;

        /// <summary>
        /// Ctor
        /// </summary> 
        public ApiIntegration(HttpClient client, string baseUrlAndQuery)
        {
            _client = client;
            _baseUrlAndQuery = baseUrlAndQuery;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Call GET API
        /// </summary>
        public T GetEntityFromApi<T>(bool isThrowOnError = true)
            where T : class
        {
            T results = null;

            try
            {
                var response = _client.GetAsync(_baseUrlAndQuery).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    results = JsonConvert.DeserializeObject(jsonContent, typeof(T)) as T;
                }
                else
                {
                    if (isThrowOnError)
                        throw new Exception($"Error in Get response code - {response.StatusCode}, {response.Content}");
                }
            }
            catch (Exception ex)
            {
                if (isThrowOnError)
                    throw;
            }

            return results;
        }

        /// <summary>
        /// Post API
        /// </summary>
        public ApiResponse PostEntity<T>(object jsonToPost, bool isThrowOnError = true)
        {
            var result = new ApiResponse();

            try
            {
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = _client.PostAsJsonAsync(_baseUrlAndQuery, jsonToPost).Result;
                ProcessApiResponse(result, response);
            }
            catch (Exception)
            {
                if (isThrowOnError)
                    throw;
            }

            return result;
        }

        /// <summary>
        /// Put API
        /// </summary>
        public ApiResponse PutEntity<T>(object jsonToPost, bool isThrowOnError = true) where T : class
        {
            var result = new ApiResponse();

            try
            {
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = _client.PutAsJsonAsync(_baseUrlAndQuery, jsonToPost).Result;
                ProcessApiResponse(result, response);
            }
            catch (Exception)
            {
                if (isThrowOnError)
                    throw;
            }

            return result;
        }

        /// <summary>
        /// Delete API
        /// </summary>
        public ApiResponse DeleteEntity<T>(bool isThrowOnError = true) where T : class
        {
            var result = new ApiResponse();
            try
            {
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = _client.DeleteAsync(_baseUrlAndQuery).Result;
                ProcessApiResponse(result, response);
            }
            catch (Exception)
            {
                if (isThrowOnError)
                    throw;
            }

            return result;
        }


        public ApiResponse PatchEntity<T>(object jsonToPost, bool isThrowOnError = true) where T : class
        {
            var result = new ApiResponse();
            try
            {
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var requestContent = new ObjectContent<dynamic>(jsonToPost, new JsonMediaTypeFormatter());
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), _baseUrlAndQuery) { Content = requestContent };

                var response = _client.SendAsync(request).Result;
                ProcessApiResponse(result, response);
            }
            catch (Exception)
            {
                if (isThrowOnError)
                    throw;
            }

            return result;
        }

        #endregion

        #region Private Methods

        private void ProcessApiResponse(ApiResponse result, HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                string jsonContent = response.Content.ReadAsStringAsync().Result;
                dynamic userResult = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                if (userResult != null)
                {
                    result.Content = userResult;
                }

                result.IsSuccessStatusCode = true;
            }
            else
            {
                var jsonContent = response.Content.ReadAsStringAsync().Result;
                result.Content = jsonContent;
            }

            result.StatusCode = (int)response.StatusCode;
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
        public dynamic Content { get; set; }
    }
}