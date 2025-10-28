using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Used as the input object to link a electronic address to a person
	/// </summary>
	public class LinkElectronicAddress : ILinkElectronicAddress
	{
        /// <summary>
        /// Party Contact Mechanism
        /// </summary>
        [JsonProperty(PropertyName = "PartyContactMechanism")]
        public PartyContactMechanism PartyContactMechanism { get; set; } = new PartyContactMechanism();

        /// <summary>
        /// Contact Mechanism UsageType
        /// </summary>
        [JsonProperty(PropertyName = "ContactMechanismUsageType")]
        public ContactMechanismUsageType ContactMechanismUsageType { get; set; } = new ContactMechanismUsageType();

        /// <summary>
        /// Electronic Address
        /// </summary>
        [JsonProperty(PropertyName = "ElectronicAddress")]
        public ElectronicAddress ElectronicAddress { get; set; } = new ElectronicAddress();
	}
}
