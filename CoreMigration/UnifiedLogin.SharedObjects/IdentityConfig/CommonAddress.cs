using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Common Electronic and Postal Address attributes
	/// </summary>
	public class CommonAddress : ICommonAddress
	{
		/// <summary>
		/// Party Contact Mechanism unique Id
		/// </summary>
		[JsonProperty(PropertyName = "PartyContactMechanismId")]
		public long PartyContactMechanismId { get; set; }

		/// <summary>
		/// Contact Mechanism unique Id
		/// </summary>
		[JsonProperty(PropertyName = "ContactMechanismId")]
		public int ContactMechanismId { get; set; }

		/// <summary>
		/// Contact AddressString: Electronic address
		/// </summary>
		[JsonProperty(PropertyName = "AddressString")]
		public string AddressString { get; set; }

		/// <summary>
		/// Contact AddressType: Email
		/// </summary>
		[JsonProperty(PropertyName = "AddressType")]
		public string AddressType { get; set; }

		/// <summary>
		/// Contact Mechanism usage type Id
		/// </summary>
		[JsonIgnore]
		public int ContactMechanismUsageTypeId { get; set; }

		/// <summary>
		/// Contact Mechanism usage type detail
		/// </summary>
		[JsonProperty("contactMechanismUsageType", NullValueHandling = NullValueHandling.Ignore)]
		public ContactMechanismUsageType contactMechanismUsageType { get; set; }
	}
}
