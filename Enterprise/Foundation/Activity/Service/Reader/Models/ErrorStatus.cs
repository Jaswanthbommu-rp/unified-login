using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Models
{
	/// <summary>
	/// Error status object - API/UI Call Success/Error Communication 
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class Status<T> : IStatus<T>
	{
		/// <summary>
		/// Error Success Status
		/// </summary>
		[JsonProperty(PropertyName = "Success")]
		public bool Success { get; set; } = true;

		/// <summary>
		/// Error Code (Major.Minor: Major = Controller and Minor = API verb
		/// </summary>
		[JsonProperty(PropertyName = "ErrorCode")]
		public string ErrorCode { get; set; } = "";

		/// <summary>
		/// Technical Error Message
		/// </summary>
		[JsonProperty(PropertyName = "ErrorMsg")]
		public string ErrorMsg { get; set; } = "";

		/// <summary>
		/// Details about the number of rows affected or read by the query
		/// </summary>
		[JsonProperty(PropertyName = "ErrorData", NullValueHandling = NullValueHandling.Ignore)]
		public T ErrorData { get; set; }
	}
}
