using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Contact Mechanism UsageType
	/// </summary>
	public class ContactMechanismUsageType : IContactMechanismUsageType
	{
		/// <summary>
		/// Contact Mechanism UsageTypeID
		/// </summary>
		[JsonProperty(PropertyName = "ContactMechanismUsageTypeId")]
		public int? ContactMechanismUsageTypeId { get; set; } = null;

		/// <summary>
		/// Contact Mechanism UsageTypeID
		/// </summary>
		[JsonProperty(PropertyName = "ParentContactMechanismUsageTypeId")]
		public int ParentContactMechanismUsageTypeId { get; set; }

		/// <summary>
		/// Contact Mechanism UsageType name
		/// </summary>
		[JsonProperty(PropertyName = "Name")]
		public string Name { get; set; } = "";
	}
}
