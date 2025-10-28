using Newtonsoft.Json;
using System;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Returned response from the stored procedure
	/// </summary>
	public class RepositoryResponse : IRepositoryResponse
	{
		/// <summary>
		/// User master unique identifier
		/// </summary>
		[JsonProperty(PropertyName = "RealPageId")]
		public Guid RealPageId { get; set; }

		/// <summary>
		/// Id of the updated/Inserted row
		/// </summary>
		[JsonProperty(PropertyName = "Id")]
		public long Id { get; set; }

        /// <summary>
        /// Returns the message text of the error that caused the CATCH block of a TRY…CATCH
        /// </summary>
        [JsonProperty(PropertyName = "ErrorMessage")]
        public string ErrorMessage { get; set; } = "";
	}
}
