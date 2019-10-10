using Newtonsoft.Json;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal
{
	/// <summary>
	/// Messaging groups
	/// </summary>
	public class MessagingGroups : IMessagingGroups
	{
		/// <summary>
		/// Messaging group Id
		/// </summary>
		[JsonProperty(PropertyName = "Id")]
		public string Id { get; set; }

		/// <summary>
		/// Messaging group Name
		/// </summary>
		[JsonProperty(PropertyName = "Name")]
		public string Name { get; set; }

		/// <summary>
		/// Is a Messaging group selected
		/// </summary>
		[JsonProperty(PropertyName = "IsAssigned")]
		public bool IsAssigned { get; set; } = false;
	}
}
