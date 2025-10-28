using Newtonsoft.Json;
using System;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Party Contact Mechanism
	/// </summary>
	public class PartyContactMechanism : IPartyContactMechanism
	{
		/// <summary>
		/// Party Contact Contact Mechanism unique Id
		/// </summary>
		[JsonProperty(PropertyName = "PartyContactMechanismId")]
		public long PartyContactMechanismId { get; set; }

		/// <summary>
		/// PartyId
		/// </summary>
		[JsonProperty(PropertyName = "PartyId")]
		public long PartyId { get; set; }

		/// <summary>
		/// Contact Mechanism unique Id
		/// </summary>
		[JsonProperty(PropertyName = "ContactMechanismId")]
		public int ContactMechanismId { get; set; }

        /// <summary>
        /// Contact Mechanism From Date
        /// </summary>
        [JsonProperty(PropertyName = "FromDate")]
        public DateTime? FromDate { get; set; } 

		/// <summary>
		/// Contact Mechanism thru Date
		/// </summary>
		[JsonProperty(PropertyName = "ThruDate")]
		public DateTime? ThruDate { get; set; }
	}
}
