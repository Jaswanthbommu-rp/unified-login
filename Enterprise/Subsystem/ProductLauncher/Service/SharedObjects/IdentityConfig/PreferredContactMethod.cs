using Newtonsoft.Json;
using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Preferred Contact Method
	/// </summary>
	public class PreferredContactMethod : IPreferredContactMethod
	{
		/// <summary>
		/// Unique Identifier - Preferred Contact Method Id
		/// </summary>
		[JsonProperty(PropertyName = "PreferredContactMethodId")]
		public int PreferredContactMethodId { get; set; }

		/// <summary>
		/// Preferred Contact Method Name
		/// </summary>
		[JsonProperty(PropertyName = "Name")]
		public string Name { get; set; }
	}
}