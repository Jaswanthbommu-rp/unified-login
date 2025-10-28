using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Helper
{
	/// <summary>
	/// Validate a Json 
	/// </summary>
	public class ValidateJson
	{
		/// <summary>
		/// Validate Json string
		/// </summary>
		/// <typeparam name="T">Generic Object</typeparam>
		/// <param name="json">Json string to Validate</param>
		/// <returns>Boolean</returns>
		public static bool IsValidJson<T>(string json)
		{
			bool IsValidJson = true;

			var settings = new JsonSerializerSettings
			{
				Error = (sender, args) => { IsValidJson = false; args.ErrorContext.Handled = true; },
				MissingMemberHandling = MissingMemberHandling.Error
			};
			T result = JsonConvert.DeserializeObject<T>(json, settings);

			return IsValidJson;
		}
	}
}
