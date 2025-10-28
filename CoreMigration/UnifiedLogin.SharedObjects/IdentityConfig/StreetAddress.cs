using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Person StreetAdress
	/// </summary>
	public class StreetAddress : IStreetAddress
	{
		/// <summary>
		/// Contact Contact Mechanism unique Id
		/// </summary>
		[JsonProperty(PropertyName = "ContactMechanismId")]
		public int ContactMechanismId { get; set; }

		/// <summary>
		/// Street Address1
		/// </summary>
		[JsonProperty(PropertyName = "StreetAddress1")]
		public string StreetAddress1 { get; set; }

		/// <summary>
		/// Street Address2
		/// </summary>
		[JsonProperty(PropertyName = "StreetAddress2")]
		public string StreetAddress2 { get; set; }

		/// <summary>
		/// Street Address3
		/// </summary>
		[JsonProperty(PropertyName = "StreetAddress3")]
		public string StreetAddress3 { get; set; }
	}
}
