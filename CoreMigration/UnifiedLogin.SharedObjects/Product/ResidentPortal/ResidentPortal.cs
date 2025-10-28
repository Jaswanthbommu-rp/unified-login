using Newtonsoft.Json;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.ResidentPortal
{
	/// <summary>
	/// Used to grant a user level, set the Messaging groups, and Is the Product assigned or removed for the user.
	/// </summary>
	public class ResidentPortal : IResidentPortal
	{
		/// <summary>
		/// Array of Communities
		/// </summary>
		[JsonProperty("PropertyList", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> PropertyList { get; set; } = null;

		/// <summary>
		/// Groups (Messaging groups) - Format from and to Resident Portal
		/// </summary>
		[JsonProperty("MessageGroups", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> MessageGroups { get; set; } = null;

		/// <summary>
		/// Groups (Messaging groups) - Format from and to UI
		/// </summary>
		[JsonProperty("messagingGroups", NullValueHandling = NullValueHandling.Ignore)]
		public List<IMessagingGroups> MessagingGroups { get; set; } = null;

		/// <summary>
		/// Is product assigned or removed
		/// </summary>
		[JsonProperty(PropertyName = "IsAssigned")]
		public bool IsAssigned { get; set; }

		/// <summary>
		/// level of access
		/// </summary>
		[JsonProperty(PropertyName = "RoleList")]
		public List<string> RoleList { get; set; }

		/// <summary>
		/// level of access object - Format from and to UI
		/// </summary>
		[JsonProperty("levels", NullValueHandling = NullValueHandling.Ignore)]
		public List<ILevel> Levels { get; set; } = null;

		/// <summary>
		/// Staff user notification settings (optional)
		/// </summary>
		[JsonProperty(PropertyName = "Notifications", NullValueHandling = NullValueHandling.Ignore)]
		public Notifications Notifications { get; set; }
	}
}
