using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Used as the input object to link a telecommunication number to a person
	/// </summary>
	public class LinkTelecommunicationNumber : ILinkTelecommunicationNumber
	{
		/// <summary>
		/// Party Contact Mechanism
		/// </summary>
		[JsonProperty(PropertyName = "PartyContactMechanism")]
		public PartyContactMechanism PartyContactMechanism { get; set; }

		/// <summary>
		/// Contact Mechanism UsageType
		/// </summary>
		[JsonProperty(PropertyName = "ContactMechanismUsageType")]
		public ContactMechanismUsageType ContactMechanismUsageType { get; set; }

		/// <summary>
		/// Telecommunication Number
		/// </summary>
		[JsonProperty(PropertyName = "TelecommunicationNumber")]
		public TelecommunicationNumber TelecommunicationNumber { get; set; }
	}
}
