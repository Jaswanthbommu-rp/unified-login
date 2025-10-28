using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Telecommunication Number Contact Mechanism
	/// </summary>
	public class TelecommunicationNumber : ITelecommunicationNumber
	{
		/// <summary>
		/// Party Contact Mechanism unique Id
		/// </summary>
		[JsonProperty(PropertyName = "PartyContactMechanismId")]
		public long PartyContactMechanismId { get; set; }

		/// <summary>
		/// Contact Contact Mechanism unique Id
		/// </summary>
		[JsonProperty(PropertyName = "ContactMechanismId")]
		public int ContactMechanismId { get; set; }

		/// <summary>
		/// Country Code
		/// </summary>
		[JsonProperty(PropertyName = "CountryCode")]
		public string CountryCode { get; set; } = "";

		/// <summary>
		/// Area Code
		/// </summary>
		[JsonProperty(PropertyName = "AreaCode")]
		public string AreaCode { get; set; } = "";

		/// <summary>
		/// PhoneNumber
		/// </summary>
		[JsonProperty(PropertyName = "PhoneNumber")]
		public string PhoneNumber { get; set; } = "";

        /// <summary>
        /// IsDeleted
        /// </summary>
        [JsonProperty(PropertyName = "IsDeleted")]
        public bool IsDeleted { get; set; } = false;

		/// <summary>
		/// IsPreferred
		/// </summary>
		[JsonProperty(PropertyName = "IsPreferred")]
		public bool IsPreferred { get; set; } = false;

		/// <summary>
		/// Contact Mechanism usage type Id
		/// </summary>
		[JsonIgnore]
		public int ContactMechanismUsageTypeId { get; set; }

		/// <summary>
		/// ISOCode
		/// </summary>
		[JsonProperty(PropertyName = "ISOCode")]
		public string ISOCode { get; set; } = "";

        /// <summary>
        /// IsDefault
        /// </summary>
        [JsonProperty(PropertyName = "IsDefault")]
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Contact Mechanism usage type detail
        /// </summary>
        [JsonProperty("contactMechanismUsageType", NullValueHandling = NullValueHandling.Ignore)]
		public ContactMechanismUsageType contactMechanismUsageType { get; set; }

		#region Examples
		/// <summary>
		/// Example for linking a Telecommunication Number to a Person method
		/// </summary>
		/// <returns>Newly Created Contact Mechanism Id</returns>
		public static TelecommunicationNumberOutputResult LinkTelecommunicationNumberOutputResultExample()
		{
			TelecommunicationNumberOutputResult result = new TelecommunicationNumberOutputResult();
			result.ContactMechanismId = 1;
			return result;
		}

		/// <summary>
		/// Output result for newly linked telecommunication number to a person
		/// </summary>
		public class TelecommunicationNumberOutputResult
		{
			/// <summary>
			/// Represents the newly linked telecommunication number to a person
			/// </summary>
			public int ContactMechanismId { get; set; }
		}
		#endregion
	}
}
