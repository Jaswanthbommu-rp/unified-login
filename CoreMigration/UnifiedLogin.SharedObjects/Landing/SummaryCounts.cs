using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Summary count of properties, products and roles associated to user
	/// </summary>
	public class SummaryCounts
    {
		/// <summary>
		/// Count of properties associated to user
		/// </summary>
		[JsonProperty(PropertyName = "properties")]
        public int TotalAssignedProperties { get; set; } = 0;

        /// <summary>
        /// Count of products associated to user
        /// </summary>
        [JsonProperty(PropertyName = "products")]
        public int TotalAssignedProducts { get; set; } = 0;

		/// <summary>
		/// Count of roles associated to user
		/// </summary>
		[JsonProperty(PropertyName = "roles")]
        public int TotalAssignedRoles { get; set; } = 0;
    }
}