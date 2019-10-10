using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Helper
{
    public static class ApiCaller
    {
        #region Public Methods

        public static async Task<T> PostApi<T, TK>(TK inputObject, string apiPathAndQuery) where T : class
        {
            T results = null;
			
            using (var client = new HttpClient())
            {
                var response = await client.PostAsJsonAsync(apiPathAndQuery, inputObject);

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
