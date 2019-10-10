using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityGBDisableUser.Helper
{
	public static class ApiCaller
	{
		#region Public Methods

		public static async Task<T> PostApi<T, TK>(TK inputObject, string apiPathAndQuery) where T : class
		{
			T results = null;

			using (var client = new HttpClient())
			{
				var baseUrl = new Uri(ConfigReader.GetLandingApiUri);
				var apiUrl = baseUrl + apiPathAndQuery;
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
	}
}
