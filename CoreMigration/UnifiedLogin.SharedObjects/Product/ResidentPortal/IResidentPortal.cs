using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.ResidentPortal
{
	/// <summary>
	/// Interface for ResidentPortal
	/// </summary>
	public interface IResidentPortal
	{
		/// <summary>
		/// Array of Communities
		/// </summary>
		List<string> PropertyList { get; set; }

		/// <summary>
		/// Groups (Messaging groups) - Format from and to Resident Portal
		/// </summary>
		List<string> MessageGroups { get; set; }

		/// <summary>
		/// Groups (Messaging groups) - Format from and to UI
		/// </summary>
		List<IMessagingGroups> MessagingGroups { get; set; }

		/// <summary>
		/// Is product assigned or removed
		/// </summary>
		bool IsAssigned { get; set; }

		/// <summary>
		/// level of access
		/// </summary>
		List<string> RoleList { get; set; }

		/// <summary>
		/// level of access object - Format from and to UI
		/// </summary>
		List<ILevel> Levels { get; set; }

		/// <summary>
		/// Staff user notification settings (optional)
		/// </summary>
		Notifications Notifications { get; set; }
	}
}