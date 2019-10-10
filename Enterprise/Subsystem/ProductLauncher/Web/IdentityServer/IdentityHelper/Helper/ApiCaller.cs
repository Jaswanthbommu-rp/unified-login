using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Helper
{
	public static class ApiCaller
	{

		#region Public Methods

		// TODO: Check - http://johnnycode.com/2012/02/23/consuming-your-own-asp-net-web-api-rest-service/

		public static T GetResultFromApi<T>(string apiPathAndQuery, Uri baseUrl, bool throwOnError = true) where T : class
		{
			T results = null;
			using (var client = new HttpClient())
			{

				var apiUrl = baseUrl + apiPathAndQuery;
				AddHeader(client, apiUrl, HttpMethod.Get);
				var response = client.GetAsync(apiUrl).Result;

				if (response.IsSuccessStatusCode)
				{
					var jsonContent = response.Content.ReadAsStringAsync().Result;
					results = JsonConvert.DeserializeObject(jsonContent, typeof(T)) as T;
				}
			}

			return results;
		}

		public static T GetResultFromApi<T>(string token, string apiPathAndQuery, Uri baseUrl, bool throwOnError = true) where T : class
		{
			T results = null;
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
				var apiUrl = baseUrl + apiPathAndQuery;
				AddHeader(client, apiUrl, HttpMethod.Get);
				var response = client.GetAsync(apiUrl).Result;

				if (response.IsSuccessStatusCode)
				{
					var jsonContent = response.Content.ReadAsStringAsync().Result;
					results = JsonConvert.DeserializeObject(jsonContent, typeof(T)) as T;
				}
			}

			return results;
		}

		public static async Task<T> PostApi<T>(T inputObject, Uri baseUrl, string apiPathAndQuery) where T : class
		{
			T results = null;

			using (var client = new HttpClient())
			{
				//var baseUrl = new Uri(ConfigReader.GetIdentityApiUri);  
				var apiUrl = baseUrl + apiPathAndQuery;
				AddHeader(client, apiUrl, HttpMethod.Post);
				var response = await client.PostAsJsonAsync(apiUrl, inputObject);
				if (response.IsSuccessStatusCode)
				{
					var jsonContent = response.Content.ReadAsStringAsync().Result;
					results = JsonConvert.DeserializeObject(jsonContent, typeof(T)) as T;
				}

				return results;
			}
		}
		public static async Task<T> PostApi<T>(T inputObject, string apiPathAndQuery) where T : class
		{
			T results = null;

			using (var client = new HttpClient())
			{
				var baseUrl = new Uri(ConfigReader.GetIdentityApiUri);
				var apiUrl = baseUrl + apiPathAndQuery;
				AddHeader(client, apiUrl, HttpMethod.Post);
				var response = await client.PostAsJsonAsync(apiUrl, inputObject);
				if (response.IsSuccessStatusCode)
				{
					var jsonContent = response.Content.ReadAsStringAsync().Result;
					results = JsonConvert.DeserializeObject(jsonContent, typeof(T)) as T;
				}

				return results;
			}
		}

		public static Task<T> PostApiNotAsync<T>(T inputObject, string apiPathAndQuery) where T : class
		{
			T results = null;

			using (var client = new HttpClient())
			{
				var baseUrl = new Uri(ConfigReader.GetIdentityApiUri);
				var apiUrl = baseUrl + apiPathAndQuery;
				AddHeader(client, apiUrl, HttpMethod.Post);
				var response = client.PostAsJsonAsync(apiUrl, inputObject).Result;
				if (response.IsSuccessStatusCode)
				{
					var jsonContent = response.Content.ReadAsStringAsync().Result;
					results = JsonConvert.DeserializeObject(jsonContent, typeof(T)) as T;
				}

				return Task.FromResult(results);
			}
		}

		public static async Task<T> PostApi<T, TK>(TK inputObject, string apiPathAndQuery) where T : class
		{
			T results = null;

			using (var client = new HttpClient())
			{
				var baseUrl = new Uri(ConfigReader.GetIdentityApiUri); ;
				var apiUrl = baseUrl + apiPathAndQuery;
				AddHeader(client, apiUrl, HttpMethod.Post);
				var response = await client.PostAsJsonAsync(apiUrl, inputObject);
				if (response.IsSuccessStatusCode)
				{
					var jsonContent = await response.Content.ReadAsStringAsync();
					results = JsonConvert.DeserializeObject(jsonContent, typeof(T)) as T;
				}

				return results;
			}
		}

		#endregion

		/// <summary>
		/// Used to add authentication header to api requests
		/// </summary>
		/// <param name="client"></param>
		/// <param name="apiUrl"></param>
		/// <param name="method"></param>
		private static void AddHeader(HttpClient client, string apiUrl, HttpMethod method)
		{
			string requestContentBase64String = string.Empty;

			// exclude the protocol because it can change when behind F5
			Uri api = new Uri(apiUrl);
			string requestUri = System.Web.HttpUtility.UrlEncode((api.Host + api.PathAndQuery).ToLower());

			string requestHttpMethod = method.Method;

			//Calculate UNIX time
			DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
			TimeSpan timeSpan = DateTime.UtcNow - epochStart;
			string requestTimeStamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

			//create random nonce for each request
			string nonce = Guid.NewGuid().ToString("N");

			//Checking if the request contains body, usually will be null wiht HTTP GET and DELETE

			//if (request.Content != null)
			//{
			byte[] content = Encoding.ASCII.GetBytes(ConfigReader.IdentityAPIId); //request.Content.ReadAsByteArrayAsync();
			MD5 md5 = MD5.Create();
			//Hashing the request body, any change in request body will result in different hash, we'll incure message integrity
			byte[] requestContentHash = md5.ComputeHash(content);
			requestContentBase64String = Convert.ToBase64String(requestContentHash);
			//}
			//*/

			//Creating the raw signature string
			string signatureRawData = String.Format("{0}{1}{2}{3}{4}{5}", ConfigReader.IdentityAPIId, requestHttpMethod, requestUri, requestTimeStamp, nonce, requestContentBase64String);

			var secretKeyByteArray = Convert.FromBase64String(ConfigReader.IdentityAPIKey);

			byte[] signature = Encoding.UTF8.GetBytes(signatureRawData);

			using (HMACSHA256 hmac = new HMACSHA256(secretKeyByteArray))
			{
				byte[] signatureBytes = hmac.ComputeHash(signature);
				string requestSignatureBase64String = Convert.ToBase64String(signatureBytes);
				//Setting the values in the Authorization header using custom scheme (amx)
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("amx", string.Format("{0}:{1}:{2}:{3}", ConfigReader.IdentityAPIId, requestSignatureBase64String, nonce, requestTimeStamp));
			}
		}
	}
}
