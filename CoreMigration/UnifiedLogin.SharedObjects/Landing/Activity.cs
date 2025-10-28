using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// User activity
    /// </summary>
    public class Activity
	{
		/// <summary>
		/// Unique Portfolio ID
		/// </summary>
		public long PartyId { get; set; }

		/// <summary>
		/// Activity ConfigurationId
		/// </summary>
		public int ActivityConfigurationId { get; set; }

		/// <summary>
		/// Activity TypeId
		/// </summary>
		public int ActivityTypeId { get; set; }

		/// <summary>
		/// ActivityCode
		/// </summary>
		public string ActivityCode { get; set; }

		/// <summary>
		/// Description
		/// </summary>
        public string Description { get; set; }

		/// <summary>
		/// Max Activity Attempt Count
		/// </summary>
        public int MaxActivityAttemptCount { get; set; }

		/// <summary>
		/// Activity Token Expiration Minutes
		/// </summary>
        public int ActivityTokenExpirationMinutes { get; set; }

		/// <summary>
		/// Activity Token Expiration Days - SQL Calculated property: ActivityTokenExpirationMinutes / 60 / 24
		/// </summary>
		[JsonProperty(PropertyName = "ActivityTokenExpirationDays", NullValueHandling = NullValueHandling.Ignore)]
		public int ActivityTokenExpirationDays { get; set; }
	}
}
