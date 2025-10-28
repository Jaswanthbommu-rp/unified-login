using Newtonsoft.Json;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// User Profile
	/// </summary>
	public class Profile : Person, IProfile
	{
		/// <summary>
		/// UserLogin attributes
		/// </summary>
		[JsonProperty("userLogin", NullValueHandling = NullValueHandling.Ignore)]
		public IUserLogin userLogin { get; set; } = new UserLogin();

		/// <summary>
		/// Contact mechanisim telecommunication number attributes
		/// </summary>
		[JsonProperty(PropertyName = "telecommunicationNumber")]
		public IList<TelecommunicationNumber> TelecommunicationNumber { get; set; }

        /// <summary>
		/// Contact mechanisim Electronic attributes
		/// </summary>
		[JsonProperty(PropertyName = "emailContacts")]
        public IList<ElectronicAddress> EmailContacts { get; set; }

        /// <summary>
        /// PartyRole (e.g. User Job Title) attributes
        /// </summary>
        [JsonProperty("partyRole", NullValueHandling = NullValueHandling.Ignore)]
		public PartyRole PartyRole { get; set; }

        /// <summary>
		/// Impersonate Profile Edit
		/// </summary>
        [JsonProperty(PropertyName = "IsImpersonated")]
        public bool IsImpersonated { get; set; } = false;
    }
}
